using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using VideoLinks.Models;

namespace VideoLinks
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Video>("Videos");
            builder.EntitySet<Actor>("Actors");
            builder.EntitySet<Country>("Countries");
            builder.EntitySet<Genre>("Genres");
            builder.EntitySet<Link>("Links");
            builder.EntitySet<Vote>("Votes");
            builder.EntitySet<Host>("Hosts");
            config.Routes.MapODataRoute("odata", "odata", builder.GetEdmModel());

        }
    }
}
