using System.Web.Mvc;
using System.Web.Routing;

namespace QuanLyDoanVien
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // API routes handled by WebApiConfig
            // All other routes go to Home/Index (AngularJS SPA shell)
            routes.MapRoute(
                name: "Default",
                url: "{*url}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}

