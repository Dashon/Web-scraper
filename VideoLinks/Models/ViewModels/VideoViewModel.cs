using System;
using System.Collections.Generic;

namespace VideoLinks.Models.ViewModels
{
    public class VideoViewModel
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public byte[] Image { get; set; }
        public List<Genre> Genres { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Director { get; set; }
        public int Runtime { get; set; }
        public DateTime DateAdded { get; set; }
        public string ImdbLink { get; set; }
        public string TrailerLink { get; set; }
        public string BuyLink { get; set; }

        public virtual List<VoteViewModel> Votes { get; set; }
        public virtual List<LinkViewModel> Links { get; set; }
        public virtual List<CountryViewModel> Countries { get; set; }
        public virtual List<ActorViewModel> Actors { get; set; }

    }
}