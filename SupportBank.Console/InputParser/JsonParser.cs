using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;

namespace SupportBank.Console.InputParser
{
    public class JsonParser : IInputParser
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public List<Transaction> ParseInput(string filePath)
        {
            logger.Debug("Parsing JSON file " + filePath);
            var newTransactions = new List<Transaction>();

            var jsonString = File.ReadAllText(filePath);
            var transactionsJSON = JArray.Parse(jsonString);
            var tokens = transactionsJSON.Children().ToList();

            foreach (var token in tokens)
            {
                // read transaction data
                var date = token["Date"]?.ToString();
                var fromAccount = token["FromAccount"]?.ToString();
                var toAccount = token["ToAccount"]?.ToString();
                var narrative = token["Narrative"]?.ToString();
                var amount = token["Amount"]?.ToString();

                // build transaction object
                var transaction = new Transaction(date, fromAccount, toAccount, narrative, amount);

                // validate transaction
                if (transaction.Amount != 0)
                    newTransactions.Add(transaction);
            }

            return newTransactions;
        }
    }
}