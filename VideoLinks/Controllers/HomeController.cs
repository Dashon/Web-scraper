using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using VideoLinks.Models;
using System.Threading.Tasks;
using VideoLinks.Scraper;

namespace VideoLinks.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            var watch = new Stopwatch();
            watch.Start();
            var db = new VideosEntities();
            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            var scraper = new PrimeWireScraper();
       //      scraper.DownloadAllMovies(1);
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