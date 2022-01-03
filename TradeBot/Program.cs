using Alpaca.Markets;
using Microsoft.Extensions.Configuration;
using TradeBot.CodeResources;
using TradeBot.CodeResources.Api;
using TradeBot.Objects;
using TradeBot.Objects.Stocks;
using TradeBot.Strategies;

namespace TradeBot
{
    internal static class Program
    {
        public static Strategies.BaseStrategy CurrentStrategy { get; set; } = new CustomDom();
        public static Timers TimeKeeper { get; set; }
        public static void Main()
        {
            ReadAppsettings();
            TimeKeeper = new Timers();
            WorkingData.Logger = new Logger();
            //CodeResources.HistoricalData.HistoricalData.ReadAllHistoricalData();

            // First, open the API connection
            ApiUtils.InitApi();



            TimeKeeper.AddSub(TimeKeeper.HourlySynced, ApiUtils.RefreshHistory);
            ResubToItems();
            
            TimeKeeper.AddSub(TimeKeeper.SecondlySynced,WorkingData.Logger.UpdateConsole);

            Console.Read();
        }

        private static void ResubToItems()
        {
            GetStocks();
            Task.Run(ApiUtils.RefreshHistory);
            //ApiUtils.RefreshHistory();
            Console.WriteLine("----------------------------------------------------------------------------------------");
            
            foreach (Stock stock in WorkingData.StockList)
            {
                if (CurrentStrategy.HasTradeStrat)
                {
                    if (stock.SType == AssetClass.Crypto)
                    {
                        ApiRecords.CryptoStreamingClient.SubscribeAsync(stock.TradeSub);
                    }
                    else if (stock.SType == AssetClass.UsEquity)
                    {
                        ApiRecords.DataStreamingClinet.SubscribeAsync(stock.TradeSub);
                    }
                    stock.TradeSub.Received += (trade) =>
                    {
                        WorkingData.Logger.UpdateLogData(stock);
                        if (stock.ProcessingLock)
                            return;
                        if (!WorkingData.StockClock.IsOpen && stock.SType == AssetClass.UsEquity)
                            return;
                        stock.ProcessingLock = true;
                        lock (stock)
                        {
                            lock (trade)
                            {
                                CurrentStrategy.RunTradeStrategy(trade, stock);
                            }
                        }
                        stock.ProcessingLock = false;
                    };

                }
                

                if (CurrentStrategy.HasQuoteStrat)
                {
                    if (stock.SType == AssetClass.Crypto)
                    {
                        ApiRecords.CryptoStreamingClient.SubscribeAsync(stock.QuoteSub);
                    }
                    else if (stock.SType == AssetClass.UsEquity)
                    {
                        ApiRecords.DataStreamingClinet.SubscribeAsync(stock.QuoteSub);
                    }
                    stock.QuoteSub.Received += (quote) =>
                    {
                        stock.LastQuote = quote;
                        WorkingData.Logger.UpdateLogData(stock);
                        if (stock.ProcessingLock)
                            return;
                        if (!WorkingData.StockClock.IsOpen && stock.SType == AssetClass.UsEquity)
                            return;
                        stock.ProcessingLock = true;
                        lock (stock)
                        {
                            lock (quote)
                            {
                                CurrentStrategy.RunQuoteStrategy(quote,stock);
                            }
                        }
                        stock.ProcessingLock = false;
                    };
                }
            }
        }

