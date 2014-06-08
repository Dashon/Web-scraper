using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoLinks.Models
{
    public interface IEntity
    {
       int Id { get; set; }
    }
    public class Actor : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Video> Videos { get; set; }
    }

}