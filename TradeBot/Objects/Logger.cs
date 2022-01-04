using System.Text;
using TradeBot.Objects.Stocks;

namespace TradeBot.Objects;

internal class Logger
{
    internal Dictionary<Guid, ConsoleTextContainer> ConsoleLines { get; } =
        new Dictionary<Guid, ConsoleTextContainer>();
    

    internal Guid AddLine(string text = "@Symbol: Target: @Target - Current: @Current - Position: @Position - Trend: @Trend")
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
        ConsoleLines[stock.LogId].EditParameter("@Target", (Math.Truncate(100 * stock.AverageSell)/100).ToString());
        ConsoleLines[stock.LogId].EditParameter("@Current", stock.LastQuote?.BidPrice.ToString());
        ConsoleLines[stock.LogId].EditParameter("@Position", (stock.HasPosition ? stock.Position.ChangePrice.ToString() : "No"));
        ConsoleLines[stock.LogId].EditParameter("@Trend", stock.LastHourPositiveTrend ? "UP" : "DOWN");
    }

    internal void UpdateConsole()
    {
        Console.Clear();
        Console.SetCursorPosition(0,0);
        foreach (var line in ConsoleLines)
        {
            Console.WriteLine(line.Value.FinishedText);
        }
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