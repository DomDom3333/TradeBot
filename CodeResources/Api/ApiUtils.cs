using Alpaca.Markets;
using CodeResources;
using Objects.Stocks;
using TradeBot;
using TradeBot.CodeResources;
using TradeBot.Objects;

namespace CodeResources.Api
{
    internal static class ApiUtils
    {
        internal static void InitApi()
        {
            //LoadKeys();
            LogIn();
            if (!TestConnection())
            {
                //Throw error
            }
        }

        private static bool TestConnection()
        {
            // Get our account information.
            var account =  ApiRecords.TradingClient.GetAccountAsync().Result;

            // Check if our account is restricted from trading.
            if (account.IsTradingBlocked)
            {
                Console.WriteLine("Account is currently restricted from trading.");
                return false;
            }

            Console.WriteLine($"${account.BuyingPower} is available as buying power.");
            return true;
        }

        private static void LogIn()
        {
            SecretKey secretKey = new SecretKey(Appsettings.Main.ApiId, Appsettings.Main.ApiSecret);
            if (Appsettings.Main.IsLive)
            {
                Console.WriteLine("Running on LIVE version!");
                ApiRecords.TradingClient = Environments.Live.GetAlpacaTradingClient(secretKey);
                ApiRecords.CryptoDataClient = Environments.Live.GetAlpacaCryptoDataClient(secretKey);
                ApiRecords.CryptoStreamingClient = Environments.Live.GetAlpacaCryptoStreamingClient(secretKey);
                
                ApiRecords.CryptoStreamingClient.ConnectAndAuthenticateAsync();
            }
            else
            {
                Console.WriteLine("Running on PAPER version!");
                ApiRecords.TradingClient = Environments.Paper.GetAlpacaTradingClient(secretKey);
                ApiRecords.CryptoDataClient = Environments.Paper.GetAlpacaCryptoDataClient(secretKey);
                ApiRecords.CryptoStreamingClient = Environments.Paper.GetAlpacaCryptoStreamingClient(secretKey);
                ApiRecords.DataClient = Environments.Paper.GetAlpacaDataClient(secretKey);

                ApiRecords.CryptoStreamingClient.ConnectAndAuthenticateAsync();
            }
            
            WorkingData.AddAccount(ApiRecords.TradingClient.GetAccountAsync().Result);
        }
        
        internal static void RefreshHistory()
        {
            Console.WriteLine("REFRESHING HISTORY!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            
            foreach (Stock stock in WorkingData.StockList)
            {
                IReadOnlyDictionary<string, IReadOnlyList<IBar>> bars = ApiRecords.CryptoDataClient
                    .GetHistoricalBarsAsync(new HistoricalCryptoBarsRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                        DateTime.Now, BarTimeFrame.Hour)).Result.Items;
                IReadOnlyDictionary<string, IReadOnlyList<IQuote>> prices = ApiRecords.CryptoDataClient
                    .GetHistoricalQuotesAsync(new HistoricalCryptoQuotesRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                        DateTime.Now)).Result.Items;
                Console.WriteLine($"Got History for {stock.Name}");
                stock.UpdateHostoricalData(bars[stock.Symbol], prices[stock.Symbol]);
            }
        }

        internal static Stock.PositionInformation GetlatestPosition(Stock stock)
        {
            IPosition position = null;
            try
            {
                position = ApiRecords.TradingClient.GetPositionAsync(stock.Symbol).Result;
            }
            catch (Exception e)
            {
                return null;
            }

            if (position != null)
            {
                return new Stock.PositionInformation(position.Quantity, position.AverageEntryPrice,
                    position.AssetChangePercent, position.AssetCurrentPrice);
            }

            return null;
        }
    }
}