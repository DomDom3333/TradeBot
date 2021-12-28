using Alpaca.Markets;
using CodeResources.Api;
using Objects.Stocks;
using TradeBot.CodeResources;
using TradeBot.Objects;

namespace TradeBot.Strategies;

internal class CustomDom : BaseStrategy,IBaseStrategy<BaseStrategy>
{
    private decimal PurchaseQuantity()
    {
        if (!WorkingData.Account.BuyingPower.HasValue)
        {
            return 0;
        }

        return WorkingData.Account.BuyingPower.Value / (Appsettings.Main.MaximumHoldings - WorkingData.CurrentlyHolding);
    }
    public override void RunTradeStrategy(ITrade trade, Stock stock)
    {
        Console.WriteLine($"A Trade with {trade.Symbol} was made. {trade.Size} were {trade.TakerSide}");

        //Things to do when trade comes in:
        //Check if Historic data is avaliable
        //if not avaliable,load data
        //run the strategy logic based on historic data
    }

    public override void RunQuoteStrategy(IQuote quote, Stock stock)
    {
        Console.WriteLine($"Processing {stock.Name} change.");
        if (stock.HasPosition)
        {
            PositionResponse(stock);
            return;
        }
        else
        {
            NoPositionResponse(stock);
            return;
        }
    }

    private void NoPositionResponse(Stock stock)
    {
        IQuote latestBar = null;
        try
        {
            switch (stock.SType)
            {
                case AssetClass.UsEquity:
                    latestBar = ApiRecords.DataClient.GetLatestQuoteAsync(stock.Symbol).Result;
                    break;
                case AssetClass.Crypto:
                    latestBar = ApiRecords.CryptoDataClient.GetLatestQuoteAsync(new LatestDataRequest(stock.Symbol, (CryptoExchange) stock.Exchange)).Result;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        catch (Exception e)
        {
        }
        if (latestBar == null)
        {
            Console.WriteLine($"Latest Bar from {stock.Name} did not load correctly! NOT ACTING TO PREVENT ERRORS!!");
            return;
        }
        if (latestBar.BidPrice < stock.AverageBuy + stock.AgressionBuyOffset)
        {
            stock.BuyStock(PurchaseQuantity());
        }
        else
        {
            Console.WriteLine($"{stock.Name} not low enough.");
        }
    }

    internal void PositionResponse(Stock stock)
    {
        stock.Position = ApiUtils.GetlatestPosition(stock);

        if (stock.Position == null)
        {
            Console.WriteLine($"Latest Position from {stock.Name} did not load correctly! NOT ACTING TO PREVENT ERRORS!!");
            return;
        }
        
        if (!stock.Position.Profit)
            return;
        if (stock.Position.CurrentPrice < (stock.AverageSell + stock.AgressionSellOffset))
            return;

        if (stock.LastHourPositiveTrend)
            return;
        
        stock.ClosePosition();
        
    }
    
}