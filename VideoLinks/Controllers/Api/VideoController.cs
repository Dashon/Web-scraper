using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using VideoLinks.Models;
using VideoLinks.Repositories;

namespace VideoLinks.Controllers.Api
{
    public class VideoController : ApiController
    {
        private readonly IRepository<Video> _videoRepository;

        public VideoController(IRepository<Video> videoRepository)
        {
            _videoRepository = videoRepository;
        }
         public VideoController()
        {
            _videoRepository = new Repository<Video,VideosEntities>(new VideosEntities());
        }

        public IQueryable GetVideos()
        {
            return _videoRepository.Items.Select(x => new { x.Name, x.Image, x.Genres, x.Description, x.Id }).Take(50);
        }

        // GET api/Video/5
        [ResponseType(typeof(Video))]
        public IHttpActionResult GetVideo(int id)
        {
            var video = _videoRepository.Items.Select(x => new
            {
                video = x,
                x.Genres,
                x.Actors,
                Links = x.Links.Select(l => new
                {
                    l.URL,
                    l.Host,
                    l.Quality
                })
            }).FirstOrDefault(x => x.video.Id == id);

            if (video == null)
            {
                return NotFound();
            }

            return Ok(video);
        }

    }
}