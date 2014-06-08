using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using VideoLinks.Models;
using VideoLinks.Helpers;
using System.Threading.Tasks;

namespace VideoLinks.Scraper
{
    public class PrimeWireScraper
    {
        public async Task<bool> DownloadAllMovies(int numberOfPages = 0)
        {
            var document = await GetHtmlDocument("http://www.primewire.ag/index.php?sort=featured&page=1");
            var totalpages = (numberOfPages == 0) ? GetTotalPages(document) : numberOfPages;

            Parallel.For(1, totalpages, i =>
          {
              Debug.WriteLine("SCRAPE PAGE {0} - {1}", i, DateTime.Now);
              var nextpage = "http://www.primewire.ag/index.php?sort=featured&page=" + i;
              ScrapePage(nextpage);
          });
            return true;
        }

        //Todo: Requires a seperate Video Parser.
        public async Task<bool> DownloadAllTvShows(int numberOfPages = 0)
        {
            var document = await GetHtmlDocument("http://www.primewire.ag/index.php?tv=&page=1");
            var totalpages = (numberOfPages == 0) ? GetTotalPages(document) : numberOfPages;

            Parallel.For(1, totalpages, i =>
           {
               var nextpage = "http://www.primewire.ag/index.php?tv=&page=" + i;
               ScrapePage(nextpage);
           });

            return true;
        }

        public async Task<Video> DownloadTvShow(string url)
        {
            return await ParseVideoPage(url, true);
        }
        public async Task<Video> DownloadMovie(string url)
        {
            return await ParseVideoPage(url, false);
        }

        public async void RefreshTvShowLinks(string url)
        {
            var document = await GetHtmlDocument(url);
            var videos = new List<Video>();
            Parallel.ForEach(document.DocumentNode.SelectNodes("//div[@class='tv_episode_item']/a"), episode =>
            {
                var episodeLink = episode.Attributes["href"].Value;
                RefreshVideoLinks(episodeLink);
            });
        }

        public async Task RefreshVideoLinks(string url)
        {
            var db = new VideosEntities();
            var video = db.Videos.SingleOrDefault(v => v.Link == url);

            if (video == null) { return; }
            var document = await GetHtmlDocument(url);
            var xPath = "//table[contains(@class,'movie_version')]/tbody/tr/td/span[contains(@class,'quality_')]";
            var movieLinks = document.DocumentNode.SelectNodes(xPath);

            //delete all existing Links
            foreach (Link link in video.Links)
            {
                db.Links.Remove(link);
            }

            foreach (HtmlNode link in movieLinks)
            {
                var tableRow = link.ParentNode.ParentNode;
                var newLink = new Link();
                var linkUrl = tableRow.SelectSingleNode("td[2]/span/a").Attributes["href"].Value;
                var quality = tableRow.SelectSingleNode("td[1]/span").Attributes["class"].Value;
                var host = tableRow.SelectSingleNode("td[3]/span").InnerText;

                //Load Link URL and Extract actual URL
                var urlDoc = await GetHtmlDocument("http://www.primewire.ag/" + linkUrl);
                var actualUrl = urlDoc.DocumentNode.SelectSingleNode("//noframes").InnerText;

                //Extract HostName from javascript statment
                Match match = Regex.Match(host, @"'([^']*)");
                if (match.Success)
                {
                    host = match.Groups[1].Value;
                }
                var newHost = db.Hosts.SingleOrDefault(x => x.Name == host);
                if (newHost == null)
                {
                    newHost = new Host { Name = host };
                    db.Hosts.Add(newHost);
                }

                quality = quality.Replace("quality_", "");
                newLink.Quality = quality;
                newLink.Host = newHost;
                newLink.URL = actualUrl;
                newLink.Video = video;
                db.Links.Add(newLink);
                Debug.WriteLine("LINK:{0} - {1})", video.Name, DateTime.Now);
                db.SaveChanges();
            }
        }

        #region Helpers

        private async void ScrapePage(string url)
        {
            var document = await GetHtmlDocument(url);
            Parallel.ForEach(document.DocumentNode.SelectNodes("//div[@class='index_item index_item_ie']"), video =>
            {
                var primeWireUrl = "http://www.primewire.ag";
                var videoLink = primeWireUrl + video.FirstChild.Attributes["href"].Value;
                ParseVideoPage(videoLink, false);
            });
        }

        private async void ParseAllEpisodes(string firstPage, bool allPages = true)
        {
            var document = await GetHtmlDocument(firstPage);
            var videos = new List<Video>();
            Parallel.ForEach(document.DocumentNode.SelectNodes("//div[@class='tv_episode_item']/a"), episode =>
            {
                var episodeLink = episode.Attributes["href"].Value;
                ParseVideoPage(episodeLink, true);
            });
        }

