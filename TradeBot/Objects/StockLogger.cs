using System.Text;
using Alpaca.Markets;
using TradeBot.CodeResources;

namespace TradeBot.Objects;

internal class StockLogger
{
    internal StockLogger(Stocks.Stock stock, System.Timers.Timer loggingInterval)
    {
        Stock = stock;
        LoggingInterval = loggingInterval;
    }

    private Stocks.Stock Stock { get; set; }
    internal System.Timers.Timer LoggingInterval { get; private set; }
    private StringBuilder StringBuilder { get; set; } = new StringBuilder();

    public int TimesLogged { get; set; }
    public bool WasBought { get; set; }
    public bool WasSold { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal LastBuyCheck { get; set; }
    public decimal LastSellCheck { get; set; }

    public string Trending
    {
        get
        {
            if (Open > Close)
            {
                return "down";
            }
            else
            {
                return "UP";
            }
        }
    }

    public string Response
    {
        get
        {
            if (WasBought && !WasSold)
            {
                return "BUY";
            }
            if (WasSold && !WasBought)
            {
                return "SELL";
            }
            if (!WasBought && !WasSold)
            {
                return "NOTHING";
            }
            if (WasBought && WasSold)
            {
                return "BUY AND SELL";
            }

            return string.Empty;
        }
    }

    internal void UpdateLog(IQuote quote)
    {
        if (Open == 0)
        {
            Open = quote.BidPrice;
        }
        Close = quote.BidPrice;
        LastBuyCheck = quote.AskPrice;
        LastSellCheck = quote.BidPrice;
        
        if (High < quote.BidPrice)
        {
            High = quote.BidPrice;
        }
        if (Low > quote.BidPrice || Low == 0)
        {
            Low = quote.BidPrice;
        }

        TimesLogged++;
    }

    internal void ResetLog()
    {
        TimesLogged = 0;
        WasBought = false;
        WasSold = false;
        High = 0;
        Low = decimal.MaxValue;
        Open = 0;
        Close = 0;
        LastBuyCheck = 0;
        LastSellCheck = 0;
        StringBuilder = new StringBuilder();
    }
    

    internal string ReturnLog()
    {
        if (!WorkingData.StockClock.IsOpen && Stock.SType == AssetClass.UsEquity)
        {
            return $"Exchange for {Stock.Symbol} is closed. No Change.{Environment.NewLine}";
        }
        else if (TimesLogged < 1 || Open == 0)
        {
            return $"There have been no new Trades since last Update on {Stock.Symbol}. No Change.{Environment.NewLine}";
        }

        StringBuilder.Append(
            $"Result of {Stock.Symbol} over the last {LoggingInterval.Interval / 1000}s after {TimesLogged} checks.{Environment.NewLine}");
        StringBuilder.Append($"High: {High}, Low: {Low}, Trending {Trending}, Last Prices: B={LastBuyCheck} | S={LastSellCheck}. {Environment.NewLine}");
        if (Stock.HasPosition)
        {
            StringBuilder.Append($"Current Position change since purchase: ${Stock.Position.ChangePrice}{Environment.NewLine}");
            StringBuilder.Append($"Sales Target: {Stock.AverageSell} - Current: {Stock.Position.CurrentPrice}{Environment.NewLine}");
        }
        else
        {
            StringBuilder.Append($"Buy Target: {Stock.AverageSell} - Current: {Stock.LastQuote.AskPrice}{Environment.NewLine}");
        }
        StringBuilder.Append($"Response: {Response}.{Environment.NewLine}");
        if (WasBought && Stock.HasPosition)
        {
            StringBuilder.Append($"{Stock.Symbol} WAS BOUGHT AT {Stock.Position.BuyPrice} AT {Stock.Position.BuyDate}!!!!");
        }
        if (WasSold && !Stock.HasPosition)
        {
            StringBuilder.Append($"{Stock.Symbol} WAS SOLD AT {Stock.LastSale}. THE PROFIT WAS {Stock.LastProfit}!!!!");
        }
        if (!WasSold && !WasBought)
        {
            StringBuilder.Append("There was no Position change.");
        }
        StringBuilder.Append(Environment.NewLine);
        
        return StringBuilder.ToString();
    }
    
}