namespace VendingMachine.Implementation
{
    public interface ICashCard 
    {
        string CardNo { get; }
    }

    internal interface IInternalCashCard : ICashCard
    {
        int Pin { get; }
    }


    internal class CashCard : IInternalCashCard
    {
        internal CashCard(string cardNo, int pin)
        {
            CardNo = cardNo;
            Pin = pin;
        }

        public string CardNo { get;  }
        public int Pin { get; }
    }

    
}