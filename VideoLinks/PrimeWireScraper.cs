using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.ModelBinding;
using HtmlAgilityPack;
using VideoLinks.Models;
using VideoLinks.Helpers;
using System.Threading.Tasks;

namespace VideoLinks.Scraper
{
    public class PrimeWireScraper
    {
        public bool DownloadAllMovies(int numberOfPages = 0)
        {
            var document = GetHtmlDocument("http://www.primewire.ag/index.php?sort=featured&page=1");
            var totalpages = (numberOfPages == 0) ? GetTotalPages(document) : numberOfPages;

            for (int i = 0; i <= totalpages; i++)
            {
                Debug.WriteLine("SCRAPE PAGE {0} - {1}", i, DateTime.Now);
                var nextpage = "http://www.primewire.ag/index.php?sort=featured&page=" + i;
                ScrapePage(nextpage);
            };
            return true;
        }

        //Todo: Requires a seperate Video Parser.
        public bool DownloadAllTvShows(int numberOfPages = 0)
        {
            var document = GetHtmlDocument("http://www.primewire.ag/index.php?tv=&page=1");
            var totalpages = (numberOfPages == 0) ? GetTotalPages(document) : numberOfPages;

            for (int i = 0; i <= totalpages; i++)
            {
                var nextpage = "http://www.primewire.ag/index.php?tv=&page=" + i;
                ScrapePage(nextpage);
            };

            return true;
        }

        public Video DownloadTvShow(string url)
        {
            return ParseVideoPage(url, true);
        }
        public Video DownloadMovie(string url)
        {
            return ParseVideoPage(url, false);
        }

        public void RefreshTvShowLinks(string url)
        {
            var document = GetHtmlDocument(url);
            var videos = new List<Video>();
            foreach (var episode in document.DocumentNode.SelectNodes("//div[@class='tv_episode_item']/a"))
            {
                var episodeLink = episode.AttributeValue("href");
                RefreshVideoLinks(episodeLink);
            };
        }

