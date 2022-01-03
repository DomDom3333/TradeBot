using System.Text;
using TradeBot.Objects.Stocks;

namespace TradeBot.Objects;

internal class Logger
{
    internal Dictionary<Guid, ConsoleTextContainer> ConsoleLines { get; } =
        new Dictionary<Guid, ConsoleTextContainer>();
    

    internal Guid AddLine(string text = "@Symbol: Target: @Target - Current: @Current - Position: @Position")
    {
        ConsoleTextContainer ctc = new ConsoleTextContainer(text);
        Guid lineGuid = Guid.NewGuid();
        ConsoleLines.Add(lineGuid, ctc);
        return lineGuid;
    }

    internal void AddParams(Guid lineId ,params (string, string)[] parameters)
    {
        foreach ((string key, string value) in parameters)
        {
            ConsoleLines[lineId].AddParameter(key, value);
        }
    }

    internal void RemoveLine(Guid lineToRemove)
    {
        ConsoleLines.Remove(lineToRemove);
    }

    internal void UpdateLogData(Stock stock)
    {
        ConsoleLines[stock.LogId].EditParameter("@Symbol", stock.Symbol);
        ConsoleLines[stock.LogId].EditParameter("@Target", stock.AverageSell.ToString());
        ConsoleLines[stock.LogId].EditParameter("@Current", stock.LastQuote?.BidPrice.ToString());
        ConsoleLines[stock.LogId].EditParameter("@Position", stock.HasPosition?stock.Position.Profit.ToString():"No");
    }

    internal void UpdateConsole()
    {
        Console.SetCursorPosition(0,0);
        Console.SetCursorPosition(0, ConsoleLines.Count-1);
        foreach (var line in ConsoleLines)
        {
            Console.WriteLine(line.Value.FinishedText);
        }
        Console.SetCursorPosition(0,0);
    }

}

internal class ConsoleTextContainer
{
    public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();
    public StringBuilder Line { get; } = new StringBuilder();


    internal ConsoleTextContainer(string line)
    {
        Line = new StringBuilder(line);
    }
    public string FinishedText
    {
        get
        {
            
            string outputText = Line.ToString();
            foreach (KeyValuePair<string,string> parameter in Parameters)
            {
                while (outputText.Contains(parameter.Key))
                {
                    outputText = outputText.Replace(parameter.Key, parameter.Value);
                }
            }

            return outputText;
        }
    }

    internal void AddParameter(string parameter, string replacingString)
    {
        Parameters.Add(parameter, replacingString);
    }

    internal void EditParameter(string parameter, string newString)
    {
        Parameters[parameter] = newString;
    }

    internal void RemoveParameter(string parameter)
    {
        Parameters.Remove(parameter);
    }
}