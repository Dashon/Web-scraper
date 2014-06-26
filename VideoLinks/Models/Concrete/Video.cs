using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VideoLinks.Models.Abstract;

namespace VideoLinks.Models
{
    public class Video : IEntity
    {
        public int Id { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }
        public byte[] Image { get; set; }
        public string Description { get; set; }
        public string Director { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime ReleaseDate { get; set; }
        public int Runtime { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime DateAdded { get; set; }
        public string ImdbLink { get; set; }
        public string TrailerLink { get; set; }
        public string BuyLink { get; set; }

        public virtual ICollection<Vote> Votes { get; set; }
        public virtual ICollection<Link> Links { get; set; }
        public virtual ICollection<Genre> Genres { get; set; }
        public virtual ICollection<Country> Countries { get; set; }
        public virtual ICollection<Actor> Actors { get; set; }
    }
}