using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BatcoinMarket.Models
{
    public class RegisterViewModel
    {
        [AllowHtml]
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Username can't be empty")]
        public string Username { get; set; }
    }
}