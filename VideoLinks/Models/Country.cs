using System.Collections.Generic;

namespace VideoLinks.Models
{
    public class Country : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Video> Videos { get; set; }
    }
}