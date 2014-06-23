using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http.ModelBinding;
using HtmlAgilityPack;
using Microsoft.Ajax.Utilities;
using VideoLinks.Models;
using VideoLinks.Helpers;
using System.Threading.Tasks;
using VideoLinks.Repositories;

namespace VideoLinks.Scraper
{
    public class PrimeWireScraper
    {

        private const string primeWireUrl = "http://www.primewire.ag";

        public bool DownloadAllMovies(bool redoIfFound, int numberOfPages = 0)
        {
            var videoEntities = new VideosEntities();
            var downLoadProgressRepository = new DownLoadProgressRepository(videoEntities);

            var currentPage = 0;
            var lastPage = 0;
            var result = false;

            if (downLoadProgressRepository.Items.Any())
            {
                lastPage = (int)downLoadProgressRepository.Items.FirstOrDefault().LastPage;
            }

            try
            {
                var totalpages = (numberOfPages == 0) ? GetTotalPages("http://www.primewire.ag/index.php?sort=featured&page=" + lastPage) : numberOfPages;

                for (int i = lastPage; i <= totalpages; i++)
                {
                    currentPage = i;
                    if (numberOfPages > 0)
                        numberOfPages -= 1;

                    Debug.WriteLine("SCRAPE PAGE {0} - {1}", i, DateTime.Now);

                    var nextpage = "http://www.primewire.ag/index.php?sort=featured&page=" + i;

                    var task = new Task(() => ScrapePageAsync(nextpage, redoIfFound));
                    TaskDelay();
                    task.Start();

                    downLoadProgressRepository.UpdateLastPage(i + 1);
                    downLoadProgressRepository.SaveChanges();
                }
                result = true;
            }
            catch (Exception e)
            {
                //Skip Page and reTry
                Debug.WriteLine(
                   "________________START OVER______________________{0}", e);

                downLoadProgressRepository.AddSkipedPage(currentPage);
                downLoadProgressRepository.UpdateLastPage(lastPage + 1);
                downLoadProgressRepository.SaveChanges();

                DownloadAllMovies(redoIfFound, numberOfPages);
            }

            return result;
        }


        public void RefetchNoLinkVideos(int maxVideos = 0)
        {
            var videoEntities = new VideosEntities();
            var videoRepository = new Repository<Video, VideosEntities>(videoEntities);

            var allTask = new List<Task>();
            var allVideos = videoRepository.Items.Where(x => !x.Links.Any());

            if (maxVideos > 0)
                allVideos.Take(maxVideos);

            foreach (var video in allVideos)
            {
                var task = new Task(() => RefreshVideoLinks(video));
                TaskDelay();
                task.Start();
                allTask.Add(task);
            }
            Task.WaitAll(allTask.ToArray());
        }


        //Todo: Requires a seperate Video Parser.
        public bool DownloadAllTvShows(bool redoIfFound, int numberOfPages = 0)
        {
            var totalpages = (numberOfPages == 0) ? GetTotalPages("http://www.primewire.ag/index.php?tv=&page=1") : numberOfPages;
            for (int i = 0; i <= totalpages; i++)
            {
                var nextpage = "http://www.primewire.ag/index.php?tv=&page=" + i;
                ScrapePageAsync(nextpage, redoIfFound);
            }

            return true;
        }


        public Video DownloadTvShow(string url, bool redoIfFound)
        {
            return ParseVideoPage(url, true, redoIfFound);
        }


        public Video DownloadMovie(string url, bool redoIfFound)
        {
            return ParseVideoPage(url, false, redoIfFound);
        }


        public void RefreshTvShowLinks(Video video)
        {
            var document = new MyWebClient().GetHtmlDocument(video.Link);
            if (document != null)
                foreach (var episode in document.DocumentNode.SelectNodes("//div[@class='tv_episode_item']/a"))
                {
                    var episodeLink = episode.AttributeValue("href");
                    try
                    {
                        //RefreshVideoLinks(episodeLink);
                    }
                    catch (Exception e)
                    {
                        //Debug.WriteLine("!!!!!!URL FAILED----{0}----because {1}", url, e);
                    }
                }
        }


