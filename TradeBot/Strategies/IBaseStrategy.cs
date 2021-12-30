using Alpaca.Markets;
using Objects.Stocks;
using TradeBot.Objects.Stocks;

namespace TradeBot.Strategies;

internal interface IBaseStrategy<T>
{
    public bool HasQuoteStrat { get; set; }
    public bool HasTradeStrat { get; set; }
    /// <summary>
    /// This Gets triggered every time there is a change in the Quote (Buy price, Sell price etc)
    /// </summary>
    /// <param name="quote">The new quote</param>
    /// <param name="stock">The stock which was affected</param>
    public void RunQuoteStrategy(IQuote quote, Stock stock);
    /// <summary>
    /// This Gets triggered every time a Trade happens
    /// </summary>
    /// <param name="trade">The Trade which occurred</param>
    /// <param name="stock">The stock which was traded</param>
    public void RunTradeStrategy(ITrade trade, Stock stock);
}