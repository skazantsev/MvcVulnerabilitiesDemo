using System.Web.Mvc;
using System.Web.Routing;

namespace BatcoinMarket
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Transfer", "Transfer", new { controller = "Home", action = "Transfer" }
            );

            routes.MapRoute("Store", "Store", new { controller = "Home", action = "Store" }
            );

            routes.MapRoute("Codes", "Codes", new { controller = "Home", action = "Codes" }
            );

            routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Home", action = "Transfer", id = UrlParameter.Optional }
            );
        }
    }
}
