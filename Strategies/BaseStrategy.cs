using Alpaca.Markets;
using CodeResources.Api;
using Objects.Stocks;
using TradeBot.Objects.Stocks;

namespace TradeBot.Strategies;

internal abstract class BaseStrategy:IBaseStrategy<BaseStrategy>
{
    internal decimal TargetPercentincrease { get; set; }


    public abstract void RunQuoteStrategy(IQuote quote, Stock stock);

    public abstract void RunTradeStrategy(ITrade trade, Stock stock);
}