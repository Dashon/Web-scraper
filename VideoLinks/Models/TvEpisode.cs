using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VideoLinks.Models
{
    public class TvEpisode : IEntity
    {
        public int Id { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public int VideoId { get; set; }
        public virtual Video Video { get; set; }
    }
}