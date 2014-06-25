using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Description;
using VideoLinks.Models;
using VideoLinks.Repositories;

namespace VideoLinks.Controllers.Api
{
   
    public class VideoController : ApiController
    {
        #region Private Variables
        private readonly IRepository<Video> _videoRepository;

        #endregion


        #region constructors
        public VideoController(IRepository<Video> videoRepository)
        {
            _videoRepository = videoRepository;
        }
        public VideoController()
        {
            _videoRepository = new Repository<Video, VideosEntities>(new VideosEntities());
        }
        #endregion
        

        #region Api EndPoints

        [Queryable(PageSize = 20)]
        public IQueryable GetVideos()
        {
            return _videoRepository.Items;

            //return _videoRepository.Items.Select(x => new vids
            //{
            //    Name = x.Name,
            //    Image = x.Image,
            //    Genres = x.Genres.ToList(),
            //    Description = x.Description,
            //    Id = x.Id,
            //    ReleaseDate = x.ReleaseDate
            //});
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
        #endregion


        #region Private methods
        #endregion


    }
}