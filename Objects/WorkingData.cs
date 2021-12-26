using Alpaca.Markets;

namespace TradeBot.Objects;

public static class WorkingData
{
    public static Dictionary<string, IMultiPage<IBar>> History { get; set; }
}