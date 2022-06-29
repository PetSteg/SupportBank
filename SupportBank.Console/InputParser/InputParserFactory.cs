using System;

namespace SupportBank.Console.InputParser
{
    public class InputParserFactory
    {
        public IInputParser GetInputParser(string filePath)
        {
            if (filePath.EndsWith(".csv"))
                return new CsvParser();
            if (filePath.EndsWith(".json"))
                return new JsonParser();
            if (filePath.EndsWith(".xml"))
                return new XmlParser();
            throw new Exception("Wrong file extension");
        }
    }
}