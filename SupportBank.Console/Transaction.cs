using System;

namespace SupportBank.Console
{
    public class Transaction
    {
        public Transaction(string Date, string From, string To, string Narrative, double Amount)
        {
            this.Date = Date;
            this.From = From;
            this.To = To;
            this.Narrative = Narrative;
            this.Amount = Amount;
        }

        public string Date { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Narrative { get; set; }
        public double Amount { get; set; }

        public override string ToString()
        {
            return Date + " " + From + " -> " + To + " (" + Narrative + ") " + Amount;
        }
    }
}