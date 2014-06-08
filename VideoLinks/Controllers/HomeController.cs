using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SkyScraper;
using SkyScraper.Observers.ConsoleWriter;
using SkyScraper.Observers.ImageScraper;
using VideoLinks.Models;
using System.Threading;
using System.Threading.Tasks;
using VideoLinks.Helpers;
using VideoLinks.Scraper;

namespace VideoLinks.Controllers
{
    public class HomeController : Controller
    {

        public async Task<ActionResult> Index()
        {
            var watch = new Stopwatch();
            watch.Start();
            var db = new VideosEntities();
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            var scraper = new PrimeWireScraper();
             await scraper.DownloadAllMovies(10);
            watch.Stop();
            ViewBag.Watch = watch.Elapsed;
            ViewBag.Videos = db.Videos.ToList();
            return View();
        }

        

        public FileContentResult GetImg(int vidId)
        {
            var db = new VideosEntities();
            return new FileContentResult(db.Videos.Find(vidId).Image, "image/jpeg");
        }
    }
}