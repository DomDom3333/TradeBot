using Alpaca.Markets;
using CodeResources.Api;
using TradeBot.CodeResources;

namespace Objects.Stocks
{
    internal class Stock
    {
        internal Stock(string name, string symbolName, Guid code, AssetClass type)
        {
            Name = name;
            Symbol = symbolName;
            Code = code;
            SType = type;
        }
        
        internal string Name { get; init; }
        internal string Symbol { get; init; }
        internal Guid Code { get; init; }
        internal AssetClass SType { get; init; }
        internal bool ProcessingLock { get; set; }

        internal bool HasPosition
        {
            get
            {
                return !(Position == null);
            }
        }
        internal PositionInformation Position { get; set; }

        internal decimal AverageBuy { get; set; }
        internal decimal AverageSell { get; set; }

        private IAlpacaDataSubscription<ITrade> _TradeSub;
        internal IAlpacaDataSubscription<ITrade> TradeSub
        {
            get
            {
                if (_TradeSub == null)
                {
                    _TradeSub = ApiRecords.CryptoStreamingClient.GetTradeSubscription(Symbol);
                }

                return _TradeSub;
            }
        }

        private IAlpacaDataSubscription<IQuote> _QuoteSub;
        internal IAlpacaDataSubscription<IQuote> QuoteSub
        {
            get
            {
                if (_QuoteSub == null)
                {
                    _QuoteSub = ApiRecords.CryptoStreamingClient.GetQuoteSubscription(Symbol);
                }

                return _QuoteSub;
            }
        }

        internal void UpdateHostoricalData(IReadOnlyList<IBar> barHistory, IReadOnlyList<IQuote> quoteHostory)
        {
            MinutelyBarData = barHistory;
            MinutelyPriceData = quoteHostory;
            Analytics.GetAverageBuySell(this);
        }
        internal IReadOnlyList<IBar> MinutelyBarData { get; private set; }
        internal IReadOnlyList<IQuote> MinutelyPriceData { get; private set; }

        internal class PositionInformation
        {
            public DateTime BuyDate { get; init; }
            internal decimal QuantityOwned { get; init; } = 0;
            internal decimal BuyPrice { get; init; } = 0;
            internal decimal? ChangePercent { get; init; } = 0;
            internal decimal? CurrentPrice { get; init; } = 0;
            internal decimal Profit
            {
                get
                {
                    if (CurrentPrice.HasValue)
                    {
                        return CurrentPrice.Value - BuyPrice;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            internal PositionInformation(decimal quantityOwned, decimal buyPrice, decimal? changePercent, decimal? currentPrice)
            {
                QuantityOwned = quantityOwned;
                BuyPrice = buyPrice;
                ChangePercent = changePercent;
                CurrentPrice = currentPrice;
            }
        }
    }
        
}
