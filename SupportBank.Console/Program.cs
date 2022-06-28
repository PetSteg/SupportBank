using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace SupportBank.Console
{
    class Program
    {
        private static Dictionary<string, Person> people = new Dictionary<string, Person>();
        private static List<Transaction> transactions = new List<Transaction>();

        static void ListAll()
        {
            foreach (var person in people)
            {
                System.Console.WriteLine(person.Key + ": " + person.Value.Balance);
            }
        }

        static void ListAccount(string name)
        {
            if (!people.ContainsKey(name))
            {
                System.Console.WriteLine(name + " has no transactions");
                return;
            }

            System.Console.WriteLine(name + ": " + people[name]);
            foreach (var transaction in transactions.Where(x => x.From == name || x.To == name).ToList())
            {
                System.Console.WriteLine(transaction.ToString());
            }
        }

        static void Main()
        {
            using (var reader = new StreamReader("../../../Transactions2014.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                transactions = csv.GetRecords<Transaction>().ToList();

                foreach (var transaction in transactions)
                {
                    if (!people.ContainsKey(transaction.From))
                        people.Add(transaction.From, new Person());
                    if (!people.ContainsKey(transaction.To))
                        people.Add(transaction.To, new Person());

                    people[transaction.From].Balance -= transaction.Amount;
                    people[transaction.To].Balance += transaction.Amount;
                }
            }

            System.Console.WriteLine(
                "Commands:\n\"List All\" -> outputs the names of each person and their balance\n" +
                "\"List [Name]\" -> prints a list of every transaction for the account with this name");

            string input;
            while ((input = System.Console.ReadLine()) != null && input.Length > 0)
            {
                if (input == "List All")
                    ListAll();
                else if (input.Length > 5 && input.Substring(0, 5) == "List ")
                {
                    string name = input.Substring(5);
                    ListAccount(name);
                }
                else System.Console.WriteLine("Wrong command");
            }
        }
    }
}