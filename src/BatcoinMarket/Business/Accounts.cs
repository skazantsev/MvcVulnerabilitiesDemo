using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BatcoinMarket.Business
{
    public static class Accounts
    {
        public static List<Account> All = new List<Account>
        {
            new Account("Batman", 5000),
            new Account("Robin", 0),
            new Account("Joker", 100)
        };

        public static List<Account> ExceptCurrent
        {
            get { return All.Where(x => x.Username != GetCurrent().Username).ToList(); }
        }

        public static Account Get(string username)
        {
            return All.FirstOrDefault(x => x.Username == username);
        }

        public static Account GetCurrent()
        {
            if (HttpContext.Current.User == null)
                return null;

            return Get(HttpContext.Current.User.Identity.Name);
        }
    }
}