using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Ninject;
using Ninject.Modules;
using VideoLinks.Models;
using VideoLinks.Repositories;

namespace VideoLinks
{

    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectDependencyResolver()
        {
            _kernel = new StandardKernel();
            AddBindings();
        }

        public object GetService(Type serviceType)
        {
            return _kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _kernel.GetAll(serviceType);
        }

        private void AddBindings()
        {

            _kernel.Bind<IRepository<Actor>>()
                .To<Repository<Actor, VideosEntities>>();

            _kernel.Bind<IRepository<Country>>()
              .To<Repository<Country, VideosEntities>>();

            _kernel.Bind<IRepository<Genre>>()
              .To<Repository<Genre, VideosEntities>>();

            _kernel.Bind<IRepository<Host>>()
              .To<Repository<Host, VideosEntities>>();

            _kernel.Bind<IRepository<Link>>()
              .To<Repository<Link, VideosEntities>>();

            _kernel.Bind<IRepository<TvEpisode>>()
              .To<Repository<TvEpisode, VideosEntities>>();

            _kernel.Bind<IRepository<Video>>()
              .To<Repository<Video, VideosEntities>>();

            _kernel.Bind<IRepository<Vote>>()
              .To<Repository<Vote, VideosEntities>>();

            _kernel.Bind<IDownLoadProgressRepository>()
                .To<DownLoadProgressRepository>();
        }
    }

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
            //json.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DependencyResolver.SetResolver(new NinjectDependencyResolver());
        }
    }
}
