using Alpaca.Markets;

namespace TradeBot.Strategies;

internal class MainStrategy
{
    internal void RunStrategy(ITrade trade)
    {
        Console.WriteLine($"A Trade with {trade.Symbol} was made. {trade.Size} were {trade.TakerSide}");
        
        
        //Things to do when trade comes in:
        //Check if Historic data is avaliable
        //if not avaliable,load data
        //run the strategy logic based on historic data
    }
}