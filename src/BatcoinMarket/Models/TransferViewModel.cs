using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using BatcoinMarket.Business;

namespace BatcoinMarket.Models
{
    public class TransferViewModel
    {
        public TransferViewModel()
        {
            Accounts = Business.Accounts.List;
        }

        [Range(1, int.MaxValue, ErrorMessage = "The value should be more than 0")]
        public int Amount { get; set; }

        [Required(ErrorMessage = "Select an account")]
        public string To { get; set; }

        public List<Account> Accounts { get; private set; }

        public List<SelectListItem> GetAccountsListItems()
        {
            var items = new List<SelectListItem> {new SelectListItem {Text = "", Value = "", Selected = string.IsNullOrEmpty(To)}};
            items.AddRange(Business.Accounts.ExceptCurrent.Select(x =>
                new SelectListItem
                {
                    Text = x.Username,
                    Value = x.Username,
                    Selected = x.Username == To
                }));
            return items;
        }
    }
}