using System.Collections.Generic;

namespace SupportBank.Console.InputParser
{
    public interface IInputParser
    {
        List<Transaction> ParseInput(string filePath);
    }
}