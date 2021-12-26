using Alpaca.Markets;
using CodeResources.Api;
using Objects.Stocks;

namespace TradeBot.Objects;

internal static class WorkingData
{
    internal static IAccount Account { get; private set; }
    internal static List<Stock> StockList { get; } = new List<Stock>();

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