using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace VideoLinks.Models
{
    public class VideosEntities : DbContext
    {
        public DbSet<TvEpisode> TvEpisodes { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Host> Hosts { get; set; }
        public DbSet<DownloadProgress> DownloadProgress { get; set; }


        public VideosEntities()
            : base("DefaultConnection")
        {
            // Debug.Write(Database.Connection.ConnectionString);
            this.Configuration.ProxyCreationEnabled = false;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<VideosEntities>());

            //Build ActorVideos Many-to-Many Relationship
            modelBuilder.Entity<Video>()
                .HasMany(t => t.Actors)
                .WithMany(t => t.Videos)
                .Map(m =>
                         {
                             m.ToTable("VideoActors");
                             m.MapLeftKey("VideoId");
                             m.MapRightKey("ActorId");
                         });


            //build VideoGeneres many-to-many relationship
            modelBuilder.Entity<Video>()
                .HasMany(t => t.Genres)
                .WithMany(t => t.Videos)
                .Map(m =>
                         {
                             m.ToTable("VideoGenres");
                             m.MapLeftKey("VideoId");
                             m.MapRightKey("GenreId");
                         });


            //build VideoCountries many-to-many relationship
            modelBuilder.Entity<Video>()
                .HasMany(t => t.Countries)
                .WithMany(t => t.Videos)
                .Map(m =>
                         {
                             m.ToTable("VideoCountries");
                             m.MapLeftKey("VideoId");
                             m.MapRightKey("CountryId");
                         });
        }
    }
}