        public void RefreshVideoLinks(Video video)
        {
            if (video == null)
            {
                return;
            }
            var videoEntities = new VideosEntities();
            var linkRepository = new Repository<Link, VideosEntities>(videoEntities);
            var hostRepository = new Repository<Host, VideosEntities>(videoEntities);

            var document = new MyWebClient().GetHtmlDocument(video.Link);
            var xPath = "//table[contains(@class,'movie_version')]/tbody/tr/td/span[contains(@class,'quality_')]";
            var movieLinks = document.DocumentNode.SelectNodes(xPath);

            //delete all existing Links
            if (video.Links != null)
            {
                foreach (Link link in video.Links)
                {
                    linkRepository.Remove(link);
                }
            }

            try
            {
                foreach (HtmlNode link in movieLinks)
                {
                    var newLink = new Link();

                    var linkTableRow = link.ParentNode.ParentNode;
                    var linkUrl = linkTableRow.SelectSingleNode("td[2]/span/a").AttributeValue("href");
                    var quality = linkTableRow.SelectSingleNode("td[1]/span").AttributeValue("class");
                    var host = linkTableRow.SelectSingleNode("td[3]/span").InnerTextValue();

                    //Load Link URL and retrieve redirected url
                    var urlDoc = new MyWebClient().GetHtmlDocument("http://www.primewire.ag/" + linkUrl);
                    var redirectedUrl = urlDoc.ResponseUri;

                    //Extract HostName from javascript statment
                    Match match = Regex.Match(host, @"'([^']*)");
                    if (match.Success)
                    {
                        host = match.Groups[1].Value;
                    }

                    var newHost = hostRepository.Items.FirstOrDefault(x => x.Name == host);
                    if (newHost == null)
                    {
                        newHost = new Host { Name = host };
                        hostRepository.AddItem(newHost);
                        hostRepository.SaveChanges();
                    }

                    newLink.Quality = quality.Replace("quality_", "");
                    newLink.Host = newHost;
                    newLink.URL = redirectedUrl;
                    newLink.Video = video;

                    linkRepository.AddItem(newLink);
                    linkRepository.SaveChanges();

                    Debug.WriteLine("LINK:{0} - {1})", video.Name, DateTime.Now);
                    TaskDelay();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("!!!!!!URL FAILED----{0}----because {1}", video.Link, e);
            }
        }

        private void TaskDelay()
        {
            Task.Delay(new Random().Next(1000, 7000));
        }

        #region Helpers

        private List<Video> ScrapePageAsync(string url, bool redoIfFound)
        {


            var document = new MyWebClient().GetHtmlDocument(url);
            var allTask = new List<Task>();
            var allVideos = new List<Video>();

            if (document != null)
                foreach (var video in document.DocumentNode.SelectNodes("//div[@class='index_item index_item_ie']"))
                {
                    var videoLink = string.Empty;
                    try
                    {
                        videoLink = primeWireUrl + video.FirstChild.AttributeValue("href");
                        var task = new Task<Video>(() => ParseVideoPage(videoLink, false, redoIfFound));
                        TaskDelay();
                        task.Start();
                        allTask.Add(task);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("!!!!!!Scrape FAILED----{0}----because {1}", videoLink, e);
                    }
                }
            Task.WaitAll(allTask.ToArray());

            foreach (Task<Video> task in allTask)
            {
                allVideos.Add(task.Result);
            }

            return allVideos;
        }


        private void ParseAllEpisodes(string firstPage, bool redoIfFound, bool allPages = true)
        {
            var document = new MyWebClient().GetHtmlDocument(firstPage);
            var allTask = new List<Task>();
            var allVideos = new List<Video>();

            if (document != null)
                foreach (var episode in document.DocumentNode.SelectNodes("//div[@class='tv_episode_item']/a"))
                {
                    var episodeLink = string.Empty;
                    try
                    {
                        episodeLink = episode.AttributeValue("href");
                        var task = new Task<Video>(() => ParseVideoPage(episodeLink, true, redoIfFound));
                        task.Start();
                        allTask.Add(task);
                        allVideos.Add(task.Result);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("!!!!!!Scrape FAILED----{0}----because {1}", episodeLink, e);

                    }
                }
        }


        private Video ParseVideoPage(string url, bool isTvShow, bool redoIfFound)
        {
            var videoEntities = new VideosEntities();
            var videoRepository = new Repository<Video, VideosEntities>(videoEntities);
            var actorRepository = new Repository<Actor, VideosEntities>(videoEntities);
            var genreRepository = new Repository<Genre, VideosEntities>(videoEntities);
            var countryRepository = new Repository<Country, VideosEntities>(videoEntities);

            var document = new MyWebClient().GetHtmlDocument(url);
            var movieInfo = document.DocumentNode.SelectSingleNode("//div[@class='index_container']");
            var imdb = movieInfo.SelectSingleNode("//div[@class='mlink_imdb']/a").AttributeValue("href");
            var name = movieInfo.SelectSingleNode("//div[@class='stage_navigation movie_navigation']").InnerTextValue().Trim();
            var actors = movieInfo.SelectNodes("//span[@class='movie_info_actors']/a") ?? new HtmlNodeCollection(null);
            var countries = movieInfo.SelectNodes("//span[@class='movie_info_country']/a") ?? new HtmlNodeCollection(null);
            var generes = movieInfo.SelectNodes("//span[@class='movie_info_genres']/a") ?? new HtmlNodeCollection(null);
            var newVideo = new Video();

            var existingVideo = videoRepository.Items.FirstOrDefault(x => x.ImdbLink == imdb && x.Name == name);
            if (existingVideo != null)
            {
                if (redoIfFound)
                {
                    newVideo = existingVideo;
                }
                else
                {
                    return null;
                }
            }

            newVideo.Image = new MyWebClient().UrlToImageByteArray(movieInfo.SelectSingleNode("//div[@class='movie_thumb']/img").AttributeValue("src"));
            newVideo.Name = name;
            newVideo.Description = movieInfo.SelectSingleNode("//table/tr[1]/td[1]/p").InnerTextValue().Trim();
            newVideo.DateAdded = DateTime.Now;
            newVideo.ReleaseDate = DateTime.Parse(movieInfo.SelectSingleNode("//table/tr[2]/td[2]").InnerTextValue());
            newVideo.Runtime = movieInfo.SelectSingleNode("//table/tr[3]/td[2]").InnerTextValue().ToIntIgnoreStrings();
            newVideo.Director = movieInfo.SelectSingleNode("//table/tr[6]/td[2]").InnerTextValue();
            newVideo.Link = url;
            newVideo.ImdbLink = imdb;
            newVideo.BuyLink = movieInfo.SelectSingleNode("//div[@class='mlink_buydvd']/a").AttributeValue("href");
            newVideo.TrailerLink = movieInfo.SelectSingleNode("//span[@class='movie_version_link']/a[2]").AttributeValue("href");
            newVideo.Genres = new List<Genre>();
            newVideo.Countries = new List<Country>();
            newVideo.Actors = new List<Actor>();

            foreach (HtmlNode genre in generes)
            {
                var newGenre = genreRepository.Items.FirstOrDefault(x => x.Name == genre.InnerText);
                if (newGenre == null)
                {
                    newGenre = new Genre { Name = genre.InnerTextValue() };
                    genreRepository.AddItem(newGenre);
                    genreRepository.SaveChanges();
                }
                newVideo.Genres.Add(newGenre);
            }

            foreach (HtmlNode country in countries)
            {
                var newCountry = countryRepository.Items.FirstOrDefault(x => x.Name == country.InnerText);
                if (newCountry == null)
                {
                    newCountry = new Country { Name = country.InnerTextValue() };
                    countryRepository.AddItem(newCountry);
                    countryRepository.SaveChanges();

                }
                newVideo.Countries.Add(newCountry);
            }

            foreach (HtmlNode actor in actors)
            {
                var newActor = actorRepository.Items.FirstOrDefault(x => x.Name == actor.InnerText);
                if (newActor == null)
                {
                    newActor = new Actor { Name = actor.InnerTextValue() };
                    actorRepository.AddItem(newActor);
                    actorRepository.SaveChanges();

                }
                newVideo.Actors.Add(newActor);
            }

            if (existingVideo != null)
            {
                videoRepository.UpdateItem(newVideo);
            }
            else
            {
                videoRepository.AddItem(newVideo);
            }

            videoRepository.SaveChanges();

            Debug.WriteLine("PARSE: {0}- {1}", newVideo.Name, DateTime.Now);
            try
            {
                RefreshVideoLinks(newVideo);
            }
            catch (Exception e)
            {
                Debug.WriteLine("!!!!!!REFRESH FAILED----{0}----because {1}", name, e);
            }
            return newVideo;
        }



        private string GetUrlParameter(string rawUrl, string param)
        {
            if (!Uri.IsWellFormedUriString(rawUrl, UriKind.Absolute)) { return null; }
            Uri myUri = new Uri(rawUrl);
            return HttpUtility.ParseQueryString(myUri.Query).Get(param);
        }



        /// <summary>
        /// Extracts the total page number form last page btn on any page
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private int GetTotalPages(string url)
        {
            var document = new MyWebClient().GetHtmlDocument(url);
            var totalpages = 1;
            var lastPageBtn = document.DocumentNode.SelectSingleNode("//a[text() = ' >> ']");
            if (lastPageBtn != null)
            {
                var param = GetUrlParameter(primeWireUrl + lastPageBtn.AttributeValue("href"), "page");
                Int32.TryParse(param, out totalpages);
            }
            return totalpages;
        }

        #endregion
    }

}