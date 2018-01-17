using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VendingMachine.Implementation;

namespace VendingMachine.Implementation.Test
{
    [TestClass]
    public class VendingMachineTests
    {
        private AccountServer accountServer;
        private int MaxCups;
        private Drink softdrink;

        private CashCard card1;
        private CashCard card2;
        private CashCard card3;
        private CashCard card4;

        private JointAccount accountWithBalance;
        private JointAccount accountWithInsufficientBalance;

        [TestInitialize]
        public void Setup()
        {
            MaxCups = 25;
            softdrink=new Drink(0.50m,"Soft drink");

            accountServer = AccountServer.GetAccountServerInstance as AccountServer;

            card1 = new CashCard("12345",1001);
            card2 = new CashCard("67890",1002);

            card3 = new CashCard("11111", 2001);
            card4 = new CashCard("22222", 2002);

            accountWithBalance = new JointAccount("101",20m,card1,card2);
            accountWithInsufficientBalance=new JointAccount("102",0.45m,card3,card4);

            //add some test accounts
            accountServer.AddAccount(accountWithBalance);
            accountServer.AddAccount(accountWithInsufficientBalance);

        }

        [TestMethod]
        public void InvalidPinTest()
        {
            //arrange
            var vendingmachine = new VendingMachine(MaxCups, accountServer);

            var wrongPin = card1.Pin + 1;

            //act
            string message;
            var isSuccess= vendingmachine.Vend(card1.CardNo, wrongPin, softdrink, out message);

            //assert
            var testMsg = $"INVALID Pin Test - Message returned = {message}";
            Assert.IsTrue(!isSuccess,testMsg);
        }

        [TestMethod]
        public void InsufficientBalanceTest()
        {
            //arrange
            var vendingmachine = new VendingMachine(MaxCups, accountServer);
            
            //act
            string message1;
            string message2;
            var isSuccessWithCard1 = vendingmachine.Vend(card3.CardNo, card3.Pin, softdrink, out message1);
            var isSuccessWithCard2 = vendingmachine.Vend(card4.CardNo, card4.Pin, softdrink, out message2);
            
            //assert
            var testMsg = $"Insufficient balance test - Message returned #1 = {message1} and #2={message2}";
            Assert.IsTrue(!isSuccessWithCard1 && !isSuccessWithCard2, testMsg);
        }

        [TestMethod]
        public void VendSuccessTest()
        {
            //arrange
            var vendingmachine = new VendingMachine(MaxCups, accountServer);
            
            //act
            string message;
            var isSuccess = vendingmachine.Vend(card1.CardNo, card1.Pin, softdrink, out message);

            //assert
            var testMsg = $"Vending success Test - Message returned = {message}";
            
            Assert.IsTrue(isSuccess, testMsg);
        }

        [TestMethod]
        public void BalanceAfterVendingTest()
        {
            //arrange
            var vendingmachine = new VendingMachine(MaxCups, accountServer);
            IAccount account;
            accountServer.ValidateCard(card1.CardNo, card1.Pin, out account);
            var prevBalance = account.Balance();

            //act
            string message;
            var isSuccess = vendingmachine.Vend(card1.CardNo, card1.Pin, softdrink, out message);
            var currentBalance = account.Balance();
            
            Assert.IsTrue(prevBalance-currentBalance==softdrink.Cost,"Card balance not updated after vending");

            //assert
            var testMsg = $"Vending success Test - Message returned = {message}";

            Assert.IsTrue(isSuccess, testMsg);
        }

        [TestMethod]
        public void VendAllCupsAndEmptyTest()
        {
            //arrange
            var vendingmachine = new VendingMachine(MaxCups, accountServer);
            
            //act
            //try and empty all cups
            for (var i = 1; i <= MaxCups; i++)
            {
                string message;
                var isVend= vendingmachine.Vend(card1.CardNo, card1.Pin, softdrink, out message);

                Assert.IsTrue(isVend, $"Error vending iteration : {i}");
            }

            //should not vend as empty
            string messageForEmpty;
            var isSuccess= vendingmachine.Vend(card1.CardNo, card1.Pin, softdrink, out messageForEmpty);

            //assert
            var testMsg = $"Empty Vend Test - Message returned = {messageForEmpty}";
            Console.WriteLine(testMsg);
            Assert.IsTrue(!isSuccess, testMsg);
        }

        [TestMethod]
        public void ConcurrentVendTest()
        {
            //arrange
            var vendingmachine1 = new VendingMachine(MaxCups, accountServer);
            var vendingmachine2 = new VendingMachine(MaxCups, accountServer);
            IAccount account;
            accountServer.ValidateCard(card1.CardNo, card1.Pin, out account);
            
            var isSuccess1 = false;
            var isSuccess2 = false;

            string message1=null;
            string message2=null;

            decimal balanceAtStart = account.Balance();
            decimal balance1=0m;
            decimal balance2=0m;


            
           var t1= Task.Factory.StartNew(() =>
           {
               isSuccess1 = vendingmachine1.Vend(card1.CardNo, card1.Pin, softdrink, out message1);
               balance1 = account.Balance();
           });

            var t2= Task.Factory.StartNew(() =>
            {
                isSuccess2 = vendingmachine2.Vend(card2.CardNo, card2.Pin, softdrink, out message2);
                balance2 = account.Balance();
            });

            Task.WaitAll(t1, t2);
            Console.WriteLine($"from Vending machine 1 {message1} and balance {balance1}");
            Console.WriteLine($"from Vending machine 2 {message2} and balance {balance2}");

            Assert.IsTrue(isSuccess1 && isSuccess2 && (balanceAtStart- Math.Min(balance1,balance2))==softdrink.Cost*2m,"Concurrent vending and balance check");
            

        }

        
    }
}
