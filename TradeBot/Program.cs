using Alpaca.Markets;
using AlpacaExample.CodeResources;
using CodeResources.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using Objects.Stocks;
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
        public static async Task Main()
        {
            ReadAppsettings();
            //CodeResources.HistoricalData.HistoricalData.ReadAllHistoricalData();

            // First, open the API connection
            ApiUtils.InitApi();

            TimeKeeper = new Timers();

            TimeKeeper.AddSub(TimeKeeper.HourlySynced, ApiUtils.RefreshHistory);
            ResubToItems();

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
                    stock.TradeSub.Received += (trade) =>
                    {
                        if (stock.SType == AssetClass.Crypto)
                        {
                            ApiRecords.CryptoStreamingClient.SubscribeAsync(stock.TradeSub);
                        }
                        else if (stock.SType == AssetClass.UsEquity)
                        {
                            ApiRecords.DataStreamingClinet.SubscribeAsync(stock.TradeSub);
                        }
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
                        if (stock.ProcessingLock)
                            return;
                        if (!WorkingData.StockClock.IsOpen && stock.SType == AssetClass.UsEquity)
                            return;
                        stock.ProcessingLock = true;
                        lock (stock)
                        {
                            lock (quote)
                            {
                                stock.LastQuote = quote;
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
                
                Stock newStock = new Stock(asset.Name, asset.Symbol, asset.AssetId, asset.Class, (int)asset.Exchange, TimeKeeper.MinutelySynced);
                if (position != null)
                {
                    WorkingData.PurchasedSymbols.Add(asset.Symbol);
                    newStock.Position = new Stock.PositionInformation(position.Quantity, position.AverageEntryPrice, position.AssetChangePercent, position.AssetCurrentPrice);
                }
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
                Stock newStock = new Stock(asset.Name, asset.Symbol, asset.AssetId, asset.Class, (int)asset.Exchange, TimeKeeper.MinutelySynced);
                
                Console.WriteLine($"Added Stock {asset.Name}.");
                WorkingData.StockList.Add(newStock);
            });

            var positionSearch = Parallel.ForEachAsync(WorkingData.StockList, po, (stock, token) =>
            {
                stock.Position = ApiUtils.GetLatestPosition(stock);
                return default;
            });

        }

        private static void ReadAppsettings()
        {
            string appsettingsName = "appsettings.";
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                appsettingsName += ("production.json");
            }
            else //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                appsettingsName += ("development.json");
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appsettingsName, optional: false);
            
            IConfiguration config = builder.Build();

            IConfigurationSection? secMain = config.GetSection("Main");
            Appsettings.Main.IsLive = secMain.GetValue<bool>("IsLive");
            Appsettings.Main.TradeCrypto = secMain.GetValue<bool>("TradeCrypto");
            Appsettings.Main.TradeStock = secMain.GetValue<bool>("TradeStock");
            Appsettings.Main.HistoricDataPathCrypto = secMain.GetValue<string>("HistoricDataPathCrypto");
            Appsettings.Main.HistoricDatapathStocks = secMain.GetValue<string>("HistoricDataPathStocks");
            Appsettings.Main.ApiId = secMain.GetValue<string>("ApiId");
            Appsettings.Main.ApiSecret = secMain.GetValue<string>("ApiSecret");
            Appsettings.Main.Aggression = secMain.GetValue<int>("Aggression");
            Appsettings.Main.MaximumHoldings = secMain.GetValue<int>("MaximumHoldings");
            Appsettings.Main.MonitoringList = secMain.GetValue<string>("MonitoringList").Split(',');
        }
        
    }
}