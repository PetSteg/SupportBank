using System;
using Microsoft.SqlServer.Server;
using NLog;

namespace SupportBank.Console
{
    public class Transaction
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public Transaction(String Date, string From, string To, string Narrative, string Amount)
        {
            logger.Debug(Date + "," + From + "," + To + "," + Narrative + "," + Amount);

            this.From = From;
            this.To = To;
            this.Narrative = Narrative;

            // amount is 0 for invalid transactions
            // parse amount
            double amount;
            if (!double.TryParse(Amount, out amount))
                logger.Error("Wrong amount format");
            this.Amount = amount;

            // parse date
            DateTime date;
            DateTime.TryParse(Date, out date);
            this.Date = date;
            if (date == DateTime.MinValue)
            {
                logger.Error("Wrong date format");
                this.Amount = 0;
            }
        }

        public DateTime Date { get; }
        public string From { get; }
        public string To { get; }
        public string Narrative { get; }
        public double Amount { get; }

        public override string ToString()
        {
            return Date.ToString("dd/MM/yyyy") + " " + From + " -> " + To + " (" + Narrative + ") " + Amount;
        }
    }
}