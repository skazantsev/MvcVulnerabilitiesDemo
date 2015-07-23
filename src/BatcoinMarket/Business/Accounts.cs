using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BatcoinMarket.Business
{
    public static class Accounts
    {
        public static readonly string StoreKey = "Accounts";

        public static List<Account> List { get; private set; }

        static Accounts()
        {
            List = HttpContext.Current.Session[StoreKey] as List<Account>;
            if (List == null)
            {
                List = new List<Account>
                {
                    new Account("Batman", 5000, "Credit Card number: 1111-2222-3333-4444"),
                    new Account("Robin", 100)
                };
                Save();
            }
        }

        public static List<Account> ExceptCurrent
        {
            get { return List.Where(x => x.Username != GetCurrent().Username).ToList(); }
        }

        public static Account Get(string username)
        {
            return List.FirstOrDefault(x => x.Username == username);
        }

        public static Account GetCurrent()
        {
            if (HttpContext.Current.User == null)
                return null;

            return Get(HttpContext.Current.User.Identity.Name);
        }

        public static void Register(string username)
        {
            List.Add(new Account(username, 0));
            Save();
        }

        public static void Delete(string username)
        {
            List.RemoveAll(x => x.Username == username);
            Save();
        }

        private static void Save()
        {
            HttpContext.Current.Session[StoreKey] = List;
        }
    }
}