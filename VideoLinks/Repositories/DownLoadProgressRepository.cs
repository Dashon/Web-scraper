using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using CsQuery.Utility;
using Newtonsoft.Json;
using VideoLinks.Models;

namespace VideoLinks.Repositories
{
    public class DownLoadProgressRepository : Repository<DownloadProgress, VideosEntities>, IDownLoadProgressRepository
    {

        public DownLoadProgressRepository(VideosEntities context)
            : base(context)
        {

        }

        public DownloadProgress UpdateLastPage(int page)
        {
            var progress = Items.FirstOrDefault();
            if (progress != null)
            {
                progress.LastPage = page;
                UpdateItem(progress);
            }
            return progress;
        }

        public DownloadProgress UpdateSkipedPages(List<Video> pages)
        {
            var progress = Items.FirstOrDefault();
            if (progress != null)
            {
                progress.SkippedPages = JsonConvert.SerializeObject(pages);
                UpdateItem(progress);
            }
            return progress;
        }

        public DownloadProgress AddSkipedPage(int page)
        {
            var progress = Items.FirstOrDefault();
            if (progress != null)
            {
                var previous = new List<int>();
                if (progress.SkippedPages != null)
                    previous = JsonConvert.DeserializeObject<List<int>>(progress.SkippedPages);

                previous.Add(page);

                progress.SkippedPages = JsonConvert.SerializeObject(previous);
                UpdateItem(progress);
            }
            return progress;
        }
    }
}