        public void RefreshVideoLinks(string url)
        {
            var db = new VideosEntities();
            var video = db.Videos.SingleOrDefault(v => v.Link == url);

            if (video == null) { return; }
            var document = GetHtmlDocument(url);
            var xPath = "//table[contains(@class,'movie_version')]/tbody/tr/td/span[contains(@class,'quality_')]";
            var movieLinks = document.DocumentNode.SelectNodes(xPath);
            \
            //delete all existing Links
            foreach (Link link in video.Links)
            {
                db.Links.Remove(link);
            }

            foreach (HtmlNode link in movieLinks)
            {
                var tableRow = link.ParentNode.ParentNode;
                var newLink = new Link();
                var linkUrl = tableRow.SelectSingleNode("td[2]/span/a").AttributeValue("href");
                var quality = tableRow.SelectSingleNode("td[1]/span").AttributeValue("class");
                var host = tableRow.SelectSingleNode("td[3]/span").InnerTextValue();

                //Load Link URL and Extract actual URL
                var urlDoc = GetHtmlDocument("http://www.primewire.ag/" + linkUrl);

                var actualUrl = urlDoc.ResponseUri;

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

        private void ScrapePage(string url)
        {
            var document = GetHtmlDocument(url);
            foreach (var video in document.DocumentNode.SelectNodes("//div[@class='index_item index_item_ie']"))
            {
                var primeWireUrl = "http://www.primewire.ag";
                var videoLink = primeWireUrl + video.FirstChild.AttributeValue("href");
                ParseVideoPage(videoLink, false);
            };
        }

        private void ParseAllEpisodes(string firstPage, bool allPages = true)
        {
            var document = GetHtmlDocument(firstPage);
            var videos = new List<Video>();
            foreach (var episode in document.DocumentNode.SelectNodes("//div[@class='tv_episode_item']/a"))
            {
                var episodeLink = episode.AttributeValue("href");
                ParseVideoPage(episodeLink, true);
            };
        }

        private Video ParseVideoPage(string url, bool isTvShow)
        {
            var db = new VideosEntities();
            var document = GetHtmlDocument(url);
            var movieInfo = document.DocumentNode.SelectSingleNode("//div[@class='index_container']");
            var IMDB = movieInfo.SelectSingleNode("//div[@class='mlink_imdb']/a").AttributeValue("href");
            var name = movieInfo.SelectSingleNode("//div[@class='stage_navigation movie_navigation']").InnerTextValue().Trim();
            var actors = movieInfo.SelectNodes("//span[@class='movie_info_actors']/a") ?? new HtmlNodeCollection(null);
            var countries = movieInfo.SelectNodes("//span[@class='movie_info_country']/a") ?? new HtmlNodeCollection(null);
            var generes = movieInfo.SelectNodes("//span[@class='movie_info_genres']/a") ?? new HtmlNodeCollection(null);
            var newVideo = new Video();

            var existingVideo = db.Videos.SingleOrDefault(x => x.ImdbLink == IMDB && x.Name == name);
            if (existingVideo != null)
            {
                return null;
                newVideo = existingVideo;
            }

            newVideo.Image = ImageToByteArray(movieInfo.SelectSingleNode("//div[@class='movie_thumb']/img").AttributeValue("src"));
            newVideo.Name = name;
            newVideo.Description = movieInfo.SelectSingleNode("//table/tr[1]/td[1]/p").InnerTextValue().Trim();
            newVideo.DateAdded = DateTime.Now;
            newVideo.ReleaseDate = DateTime.Parse(movieInfo.SelectSingleNode("//table/tr[2]/td[2]").InnerTextValue());
            newVideo.Runtime = movieInfo.SelectSingleNode("//table/tr[3]/td[2]").InnerTextValue().ToIntIgnoreStrings();
            newVideo.Director = movieInfo.SelectSingleNode("//table/tr[6]/td[2]").InnerTextValue();
            newVideo.Link = url;
            newVideo.ImdbLink = IMDB;
            newVideo.BuyLink = movieInfo.SelectSingleNode("//div[@class='mlink_buydvd']/a").AttributeValue("href");
            //newVideo.TrailerLink = movieInfo.SelectSingleNode("//div[@class='movie_version_link']/a").InnerTextValue();
            newVideo.Genres = new List<Genre>();
            newVideo.Countries = new List<Country>();
            newVideo.Actors = new List<Actor>();

            foreach (HtmlNode genre in generes)
            {
                var newGenre = db.Genres.SingleOrDefault(x => x.Name == genre.InnerText);
                if (newGenre == null)
                {
                    newGenre = new Genre { Name = genre.InnerTextValue() };
                    db.Genres.Add(newGenre);
                }
                newVideo.Genres.Add(newGenre);
            }

            foreach (HtmlNode country in countries)
            {
                var newCountry = db.Countries.SingleOrDefault(x => x.Name == country.InnerText);
                if (newCountry == null)
                {
                    newCountry = new Country { Name = country.InnerTextValue() };
                    db.Countries.Add(newCountry);
                }
                newVideo.Countries.Add(newCountry);
            }

            foreach (HtmlNode actor in actors)
            {
                var newActor = db.Actors.SingleOrDefault(x => x.Name == actor.InnerText);
                if (newActor == null)
                {
                    newActor = new Actor { Name = actor.InnerTextValue() };
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
            RefreshVideoLinks(url);
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

        private MyHtmlDocument GetHtmlDocument(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) { return null; }
            var uri = new Uri(url);
            var document = new MyHtmlDocument();
            var client = new MyWebClient();
            document.ResponseUri = client.ResponseUri;
            document.LoadHtml(client.DownloadString(url));

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
                var param = GetUrlParameter(primeWireUrl + lastPageBtn.AttributeValue("href"), "page");
                Int32.TryParse(param, out totalpages);
            }
            return totalpages;
        }

        #endregion
    }
}