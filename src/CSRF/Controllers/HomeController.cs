using System.Collections.Generic;
using System.Web.Mvc;

namespace CSRF.Controllers
{
    public class HomeController : Controller
    {
        public static List<string> Log = new List<string>();
 
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Csrf()
        {
            return View();
        }

        public ActionResult Theft(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                Log.Add(data);
            }
            return new HttpStatusCodeResult(200);
        }

        public ActionResult Catch()
        {
            ViewBag.Log = Log;
            return View();
        }
    }
}