        private async Task<Video> ParseVideoPage(string url, bool isTvShow)
        {
            var db = new VideosEntities();
            var document = await GetHtmlDocument(url);
            var movieInfo = document.DocumentNode.SelectSingleNode("//div[@class='index_container']");
            var IMDB = movieInfo.SelectSingleNode("//div[@class='mlink_imdb']/a").Attributes["href"].Value;
            var name = movieInfo.SelectSingleNode("//div[@class='stage_navigation movie_navigation']").InnerText.Trim();
            var newVideo = new Video();

            var existingVideo = db.Videos.SingleOrDefault(x => x.ImdbLink == IMDB && x.Name == name);
            if (existingVideo != null)
            {
                newVideo = existingVideo;
            }

            newVideo.Image = ImageToByteArray(movieInfo.SelectSingleNode("//div[@class='movie_thumb']/img").Attributes["src"].Value);
            newVideo.Name = name;
            newVideo.Description = movieInfo.SelectSingleNode("//table/tr[1]/td[1]/p").InnerText.Trim();
            newVideo.DateAdded = DateTime.Now;
            newVideo.ReleaseDate = DateTime.Parse(movieInfo.SelectSingleNode("//table/tr[2]/td[2]").InnerText);
            newVideo.Runtime = movieInfo.SelectSingleNode("//table/tr[3]/td[2]").InnerText.ToIntIgnoreStrings();
            newVideo.Director = movieInfo.SelectSingleNode("//table/tr[6]/td[2]").InnerText;
            newVideo.Link = url;
            newVideo.ImdbLink = IMDB;
            newVideo.BuyLink = movieInfo.SelectSingleNode("//div[@class='mlink_buydvd']/a").Attributes["href"].Value;
            //newVideo.TrailerLink = movieInfo.SelectSingleNode("//div[@class='movie_version_link']/a").InnerText;
            newVideo.Genres = new List<Genre>();
            newVideo.Countries = new List<Country>();
            newVideo.Actors = new List<Actor>();

            foreach (HtmlNode genre in movieInfo.SelectNodes("//span[@class='movie_info_genres']/a"))
            {
                var newGenre = db.Genres.SingleOrDefault(x => x.Name == genre.InnerText);
                if (newGenre == null)
                {
                    newGenre = new Genre { Name = genre.InnerText };
                    db.Genres.Add(newGenre);
                }
                newVideo.Genres.Add(newGenre);
            }

            foreach (HtmlNode country in movieInfo.SelectNodes("//span[@class='movie_info_country']/a"))
            {
                var newCountry = db.Countries.SingleOrDefault(x => x.Name == country.InnerText);
                if (newCountry == null)
                {
                    newCountry = new Country { Name = country.InnerText };
                    db.Countries.Add(newCountry);
                }
                newVideo.Countries.Add(newCountry);
            }

            foreach (HtmlNode actor in movieInfo.SelectNodes("//span[@class='movie_info_actors']/a"))
            {
                var newActor = db.Actors.SingleOrDefault(x => x.Name == actor.InnerText);
                if (newActor == null)
                {
                    newActor = new Actor { Name = actor.InnerText };
                    db.Actors.Add(newActor);
                }
                newVideo.Actors.Add(newActor);
            }

            if (existingVideo != null)
            {
                db.Videos.Attach(newVideo);
            }
            else
            {
                db.Videos.Add(newVideo);
            }
            Debug.WriteLine("PARSE: {0}- {1}", newVideo.Name, DateTime.Now);
            db.SaveChanges();
            await RefreshVideoLinks(url);
            return newVideo;
        }

        private static byte[] ImageToByteArray(string url)
        {
            byte[] byteArray = null;
            var webRequest = WebRequest.Create(url);
            var webResponse = webRequest.GetResponse();
            var responseStream = webResponse.GetResponseStream();
            if (responseStream != null)
            {
                var image = System.Drawing.Image.FromStream(responseStream);
                var memoryStream = new MemoryStream();
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                byteArray = memoryStream.ToArray();
            }
            return byteArray;
        }

        private string GetUrlParameter(string rawUrl, string param)
        {
            if (!Uri.IsWellFormedUriString(rawUrl, UriKind.Absolute)) { return null; }
            Uri myUri = new Uri(rawUrl);
            return HttpUtility.ParseQueryString(myUri.Query).Get(param);
        }

        private async Task<HtmlDocument> GetHtmlDocument(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) { return null; }
            var uri = new Uri(url);
            var document = new HtmlDocument();
            var client = new WebClient();

            document.LoadHtml(await client.DownloadStringTaskAsync(url));
            Debug.WriteLine("Load: {1} - {0}", DateTime.Now, url);

            return document;
        }

        private int GetTotalPages(HtmlDocument document)
        {
            var totalpages = 1;
            var lastPageBtn = document.DocumentNode.SelectSingleNode("//a[text() = ' >> ']");
            var primeWireUrl = "http://www.primewire.ag/";
            if (lastPageBtn != null)
            {
                var param = GetUrlParameter(primeWireUrl + lastPageBtn.Attributes["href"].Value, "page");
                Int32.TryParse(param, out totalpages);
            }
            return totalpages;
        }

        #endregion
    }
}