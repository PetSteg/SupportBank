using System;
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

        // Imports and applies all transactions from file
        private static void ImportFile(string fileName)
        {
            string filePath = "../../../" + fileName;

            // create the right InputParser
            IInputParser inputParser = new InputParserFactory().GetInputParser(filePath);

            List<Transaction> newTransactions = inputParser.ParseInput(filePath);
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
                else if (input.StartsWith("List "))
                {
                    string name = input.Substring(5);
                    ListAccount(name);
                }
                else if (input.StartsWith("Import File "))
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
                else if (input.StartsWith("Export File "))
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