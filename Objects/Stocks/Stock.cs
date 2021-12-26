using Alpaca.Markets;
using CodeResources.Api;
using TradeBot.CodeResources;

namespace Objects.Stocks
{
    class Stock
    {
        public Stock(string name, string symbolName, Guid code, AssetClass type)
        {
            Name = name;
            Symbol = symbolName;
            Code = code;
            SType = type;
        }
        
        public string Name { get; init; }
        public string Symbol { get; init; }
        public Guid Code { get; init; }
        public AssetClass SType { get; init; }
        public bool ProcessingLock { get; set; }

        public bool HasPosition
        {
            get
            {
                return !(Position == null);
            }
        }
        public PositionInformation Position { get; set; }

        public decimal AverageBuy { get; set; }
        public decimal AverageSell { get; set; }

        private IAlpacaDataSubscription<ITrade> _TradeSub;
        public IAlpacaDataSubscription<ITrade> TradeSub
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
        public IAlpacaDataSubscription<IQuote> QuoteSub
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
        public IReadOnlyList<IBar> MinutelyBarData { get; private set; }
        public IReadOnlyList<IQuote> MinutelyPriceData { get; private set; }

        public class PositionInformation
        {
            public decimal QuantityOwned { get; set; } = 0;
            public decimal BuyPrice { get; set; } = 0;
            public decimal? ChangePercent { get; set; } = 0;
            public decimal? CurrentPrice { get; set; } = 0;
            public decimal Profit
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
            public PositionInformation(decimal quantityOwned, decimal buyPrice, decimal? changePercent, decimal? currentPrice)
            {
                QuantityOwned = quantityOwned;
                BuyPrice = buyPrice;
                ChangePercent = changePercent;
                CurrentPrice = currentPrice;
            }
        }
    }
        
}
