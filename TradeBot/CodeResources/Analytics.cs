using System.Runtime.InteropServices;
using Alpaca.Markets;
using TradeBot.Objects.Stocks;

namespace TradeBot.CodeResources;

public static class Analytics
{
    internal static void GetBarsSummary(Stock stock)
    {
        decimal totalBuy = 0;
        decimal totalSell = 0;
        int numRows = 0;
        
        List<IBar> listToAverage = stock.HourlyBarData.ToList();
        if (stock.SType == AssetClass.Crypto)
        {
            listToAverage = listToAverage.Take(stock.HouerlyPriceData.Count / 2).ToList();
        }
        foreach (IBar bar in listToAverage)
        {
            //Filter out invalid datapoints to preserve result integrity
            if (bar.Volume == 0)
                continue;
            
            totalBuy += bar.Vwap;
            numRows++;
            //Does nothing yet.
        }
        if (numRows < 1)
            return;
        stock.AverageSell = totalBuy / numRows;
        stock.AverageBuy = totalBuy / numRows;

    }
    internal static void GetAverageBuySell(Stock stock)
    {
        decimal totalBuy = 0;
        decimal totalSell = 0;
        int numRows = 0;
        List<IQuote> listToAverage = stock.HouerlyPriceData.ToList();
        if (stock.SType == AssetClass.Crypto)
        {
            listToAverage = listToAverage.Take(stock.HouerlyPriceData.Count / 2).ToList();
        }
        foreach (IQuote quote in listToAverage)
        {
            //Filter out invalid datapoints to preserve result integrity
            if (quote.AskPrice == 0 || quote.BidPrice == 0)
                continue;
            
            totalBuy += quote.AskPrice;
            totalSell += quote.BidPrice;
            numRows ++;
        }
        if (numRows < 1)
            return;
        
        //stock.AverageSell = totalSell/numRows;
        //stock.AverageBuy = totalBuy/numRows;
    }
}