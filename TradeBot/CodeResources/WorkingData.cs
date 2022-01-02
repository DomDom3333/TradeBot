using Alpaca.Markets;
using TradeBot.Objects.Stocks;

namespace TradeBot.CodeResources;

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
            return StockList.Where(x => x.HasPosition).Count();
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