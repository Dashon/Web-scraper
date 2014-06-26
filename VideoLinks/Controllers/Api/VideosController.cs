using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using VideoLinks.Models;

namespace VideoLinks.Controllers.Api
{
    /*
    The WebApiConfig class may require additional changes to add a route for this controller. Merge these statements into the Register method of the WebApiConfig class as applicable. Note that OData URLs are case sensitive.

   
    */
    public class VideosController : ODataController
    {
        private VideosEntities db = new VideosEntities();

        // GET: odata/Videos
        [Queryable(PageSize =50 )]
        public IQueryable<Video> GetVideos()
        {
            return db.Videos;
        }

        // GET: odata/Videos(5)
        [Queryable]
        public SingleResult<Video> GetVideo([FromODataUri] int key)
        {
            return SingleResult.Create(db.Videos.Where(video => video.Id == key));
        }

        // PUT: odata/Videos(5)
        public IHttpActionResult Put([FromODataUri] int key, Video video)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != video.Id)
            {
                return BadRequest();
            }

            db.Entry(video).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VideoExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(video);
        }

        // POST: odata/Videos
        public IHttpActionResult Post(Video video)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Videos.Add(video);
            db.SaveChanges();

            return Created(video);
        }

        // PATCH: odata/Videos(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Video> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Video video = db.Videos.Find(key);
            if (video == null)
            {
                return NotFound();
            }

            patch.Patch(video);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VideoExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(video);
        }

        // DELETE: odata/Videos(5)
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            Video video = db.Videos.Find(key);
            if (video == null)
            {
                return NotFound();
            }

            db.Videos.Remove(video);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/Videos(5)/Actors
        [Queryable]
        public IQueryable<Actor> GetActors([FromODataUri] int key)
        {
            return db.Videos.Where(m => m.Id == key).SelectMany(m => m.Actors);
        }

        // GET: odata/Videos(5)/Countries
        [Queryable]
        public IQueryable<Country> GetCountries([FromODataUri] int key)
        {
            return db.Videos.Where(m => m.Id == key).SelectMany(m => m.Countries);
        }

        // GET: odata/Videos(5)/Genres
        [Queryable]
        public IQueryable<Genre> GetGenres([FromODataUri] int key)
        {
            return db.Videos.Where(m => m.Id == key).SelectMany(m => m.Genres);
        }

        // GET: odata/Videos(5)/Links
        [Queryable]
        public IQueryable<Link> GetLinks([FromODataUri] int key)
        {
            return db.Videos.Where(m => m.Id == key).SelectMany(m => m.Links);
        }

        // GET: odata/Videos(5)/Votes
        [Queryable]
        public IQueryable<Vote> GetVotes([FromODataUri] int key)
        {
            return db.Videos.Where(m => m.Id == key).SelectMany(m => m.Votes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool VideoExists(int key)
        {
            return db.Videos.Count(e => e.Id == key) > 0;
        }
    }
}
