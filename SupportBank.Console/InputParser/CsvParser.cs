using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using NLog;

namespace SupportBank.Console
{
    public class CsvParser : IInputParser
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public List<Transaction> ParseInput(string filePath)
        {
            logger.Debug("Parsing CSV file " + filePath);

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // parse records and ignore invalid transactions
                return csv.GetRecords<Transaction>().Where(x => x.Amount != 0).ToList();
            }
        }
    }
}