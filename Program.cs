using Alpaca.Markets;
using CodeResources.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using Objects.Stocks;
using TradeBot.CodeResources;
using TradeBot.Strategies;

namespace TradeBot
{
    internal static class Program
    {
        public static Strategies.MainStrategy Strats { get; set; } = new MainStrategy();
        public static async Task Main()
        {
            ReadAppsettings();
            //CodeResources.HistoricalData.HistoricalData.ReadAllHistoricalData();

            // First, open the API connection
            ApiUtils.InitApi();

            var clock = await ApiRecords.TradingClient.GetClockAsync();
            if (clock != null)
            {
                Console.WriteLine(
                    "Timestamp: {0}, NextOpen: {1}, NextClose: {2}",
                    clock.TimestampUtc, clock.NextOpenUtc, clock.NextCloseUtc);
                if (!clock.IsOpen)
                {
                    Console.WriteLine("Market is currently CLOSED");
                }
            }

            ResubToItems();

            Console.Read();
        }

        private static void ResubToItems()
        {
            DownloadHistories();

            foreach (IAlpacaDataSubscription<ITrade> sub in ApiRecords.Subs)
            {
                ApiRecords.CryptoStreamingClient.SubscribeAsync(sub);
                sub.Received += (trade) =>
                {
                    Strats.RunStrategy(trade);
                };
            }
        }

        private static void DownloadHistories()
        {
            foreach (string subbedItem in ApiRecords.SubbedItems)
            {
                IAlpacaDataSubscription<ITrade> newSub = ApiRecords.CryptoStreamingClient.GetTradeSubscription(subbedItem);
                if (newSub == null)
                {
                    Console.WriteLine($"The Asset with Symbol {subbedItem} cannot be found");
                    continue;
                    
                }
                ApiRecords.Subs.Add(newSub);
                
                //Gets History
                Objects.WorkingData.History.Add(subbedItem, ApiRecords.CryptoDataClient.GetHistoricalBarsAsync(
                    new HistoricalCryptoBarsRequest(subbedItem,
                        DateTime.Today.AddYears(-1), DateTime.Now, BarTimeFrame.Minute)).Result);
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
            Appsettings.Main.IsLive = secMain.GetValue<bool>("isLive");
            Appsettings.Main.TradeCrypto = secMain.GetValue<bool>("TradeCrypto");
            Appsettings.Main.TradeStock = secMain.GetValue<bool>("TradeStock");
            Appsettings.Main.HistoricDataPathCrypto = secMain.GetValue<string>("HistoricDataPathCrypto");
            Appsettings.Main.HistoricDatapathStocks = secMain.GetValue<string>("HistoricDataPathStocks");
        }
        
    }
}