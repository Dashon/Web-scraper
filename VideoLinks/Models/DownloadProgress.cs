using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace VideoLinks.Models
{
    public class DownloadProgress: IEntity
    {
        public int Id { get; set; }
        public int? LastPage { get; set; }
        public string SkippedPages { get; set; }
    }
}