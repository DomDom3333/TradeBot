using Alpaca.Markets;
using Objects.Stocks;

namespace TradeBot.CodeResources;

public static class Analytics
{
    internal static void GetBarsSummary(Stock stock)
    {
        decimal totalBuy = 0;
        decimal totalSell = 0;
        int numRows = 0;


        foreach (IQuote quote in stock.HouerlyPriceData)
        {
            totalBuy += quote.BidPrice;
            totalSell += quote.AskPrice;
            numRows ++;
        }

        stock.AverageSell = totalSell/numRows;
        stock.AverageBuy = totalBuy/numRows;
    }
    internal static void GetAverageBuySell(Stock stock)
    {
        decimal totalBuy = 0;
        decimal totalSell = 0;
        int numRows = 0;
        
        foreach (IQuote quote in stock.HouerlyPriceData)
        {
            totalBuy += quote.BidPrice;
            totalSell += quote.AskPrice;
            numRows ++;
        }

        if (numRows < 1)
            return;
        
        stock.AverageSell = totalSell/numRows;
        stock.AverageBuy = totalBuy/numRows;
    }
}