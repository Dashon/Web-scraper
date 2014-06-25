using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VideoLinks.Models.Abstract;

namespace VideoLinks.Models
{
    public class Host : IEntity
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string VideoObject { get; set; }
        public string FileNotFoundObject { get; set; }
        public string DownloadObject { get; set; }

        public virtual ICollection<Link> Links { get; set; }
    }
}