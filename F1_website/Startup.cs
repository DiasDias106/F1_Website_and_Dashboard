using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(F1_website.Startup))]
namespace F1_website
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
