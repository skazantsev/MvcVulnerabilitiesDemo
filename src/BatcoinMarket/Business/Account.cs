using System;

namespace BatcoinMarket.Business
{
    [Serializable]
    public class Account
    {
        public Account(string username, int balance)
            : this(username, balance, "none")
        {
        }

        public Account(string username, int balance, string sensetiveInfo)
        {
            Username = username;
            Balance = balance;
            SensetiveInfo = sensetiveInfo;
        }

        public string Username { get; private set; }

        public int Balance { get; private set; }

        public string SensetiveInfo { get; private set; }

        public void AddSensetiveInfo(string data)
        {
            SensetiveInfo += data;
        }

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