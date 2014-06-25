using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using VideoLinks.Models.Abstract;

namespace VideoLinks.Models
{
    public class DownloadProgress: IEntity
    {
        public int Id { get; set; }
        public int? LastPage { get; set; }
        public string SkippedPages { get; set; }
    }
}