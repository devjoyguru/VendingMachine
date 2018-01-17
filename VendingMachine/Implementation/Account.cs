using System;
using System.Collections.Generic;

namespace VendingMachine.Implementation
{
    public interface IAccount
    {
        string AccountNo { get; }
        bool TryValidateCard(string cardNo, int pin);
        bool Withdraw(decimal amount);
        decimal Balance();
        IEnumerable<ICashCard> GetCards();

    }

    internal class JointAccount : IAccount
    {
        private readonly Tuple<IInternalCashCard, IInternalCashCard> Cards;
        private decimal CurrentBalance;
        private static readonly object accountLocker = new object();

        public JointAccount(string accountNo, decimal currentBalance, IInternalCashCard cashCard1, IInternalCashCard cashCard2)
        {
            AccountNo = accountNo;
            CurrentBalance = currentBalance;
            Cards=new Tuple<IInternalCashCard, IInternalCashCard>(cashCard1,cashCard2);
        }


        public IEnumerable<ICashCard> GetCards()
        {
            return new[] {Cards.Item1, Cards.Item2};
        }

        public string AccountNo { get; }
        public bool TryValidateCard(string cardNo, int pin)
        {
            if (Cards == null) return false;

            return ValidateCard(Cards.Item1, cardNo, pin) || ValidateCard(Cards.Item2, cardNo, pin);
            
        }

        private static bool ValidateCard(IInternalCashCard card, string cardNo, int pin)
        {
            return card!=null && card.CardNo.Equals(cardNo) && card.Pin == pin;
        }

        /// <summary>
        /// Thread safe account withdrawl
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool Withdraw(decimal amount)
        {
            lock (accountLocker)
            {
                if (CurrentBalance < amount) return false;

                CurrentBalance -= amount;
                return true;
            }
        }

        public decimal Balance()
        {
            return CurrentBalance;
        }
    }
}