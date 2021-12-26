using Alpaca.Markets;
using CodeResources.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using Objects.Stocks;
using TradeBot.CodeResources;

namespace TradeBot
{
    internal static class Program
    {
        public static async Task Main()
        {
            ReadAppsettings();
            CodeResources.HistoricalData.HistoricalData.ReadAllHistoricalData();

            // First, open the API connection
            ApiUtils.InitApi();

            var clock = await ApiRecords.AlpacaTradingClient.GetClockAsync();
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

            var watchlist = ApiRecords.AlpacaTradingClient.CreateWatchListAsync(new NewWatchListRequest("My Watchlist"))
                .Result;
            var asset = ApiRecords.AlpacaTradingClient.GetAssetAsync("AMD").Result;
            var history = ApiRecords.AlpacaTradingClient.AddAssetIntoWatchListByNameAsync(new ChangeWatchListRequest<string>(Guid.NewGuid().ToString(), asset.Symbol)).Result;
            
            Console.Read();
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