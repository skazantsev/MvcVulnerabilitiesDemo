using System.Web.Mvc;
using BatcoinMarket.Business;

namespace BatcoinMarket.Controllers
{
    public class AdminController : Controller
    {
        [ValidateInput(false)]
        public ActionResult Delete(string username)
        {
            Accounts.Delete(username);
            return RedirectToAction("Login", "Account");
        }
    }
}