using Objects.Stocks;
using TradeBot.Objects.Stocks;

namespace TradeBot.CodeResources;

internal static class CurrentStockData
{
    internal static List<Stock> Cryptos { get; } = new List<Stock>();
    internal static List<Stock> CompanyStock { get; } = new List<Stock>();
}