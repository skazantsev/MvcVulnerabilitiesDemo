using System;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BatcoinMarket
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
        }

        protected void Application_OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            // insecure auth
            var authCookie = Request.Cookies["Auth"];
            if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
            {
                var identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.Name, authCookie.Value)
                    }, "ApplicationCookie");
                var user = new ClaimsPrincipal(identity);
                Thread.CurrentPrincipal = user;
                HttpContext.Current.User = user;
            }
        }
    }
}
