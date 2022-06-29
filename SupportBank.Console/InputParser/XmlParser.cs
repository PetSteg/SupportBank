using System;
using System.Collections.Generic;
using System.Xml;
using NLog;

namespace SupportBank.Console
{
    public class XmlParser : IInputParser
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public List<Transaction> ParseInput(string filePath)
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
    }
}