        private static void GetStocks()
        {
            if (Appsettings.Main.MonitoringList == null || Appsettings.Main.MonitoringList.Length < 1)
            {
                LoadAllStocks();
                return;
            }

            foreach (string stock in Appsettings.Main.MonitoringList)
            {
                IAsset asset = null;
                IPosition position = null;
                try
                {
                    asset = ApiRecords.TradingClient.GetAssetAsync(stock).Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not find Symbol {stock}");
                    continue;
                }

                try
                {
                    position = ApiRecords.TradingClient.GetPositionAsync(asset.Symbol).Result;
                    Console.WriteLine($"Found position for Symbol {stock}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"No position found for Symbol {stock}");
                }

                if (asset == null)
                    continue;

                Stock newStock = new Stock(asset.Name, asset.Symbol, asset.AssetId, asset.Class, (int) asset.Exchange, TimeKeeper.MinutelySynced);
                if (position != null)
                {
                    WorkingData.PurchasedSymbols.Add(asset.Symbol);
                    newStock.Position = new Stock.PositionInformation(position.Quantity, position.AverageEntryPrice, position.AssetChangePercent, position.AssetCurrentPrice);
                }
                
                newStock.LogId = WorkingData.Logger.AddLine();
                WorkingData.Logger.AddParams(newStock.LogId, new (string, string)[]
                {
                    ("@Symbol",newStock.Symbol),
                    ("@Target",newStock.AverageSell.ToString()),
                    ("@Current",newStock.LastQuote?.BidPrice.ToString()),
                    ("@Position",newStock.HasPosition ? newStock.Position.Profit.ToString() : "No"),
                });
                Console.WriteLine($"Added Stock {asset.Name}.");
                WorkingData.StockList.Add(newStock);
            }

            Console.WriteLine($"Found {WorkingData.PurchasedSymbols.Count} Positions");
        }

        private static void LoadAllStocks()
        {
            List<IAsset> assetList = new List<IAsset>();
            foreach (string item in ApiRecords.SubbedItems)
            {
                assetList.Add(ApiRecords.TradingClient.GetAssetAsync(item).Result);
            }
            assetList.AddRange(ApiRecords.TradingClient.ListAssetsAsync(new AssetsRequest()).Result.Where(x =>
                x.IsTradable && x.Status == AssetStatus.Active && x.Exchange != Exchange.Unknown && x.Symbol.All(c => char.IsLetter(c)))
                .ToList());

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = 100;

            Parallel.ForEach(assetList, po, asset =>
            {
                if (asset.Class == AssetClass.UsEquity && !Appsettings.Main.TradeStock)
                {
                    Console.WriteLine($"Configured not to trade Stocks in Appsettings. Not adding {asset.Symbol}.");
                    return;
                }
                if (asset.Class == AssetClass.Crypto && !Appsettings.Main.TradeCrypto)
                {
                    Console.WriteLine($"Configured not to trade Crypto in Appsettings. Not adding {asset.Symbol}.");
                    return;
                }
                Stock newStock = new Stock(asset.Name, asset.Symbol, asset.AssetId, asset.Class, (int)asset.Exchange, TimeKeeper.MinutelySynced);
                
                Console.WriteLine($"Added Stock {asset.Name}.");
                WorkingData.StockList.Add(newStock);
            });

            Parallel.ForEachAsync(WorkingData.StockList, po, (stock, token) =>
            {
                stock.Position = ApiUtils.GetLatestPosition(stock);
                return default;
            });

        }

        private static void ReadAppsettings()
        {
            string appsettingsName = "appsettings.json";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appsettingsName, optional: false);
            
            IConfiguration config = builder.Build();

            IConfigurationSection? secMain = config.GetSection("Main");
            Appsettings.Main.IsLive = secMain.GetValue<bool>("IsLive");
            Appsettings.Main.TradeCrypto = secMain.GetValue<bool>("TradeCrypto");
            Appsettings.Main.TradeStock = secMain.GetValue<bool>("TradeStock");
            Appsettings.Main.PaperApiId = secMain.GetValue<string>("PaperApiId");
            Appsettings.Main.PaperApiSecret = secMain.GetValue<string>("PaperApiSecret");            
            Appsettings.Main.LiveApiId = secMain.GetValue<string>("LiveApiId");
            Appsettings.Main.LiveApiSecret = secMain.GetValue<string>("LiveApiSecret");
            Appsettings.Main.Aggression = secMain.GetValue<int>("Aggression");
            Appsettings.Main.MaximumHoldings = secMain.GetValue<int>("MaximumHoldings");
            Appsettings.Main.MonitoringList = secMain.GetValue<string>("MonitoringList").Split(',');
        }
        
    }
}