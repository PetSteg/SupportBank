using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using CsvHelper;
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

        // Prints everyone's balance
        private static void ListAll()
        {
            foreach (var person in people)
                System.Console.WriteLine(person.Key + ": " + Math.Round(person.Value.Balance, 2));
        }

        // Prints name's transactions 
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

        private static List<Transaction> ParseCSV(string filePath)
        {
            logger.Debug("Parsing CSV file " + filePath);

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // parse records and ignore invalid transactions
                return csv.GetRecords<Transaction>().Where(x => x.Amount != 0).ToList();
            }
        }

        private static List<Transaction> ParseJSON(string filePath)
        {
            logger.Debug("Parsing JSON file " + filePath);
            List<Transaction> newTransactions = new List<Transaction>();

            string jsonString = File.ReadAllText(filePath);
            JArray transactionsJSON = JArray.Parse(jsonString);
            List<JToken> tokens = transactionsJSON.Children().ToList();

            foreach (var token in tokens)
            {
                // read transaction data
                string date = token["Date"]?.ToString();
                string fromAccount = token["FromAccount"]?.ToString();
                string toAccount = token["ToAccount"]?.ToString();
                string narrative = token["Narrative"]?.ToString();
                string amount = token["Amount"]?.ToString();

                // build transaction object
                Transaction transaction = new Transaction(date, fromAccount, toAccount, narrative, amount);

                // validate transaction
                if (transaction.Amount != 0)
                    newTransactions.Add(transaction);
            }

            return newTransactions;
        }

        private static List<Transaction> ParseXML(string filePath)
        {
            logger.Debug("Parsing XML file " + filePath);
            List<Transaction> newTransactions = new List<Transaction>();

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            if (doc.DocumentElement?.ChildNodes == null) return null;

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                // read transaction data
                string from = node.SelectSingleNode("Parties/From")?.InnerText;
                string to = node.SelectSingleNode("Parties/To")?.InnerText;
                string narrative = node.SelectSingleNode("Description")?.InnerText;
                string amount = node.SelectSingleNode("Value")?.InnerText;

                double.TryParse(node.Attributes["Date"].InnerText, out var oaDate);
                string date = DateTime.FromOADate(oaDate).ToString("dd/MM/yyyy");

                // build transaction object
                Transaction transaction = new Transaction(date, from, to, narrative, amount);

                // validate transaction
                if (transaction.Amount != 0)
                    newTransactions.Add(transaction);
            }

            return newTransactions;
        }

        // Imports and applies all transactions from file
        private static void ImportFile(string fileName)
        {
            string filePath = "../../../" + fileName;
            List<Transaction> newTransactions;

            if (fileName.EndsWith(".csv"))
                newTransactions = ParseCSV(filePath);
            else if (fileName.EndsWith(".json"))
                newTransactions = ParseJSON(filePath);
            else if (fileName.EndsWith(".xml"))
                newTransactions = ParseXML(filePath);
            else throw new Exception("Wrong file extension");

            ApplyTransactions(newTransactions);
        }

        private static void ExportFile(string fileName)
        {
            string filePath = "../../../" + fileName + ".csv";

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(transactions);
            }
        }

        private static void PrintTransactions()
        {
            foreach (var transaction in transactions)
                System.Console.WriteLine(transaction.ToString());
        }

        private static void ApplyTransactions(List<Transaction> newTransactions)
        {
            foreach (var transaction in newTransactions)
            {
                if (!people.ContainsKey(transaction.From))
                    people.Add(transaction.From, new Person());
                if (!people.ContainsKey(transaction.To))
                    people.Add(transaction.To, new Person());

                people[transaction.From].Pay(people[transaction.To], transaction.Amount);
            }

            // Concatenate new and old transactions
            transactions.AddRange(newTransactions);
        }

        private static void UserConsole()
        {
            System.Console.WriteLine(
                "Commands:\n\"List All\" -> outputs the names of each person and their balance\n" +
                "\"List [Name]\" -> prints a list of every transaction for the account with this name\n" +
                "\"Import File [filename]\" -> Reads and applies all transactions from file\n" +
                "\"Export File [filename]\" -> Writes all applied transactions to file");

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
                else if (input.Length > 12 && input.Substring(0, 12) == "Import File ")
                {
                    string fileName = input.Substring(12);
                    try
                    {
                        ImportFile(fileName);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Couldn't parse file.");
                        logger.Error("Wrong file name: " + fileName + ". " + e.Message);
                    }
                }
                else if (input.Length > 12 && input.Substring(0, 12) == "Export File ")
                {
                    string fileName = input.Substring(12);
                    try
                    {
                        ExportFile(fileName);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Couldn't export file.");
                        logger.Error("Couldn't export file: " + fileName + ". " + e.Message);
                    }
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

            UserConsole();
        }
    }
}