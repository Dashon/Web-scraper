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
            return View();
        }
    }
}