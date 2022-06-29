namespace SupportBank.Console
{
    public class Person
    {
        public double Balance { get; set; }

        public Person()
        {
            Balance = 0;
        }

        public void Pay(Person p, double amount)
        {
            Balance -= amount;
            p.Balance += amount;
        }
    }
}