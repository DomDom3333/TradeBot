using Alpaca.Markets;
using CodeResources.Api;
using Objects.Stocks;
using TradeBot.CodeResources;
using TradeBot.CodeResources.Api;
using TradeBot.Objects;
using TradeBot.Objects.Stocks;

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
        //Console.WriteLine($"A Trade with {trade.Symbol} was made. {trade.Size} were {trade.TakerSide}");
    }

    public override bool HasQuoteStrat { get; set; } = true;
    public override bool HasTradeStrat { get; set; } = false;

    public override void RunQuoteStrategy(IQuote quote, Stock stock)
    {
        stock.Log.UpdateLog(quote);
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

        if (!stock.LastHourPositiveTrend)
            return;
        if (WorkingData.CurrentlyHolding >= Appsettings.Main.MaximumHoldings)
            return;
        decimal target = stock.AverageBuy + stock.AgressionBuyOffset;
        if (target == 0)
            return;
        
        if (stock.LastBuy.AddSeconds(5) < DateTime.Now && !WorkingData.PurchasedSymbols.Contains(stock.Symbol) && latestBar.AskPrice < target && !stock.HasPosition)
        {
            stock.BuyStock(PurchaseQuantity());
        }
    }

    internal void PositionResponse(Stock stock)
    {
        stock.Position = ApiUtils.GetLatestPosition(stock);

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