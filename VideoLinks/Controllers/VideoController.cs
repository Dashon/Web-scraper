using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VideoLinks.Models;
using VideoLinks.Repositories;
using VideoLinks.Scraper;

namespace VideoLinks.Controllers
{
    public class VideoController : Controller
    {
        private readonly IRepository<Video> _videoRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<Genre> _genreRepository;
        private readonly IRepository<Link> _linkRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<Host> _hostRepository;
        private readonly IRepository<TvEpisode> _tvEpisodeRepository;
        private readonly IDownLoadProgressRepository _downLoadProgressRepository;
        private readonly VideosEntities _videoEntities;

        public VideoController()
        {
            _videoEntities = new VideosEntities();
            _videoRepository = new Repository<Video, VideosEntities>(_videoEntities);
            _actorRepository = new Repository<Actor, VideosEntities>(_videoEntities);
            _genreRepository = new Repository<Genre, VideosEntities>(_videoEntities);
            _linkRepository = new Repository<Link, VideosEntities>(_videoEntities);
            _countryRepository = new Repository<Country, VideosEntities>(_videoEntities);
            _hostRepository = new Repository<Host, VideosEntities>(_videoEntities);
            _tvEpisodeRepository = new Repository<TvEpisode, VideosEntities>(_videoEntities);
            _downLoadProgressRepository = new DownLoadProgressRepository(_videoEntities);
        }

        // GET: /Video/5
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Video video = _videoRepository.FindByID((int)id);
            if (video == null)
            {
                return HttpNotFound();
            }
            return View(video);
        }

        public void DownloadAllMovies()
        {
            var scraper = new PrimeWireScraper();
            scraper.DownloadAllMovies(false);
        }

    }
}
