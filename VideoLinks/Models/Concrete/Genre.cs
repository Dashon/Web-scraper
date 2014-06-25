using System.Collections.Generic;
using VideoLinks.Models.Abstract;

namespace VideoLinks.Models
{
    public class Genre : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Video> Videos { get; set; }
    }
}