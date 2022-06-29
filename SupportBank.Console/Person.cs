namespace SupportBank.Console
{
    public class Person
    {
        public double Balance { get; set; }

        public Person()
        {
            Balance = 0;
        }

        public void Pay(Person person, double amount)
        {
            Balance -= amount;
            person.Balance += amount;
        }
    }
}