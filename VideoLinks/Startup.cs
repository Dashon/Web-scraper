using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VideoLinks.Startup))]
namespace VideoLinks
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
