using System.Collections.Generic;

namespace VideoLinks.Models.ViewModels
{
    public class LinkViewModel
    {
        public int Id { get; set; }

        public virtual HostViewModel Host { get; set; }

        public string URL { get; set; }

        public string Quality { get; set; }

        public virtual List<VoteViewModel> Votes { get; set; }
    }
}