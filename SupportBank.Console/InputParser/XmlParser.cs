using System;
using System.Collections.Generic;
using System.Xml;
using NLog;

namespace SupportBank.Console.InputParser
{
    public class XmlParser : IInputParser
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public List<Transaction> ParseInput(string filePath)
        {
            logger.Debug("Parsing XML file " + filePath);
            var newTransactions = new List<Transaction>();

            var doc = new XmlDocument();
            doc.Load(filePath);
            if (doc.DocumentElement?.ChildNodes == null) return null;

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                // read transaction data
                var from = node.SelectSingleNode("Parties/From")?.InnerText;
                var to = node.SelectSingleNode("Parties/To")?.InnerText;
                var narrative = node.SelectSingleNode("Description")?.InnerText;
                var amount = node.SelectSingleNode("Value")?.InnerText;

                double.TryParse(node.Attributes["Date"].InnerText, out var oaDate);
                var date = DateTime.FromOADate(oaDate).ToString("dd/MM/yyyy");

                // build transaction object
                var transaction = new Transaction(date, from, to, narrative, amount);

                // validate transaction
                if (transaction.Amount != 0)
                    newTransactions.Add(transaction);
            }

            return newTransactions;
        }
    }
}