namespace SupportBank.Console
{
    public class Person
    {
        public Person()
        {
            Balance = 0;
        }

        public double Balance { get; set; }

        public void Pay(Person p, double amount)
        {
            Balance -= amount;
            p.Balance += amount;
        }
    }
}