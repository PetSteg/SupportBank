using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace SupportBank.Console
{
    class Program
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<string, Person> people = new Dictionary<string, Person>();
        private static List<Transaction> transactions = new List<Transaction>();
        private static string filePath = "../../../DodgyTransactions2015.csv";

        static void ListAll()
        {
            foreach (var person in people)
                System.Console.WriteLine(person.Key + ": " + person.Value.Balance);
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
            var config = new LoggingConfiguration();
            var target = new FileTarget
            {
                FileName = @"C:\Work\Logs\SupportBank.log", Layout = @"${longdate} ${level} - ${logger}: ${message}"
            };
            config.AddTarget("File Logger", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = config;

            logger.Debug("Parsing file " + filePath);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // parse records and ignore invalid transactions
                transactions = csv.GetRecords<Transaction>().Where(x => x.Amount != 0).ToList();
            }

            foreach (var transaction in transactions)
            {
                if (!people.ContainsKey(transaction.From))
                    people.Add(transaction.From, new Person());
                if (!people.ContainsKey(transaction.To))
                    people.Add(transaction.To, new Person());

                people[transaction.From].Balance -= transaction.Amount;
                people[transaction.To].Balance += transaction.Amount;
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