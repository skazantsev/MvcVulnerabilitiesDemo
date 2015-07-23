using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BatcoinMarket.Business;

namespace BatcoinMarket.Models
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            Users = Accounts.List;
        }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public List<Account> Users { get; private set; }
    }
}