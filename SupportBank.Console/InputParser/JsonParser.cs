using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;

namespace SupportBank.Console
{
    public class JsonParser : IInputParser
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public List<Transaction> ParseInput(string filePath)
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
    }
}