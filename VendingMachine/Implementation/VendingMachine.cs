namespace VendingMachine.Implementation
{
    public interface IVendingMachine
    {
        int MaxNoOfCups { get; }
        int CupsRemaining { get; }
        bool Vend(string cardNo, int pin, Drink drink, out string message);
        
    }

    public sealed class VendingMachine : IVendingMachine
    {
        
        private readonly IAccountServer AccountMgmtServer;
        private int NoOfCupsRemaining;
        public VendingMachine(int maxnoofCups,IAccountServer accountServer)
        {
            MaxNoOfCups = maxnoofCups;
            NoOfCupsRemaining = maxnoofCups;
            AccountMgmtServer = accountServer;
        }


        public int MaxNoOfCups { get; }

        public int CupsRemaining => NoOfCupsRemaining;
       
        public bool Vend(string cardNo, int pin, Drink drink, out string message)
        {

            message = null;

            if (NoOfCupsRemaining == 0)
            {
                message = "Sorry no more cups to serve drink";
                return false;
            }

            IAccount account;
            var isAuthenticated = AccountMgmtServer.ValidateCard(cardNo, pin, out account);

            if (!isAuthenticated)
            {
                message = "Invalid PIN / account";
                return false;
            }

           var isTransactionSuccess= account.Withdraw(drink.Cost);

            if (!isTransactionSuccess)
            {
                message = $"Insufficient funds. Need balance of {drink.Cost}";
                return false;
            }

            NoOfCupsRemaining--;

            message = "Enjoy your drink";
            return true;
        }

    }
    
}
