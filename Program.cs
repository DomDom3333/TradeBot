using Alpaca.Markets;
using AlpacaExample.CodeResources;
using CodeResources.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using Objects.Stocks;
using TradeBot.CodeResources;
using TradeBot.Objects;
using TradeBot.Strategies;

namespace TradeBot
{
    internal static class Program
    {
        public static Strategies.MainStrategy Strats { get; set; } = new MainStrategy();
        public static Timers TimeKeeper { get; set; }
        public static async Task Main()
        {
            ReadAppsettings();
            //CodeResources.HistoricalData.HistoricalData.ReadAllHistoricalData();

            // First, open the API connection
            ApiUtils.InitApi();

            TimeKeeper = new Timers();
            TimeKeeper.AddSub(TimeKeeper.MinutelySynced, ApiUtils.RefreshHistory);
            ResubToItems();

            Console.Read();
        }

        private static void ResubToItems()
        {
            GetStocks();
            ApiUtils.RefreshHistory();
            foreach (Stock stock in WorkingData.StockList)
            {
                ApiRecords.CryptoStreamingClient.SubscribeAsync(stock.TradeSub);
                stock.TradeSub.Received += (trade) =>
                {
                    //Strats.RunStrategy(trade);
                };
                ApiRecords.CryptoStreamingClient.SubscribeAsync(stock.QuoteSub);
                stock.QuoteSub.Received += (quote) =>
                {
                    if (stock.ProcessingLock)
                        return;
                    stock.ProcessingLock = true;
                    Strats.RunStrategy(quote, stock);
                    stock.ProcessingLock = false;
                };
            }
        }

        private static void GetStocks()
        {
            //IReadOnlyList<IAsset> test = ApiRecords.TradingClient.ListAssetsAsync(new AssetsRequest()).Result;

            foreach (string stock in ApiRecords.SubbedItems)
            {
                IAsset asset = null;
                IPosition position = null;
                try
                {
                    asset = ApiRecords.TradingClient.GetAssetAsync(stock).Result;
                    position = ApiRecords.TradingClient.GetPositionAsync(asset.Symbol).Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not find Symbol {stock}");
                    continue;
                }

                if (asset == null)
                    continue;
                
                Stock newStock = new Stock(asset.Name, asset.Symbol, asset.AssetId, asset.Class);
                if (position != null)
                {
                    newStock.Position = new Stock.PositionInformation(position.Quantity, position.AverageEntryPrice, position.AssetChangePercent, position.AssetCurrentPrice);
                }
                Console.WriteLine($"Added Stock {asset.Name}.");
                WorkingData.StockList.Add(newStock);
            }
        }

        private static void ReadAppsettings()
        {
            string appsettingsName = "appsettings.";
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                appsettingsName += ("production.json");
            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
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
        }
        
    }
}