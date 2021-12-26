using Alpaca.Markets;
using CodeResources.Api;
using Objects.Stocks;

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
    internal void RunStrategy(IQuote quote, Stock stock)
    {
        Console.WriteLine($"{stock.Name} Update!{Environment.NewLine}Ask price:{quote.AskPrice}. Bid Size: {quote.BidPrice}");

        if (stock.HasPosition)
        {
            stock.Position = ApiUtils.GetlatestPosition(stock);
        }
        
        
        
        //Things to do when trade comes in:
        //Check if Historic data is avaliable
        //if not avaliable,load data
        //run the strategy logic based on historic data
    }
}