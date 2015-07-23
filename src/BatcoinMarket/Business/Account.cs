namespace BatcoinMarket.Business
{
    public class Account
    {
        public Account(string usename, int balance)
        {
            Username = usename;
            Balance = balance;
        }

        public string Username { get; private set; }

        public int Balance { get; private set; }

        public bool Tranfer(int amount, Account to)
        {
            if (Balance - amount < 0)
                return false;

            // let's pretend it's a transaction
            Balance -= amount;
            to.Balance += amount;
            return true;
        }
    }
}