using System.Collections.Generic;

namespace SupportBank.Console
{
    public interface IInputParser
    {
        List<Transaction> ParseInput(string filePath);
    }
}