namespace VendingMachine.Implementation
{
    public struct Drink
    {
        public Drink(decimal cost, string name)
        {
            Cost = cost;
            Name = name;
        }

        public decimal Cost { get;  }
        public string Name { get; }
    }
}