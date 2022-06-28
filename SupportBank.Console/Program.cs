using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private static void ListAll()
        {
            foreach (var person in people)
                System.Console.WriteLine(person.Key + ": " + person.Value.Balance);
        }

        private static void ListAccount(string name)
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

        private static void ParseCSV(string filePath)
        {
            logger.Debug("Parsing CSV file " + filePath);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // parse records and ignore invalid transactions
                transactions = csv.GetRecords<Transaction>().Where(x => x.Amount != 0).ToList();
            }
        }

        private static void ParseJSON(string filePath)
        {
            logger.Debug("Parsing JSON file " + filePath);
            string json = File.ReadAllText(filePath);
            JArray transactionsJSON = JArray.Parse(json);

            List<JToken> tokens = transactionsJSON.Children().ToList();

            foreach (var token in tokens)
            {
                string date = token["Date"].ToString();
                string fromAccount = token["FromAccount"].ToString();
                string toAccount = token["ToAccount"].ToString();
                string narrative = token["Narrative"].ToString();
                string amount = token["Amount"].ToString();

                Transaction transaction = new Transaction(date, fromAccount, toAccount, narrative, amount);
                transactions.Add(transaction);
            }
        }

        private static void ParseXML(string filePath)
        {
            logger.Debug("Parsing XML file " + filePath);
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string date = DateTime.Today.ToString("dd/MM/yyyy");
                string from = node.SelectSingleNode("Parties/From")?.InnerText;
                string to = node.SelectSingleNode("Parties/To")?.InnerText;
                string narrative = node.SelectSingleNode("Description")?.InnerText;
                string amount = node.SelectSingleNode("Value")?.InnerText;

                Transaction transaction = new Transaction(date, from, to, narrative, amount);
                transactions.Add(transaction);
            }
        }

        private static void PrintTransactions()
        {
            foreach (var transaction in transactions)
            {
                System.Console.WriteLine(transaction.ToString());
            }
        }

        private static void ApplyTransactions()
        {
            foreach (var transaction in transactions)
            {
                if (!people.ContainsKey(transaction.From))
                    people.Add(transaction.From, new Person());
                if (!people.ContainsKey(transaction.To))
                    people.Add(transaction.To, new Person());

                people[transaction.From].Pay(people[transaction.To], transaction.Amount);
            }
        }

        private static void UserConsole()
        {
            System.Console.WriteLine(
                "Commands:\n\"List All\" -> outputs the names of each person and their balance\n" +
                "\"List [Name]\" -> prints a list of every transaction for the account with this name");

            string input;
            while ((input = System.Console.ReadLine()) != null && input.Length > 0)
            {
                if (input == "List All")
                {
                    ListAll();
                }
                else if (input.Length > 5 && input.Substring(0, 5) == "List ")
                {
                    string name = input.Substring(5);
                    ListAccount(name);
                }
                else System.Console.WriteLine("Wrong command");
            }
        }

        public static void Main()
        {
            var config = new LoggingConfiguration();
            var target = new FileTarget
            {
                FileName = @"C:\Work\Logs\SupportBank.log", Layout = @"${longdate} ${level} - ${logger}: ${message}"
            };
            config.AddTarget("File Logger", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = config;

            // ParseCSV("../../../DodgyTransactions2015.csv");
            // ParseJSON("../../../Transactions2013.json");
            ParseXML("../../../Transactions2012.xml");

            // PrintTransactions();
            ApplyTransactions();

            UserConsole();
        }
    }
}