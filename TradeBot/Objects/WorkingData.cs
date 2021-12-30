using Alpaca.Markets;
using CodeResources.Api;
using Objects.Stocks;
using TradeBot.CodeResources;
using TradeBot.Objects.Stocks;

namespace TradeBot.Objects;

internal static class WorkingData
{
    internal static IAccount Account { get; private set; }
    internal static List<Stock> StockList { get; } = new List<Stock>();
    internal static List<string> PurchasedSymbols { get; set; } = new List<string>();
    internal static IClock StockClock { get; set; }

    internal static int CurrentlyHolding
    {
        get
        {
            return WorkingData.StockList.Where(x => x.HasPosition).Count();
        }
    }
    
    internal static void AddAccount(IAccount acc)
    {
        if (Account != null)
        {
            throw new Exception("Account can not be switched while application is running.");
        }
        else
        {
            Account = acc;
        }
    }
    
}