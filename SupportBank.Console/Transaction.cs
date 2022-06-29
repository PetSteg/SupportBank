using System;
using NLog;

namespace SupportBank.Console
{
    public class Transaction
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public DateTime Date { get; }
        public string From { get; }
        public string To { get; }
        public string Narrative { get; }
        public double Amount { get; }

        public Transaction(String Date, string From, string To, string Narrative, string Amount)
        {
            logger.Debug(Date + "," + From + "," + To + "," + Narrative + "," + Amount);

            this.From = From;
            this.To = To;
            this.Narrative = Narrative;

            // amount is 0 for invalid transactions
            // parse amount
            if (!double.TryParse(Amount, out var amount))
                logger.Error("Wrong amount format");
            this.Amount = amount;

            // parse date
            DateTime.TryParse(Date, out var date);
            this.Date = date;
            if (date == DateTime.MinValue)
            {
                logger.Error("Wrong date format");
                this.Amount = 0;
            }
        }

        public override string ToString()
        {
            return Date.ToString("dd/MM/yyyy") + " " + From + " -> " + To + " (" + Narrative + ") " + Amount;
        }
    }
}