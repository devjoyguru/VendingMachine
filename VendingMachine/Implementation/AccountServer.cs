using System.Collections.Generic;

namespace VendingMachine.Implementation
{
    public interface IAccountServer
    {
       bool ValidateCard(string cardNo, int pin,out IAccount account);
    }



    internal sealed class AccountServer:IAccountServer
    {
        private static AccountServer accountServer;
        private static readonly object instanceLocker=new object();
        
        private static Dictionary<string,IAccount> Accounts;

        private AccountServer()
        {
            Accounts=new Dictionary<string, IAccount>();
        }

        public void AddAccount(IAccount account)
        {
            var cardsLinkedToAccount = account.GetCards();
            foreach (var cashCard in cardsLinkedToAccount)
            {
                if(!Accounts.ContainsKey(cashCard.CardNo))
                Accounts.Add(cashCard.CardNo,account);
            }
        }

        public static IAccountServer GetAccountServerInstance
        {
            get
            {
                if (accountServer == null)
                {
                    lock (instanceLocker)
                    {
                        if (accountServer == null)
                        {
                            accountServer = new AccountServer();
                        }
                    }
                }
                return accountServer;
            }
        }

        public bool ValidateCard(string cardNo, int pin,out IAccount account)
        {
            account = null;
            var isAccountFound= Accounts.TryGetValue(cardNo, out account);

            return isAccountFound && account.TryValidateCard(cardNo, pin);
        }


    }

    
}
