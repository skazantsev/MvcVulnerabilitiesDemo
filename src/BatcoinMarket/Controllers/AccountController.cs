using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatcoinMarket.Models;

namespace BatcoinMarket.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // insecure auth
            if (model.Username.Equals(model.Password, StringComparison.InvariantCultureIgnoreCase) &&
                model.Users.Any(x => x.Username == model.Username))
            {
                Response.Cookies.Add(new HttpCookie("Auth", model.Username));
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError("Password", "Invalid username or password.");
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            if (Response.Cookies["Auth"] != null)
                Response.Cookies["Auth"].Expires = DateTime.Now.AddDays(-1);
            return RedirectToAction("Transfer", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Transfer", "Home");
        }
    }
}