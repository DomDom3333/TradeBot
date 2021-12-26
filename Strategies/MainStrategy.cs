using Alpaca.Markets;
using CodeResources.Api;

namespace TradeBot.Strategies;

internal class MainStrategy
{
    internal void RunStrategy(ITrade trade)
    {
        Console.WriteLine($"A Trade with {trade.Symbol} was made. {trade.Size} were {trade.TakerSide}");

        if (!Objects.WorkingData.History.ContainsKey(trade.Symbol))
        {
            Objects.WorkingData.History.Add(trade.Symbol, ApiRecords.CryptoDataClient.GetHistoricalBarsAsync(new HistoricalCryptoBarsRequest(trade.Symbol,
                DateTime.Today.AddYears(-1), DateTime.Now, BarTimeFrame.Minute)).Result);
        }

        Console.WriteLine();
        //Things to do when trade comes in:
        //Check if Historic data is avaliable
        //if not avaliable,load data
        //run the strategy logic based on historic data
    }
}