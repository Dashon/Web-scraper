using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoLinks.Models
{
    public class Link : IEntity
    {
        public int Id { get; set; }

        public int HostId { get; set; }
        public virtual Host Host { get; set; }

        [Required]
        public string URL { get; set; }

        public string Quality { get; set; }

        public int VideoId { get; set; }
        public virtual Video Video { get; set; }

        public virtual ICollection<Vote> Votes { get; set; }
    }
}