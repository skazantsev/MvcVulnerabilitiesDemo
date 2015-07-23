using BatcoinMarket.Business;
using BatcoinMarket.Models;
using System.Web.Mvc;

namespace BatcoinMarket.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Transfer()
        {
            var model = new TransferViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Transfer(TransferViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentAccount = Accounts.GetCurrent();
            var to = Accounts.Get(model.To);
            if (currentAccount.Tranfer(model.Amount, to))
            {
                return RedirectToAction("Transfer", "Home");
            }

            ModelState.AddModelError("Amount", "You don't have this amount of batcoins");
            return View(model);
        }

        public ActionResult Store()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Codes()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult AccountInfo()
        {
            if (!Request.IsAuthenticated)
                return PartialView("_AccountInfo");

            return PartialView("_AccountInfo", Accounts.GetCurrent());
        }

        public ActionResult GetSensetiveInfo()
        {
            return Json(new {data = Accounts.GetCurrent().SensetiveInfo}, JsonRequestBehavior.AllowGet);
        }
    }
}