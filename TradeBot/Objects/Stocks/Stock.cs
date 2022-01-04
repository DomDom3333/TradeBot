using System.Collections.ObjectModel;
using Alpaca.Markets;
using TradeBot.CodeResources;
using TradeBot.CodeResources.Api;

namespace TradeBot.Objects.Stocks
{
    internal class Stock
    {
        internal Stock(string name, string symbolName, Guid code, AssetClass type, int exchange, System.Timers.Timer loggingInterval)
        {
            Name = name;
            Symbol = symbolName;
            Code = code;
            SType = type;
            switch (type)
            {
                case AssetClass.UsEquity:
                    SExchange = (Exchange) exchange;
                    break;
                case AssetClass.Crypto:
                    CExchange = (CryptoExchange) exchange;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            Log = new StockLogger(this, loggingInterval);
            //Program.TimeKeeper.AddSub(loggingInterval, ReturnLogEntries);
        }
        
        internal string Name { get; init; }
        internal string Symbol { get; init; }
        internal Guid Code { get; init; }
        internal AssetClass SType { get; init; }
        private CryptoExchange CExchange { get; init; }
        private Exchange SExchange { get; init; }
        public int Exchange
        {
            get
            {
                switch (SType)
                {
                    case AssetClass.UsEquity:
                        return (int) SExchange;
                    case AssetClass.Crypto:
                        return (int) CExchange;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        internal bool ProcessingLock { get; set; }

        internal bool HasPosition
        {
            get
            {
                return !(Position == null);
            }
        }
        internal PositionInformation? Position { get; set; }
        internal decimal LastSale { get; set; }
        internal decimal LastProfit { get; set; }
        public IQuote LastQuote { get; set; }
        public DateTime LastBuy { get; set; } = DateTime.Now;

        internal decimal AverageBuy { get; set; }
        internal decimal AverageSell { get; set; }

        private IAlpacaDataSubscription<ITrade>? _TradeSub;
        internal IAlpacaDataSubscription<ITrade> TradeSub
        {
            get
            {
                if (_TradeSub == null)
                {
                    if (SType == AssetClass.Crypto)
                    {
                        _TradeSub = ApiRecords.CryptoStreamingClient.GetTradeSubscription(Symbol);
                    }
                    else if (SType == AssetClass.UsEquity)
                    {
                        _TradeSub = ApiRecords.DataStreamingClinet.GetTradeSubscription(Symbol);
                    }
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
                    if (SType == AssetClass.Crypto)
                    {
                        _QuoteSub = ApiRecords.CryptoStreamingClient.GetQuoteSubscription(Symbol);
                    }
                    else if (SType == AssetClass.UsEquity)
                    {
                        _QuoteSub = ApiRecords.DataStreamingClinet.GetQuoteSubscription(Symbol);
                    }
                }

                return _QuoteSub;
            }
        }

        internal void UpdateHostoricalData(IReadOnlyList<IBar> barHistory, IReadOnlyList<IQuote> quoteHistory)
        {
            HourlyBarData = barHistory;
            HouerlyPriceData = quoteHistory;
            //Analytics.GetAverageBuySell(this);
            Analytics.GetBarsSummary(this);
        }

        internal IReadOnlyList<IBar> HourlyBarData { get; private set; } = new Collection<IBar>();
        internal IReadOnlyList<IQuote> HouerlyPriceData { get; private set; } = new Collection<IQuote>();
        
        internal decimal AgressionSellOffset
        {
            get
            {
                return 0;
                return (((Appsettings.Main.Aggression * 6) - 30)/100) * AverageSell;
            }
        }
        
        internal decimal AgressionBuyOffset
        {
            get
            {
                return 0;
                return (((10 - Appsettings.Main.Aggression * 4) - 40)/100) * AverageSell;
            }
        }

        internal bool LastHourPositiveTrend
        {
            get
            {
                if (HourlyBarData.Count < 1)
                {
                    return false;
                }
                IBar last = HourlyBarData.Last();
                return last.Open < last.Close;
            }
        }

        internal void ClosePosition()
        {
            WorkingData.PurchasedSymbols.Remove(Symbol);
            var closingOrder = ApiRecords.TradingClient.DeletePositionAsync(new DeletePositionRequest(this.Symbol)).Result;
            Console.WriteLine($"{DateTime.Now} - Selling {closingOrder.Quantity} (${LastQuote.BidPrice}) of {Name}!");
            if (closingOrder.AverageFillPrice.HasValue)
            {
                LastSale = closingOrder.AverageFillPrice.Value;
            }
            LastProfit = Position.ChangePrice;
            Log.WasSold = true;
            this.Position = null;
            ApiUtils.RefreshHistory(this);
        }

        internal void BuyStock(decimal quantity)
        {
            LastBuy = DateTime.Now;
            WorkingData.PurchasedSymbols.Add(Symbol);
            var openingOrder = ApiRecords.TradingClient
                .PostOrderAsync(MarketOrder.Buy(this.Symbol,OrderQuantity.Notional(quantity)).WithDuration(TimeInForce.Day)).Result;
            Console.WriteLine($"{DateTime.Now} - Buying {openingOrder.Quantity} (${LastQuote.AskPrice}) of {Name}!");
            this.Position = ApiUtils.GetLatestPosition(this);
            Log.WasBought = true;
            ApiUtils.RefreshHistory(this);
        }

        public StockLogger Log { get; set; }
        public Guid LogId { get; set; }

        internal void ReturnLogEntries()
        {
            Console.WriteLine(Log.ReturnLog());
            Log.ResetLog();
        }

        internal class PositionInformation
        {
            public DateTime BuyDate { get; init; }
            internal decimal QuantityOwned { get; init; } = 0;
            internal decimal BuyPrice { get; init; } = 0;
            internal decimal? ChangePercent { get; init; } = 0;
            internal decimal? CurrentPrice { get; init; } = 0;
            internal decimal ChangePrice
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

            internal bool Profit
            {
                get
                {
                    return ChangePrice > 0;
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
