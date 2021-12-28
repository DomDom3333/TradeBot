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

        private static AuthStatus scStatus { get; set; }
        private static AuthStatus dscStatus { get; set; }
        private static AuthStatus cscStatus { get; set; }
        internal static void InitApi()
        {
            //LoadKeys();
            LogIn();
            if (!TestConnection())
            {
                throw new Exception("Faled connection test!");
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

            if (scStatus != AuthStatus.Authorized || dscStatus != AuthStatus.Authorized || cscStatus != AuthStatus.Authorized)
            {
                Console.WriteLine("A streaming client failed to Authenticate!");
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
                ApiRecords.DataClient = Environments.Paper.GetAlpacaDataClient(secretKey);
                ApiRecords.StreamingClient = Environments.Paper.GetAlpacaStreamingClient(secretKey);
                ApiRecords.DataStreamingClinet = Environments.Paper.GetAlpacaDataStreamingClient(secretKey);
            }
            else
            {
                Console.WriteLine("Running on PAPER version!");
                ApiRecords.TradingClient = Environments.Paper.GetAlpacaTradingClient(secretKey);
                ApiRecords.CryptoDataClient = Environments.Paper.GetAlpacaCryptoDataClient(secretKey);
                ApiRecords.CryptoStreamingClient = Environments.Paper.GetAlpacaCryptoStreamingClient(secretKey);
                ApiRecords.DataClient = Environments.Paper.GetAlpacaDataClient(secretKey);
                ApiRecords.StreamingClient = Environments.Paper.GetAlpacaStreamingClient(secretKey);
                ApiRecords.DataStreamingClinet = Environments.Paper.GetAlpacaDataStreamingClient(secretKey);

            }

            scStatus = ApiRecords.StreamingClient.ConnectAndAuthenticateAsync().Result;
            dscStatus = ApiRecords.DataStreamingClinet.ConnectAndAuthenticateAsync().Result;
            cscStatus = ApiRecords.CryptoStreamingClient.ConnectAndAuthenticateAsync().Result;

            WorkingData.AddAccount(ApiRecords.TradingClient.GetAccountAsync().Result);
        }
        
        internal static void RefreshHistory()
        {
            Console.WriteLine("Refreshing History of all Stocks");
            
            foreach (Stock stock in WorkingData.StockList)
            {
                RefreshHistory(stock);
            }
        }

        internal static void RefreshHistory(Stock stock)
        {
            Console.WriteLine($"Refreshing {stock.Name} History.");
            IReadOnlyDictionary<string, IReadOnlyList<IBar>> bars = new Dictionary<string, IReadOnlyList<IBar>>();
            IReadOnlyDictionary<string, IReadOnlyList<IQuote>> prices = new Dictionary<string, IReadOnlyList<IQuote>>();
            
            switch (stock.SType)
            {
                case AssetClass.UsEquity:
                    bars = ApiRecords.DataClient
                        .GetHistoricalBarsAsync(new HistoricalBarsRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now, BarTimeFrame.Hour)).Result.Items;
                    prices = ApiRecords.DataClient
                        .GetHistoricalQuotesAsync(new HistoricalQuotesRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now)).Result.Items;
                    break;
                case AssetClass.Crypto:
                    bars = ApiRecords.CryptoDataClient
                        .GetHistoricalBarsAsync(new HistoricalCryptoBarsRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now, BarTimeFrame.Hour)).Result.Items;
                    prices = ApiRecords.CryptoDataClient
                        .GetHistoricalQuotesAsync(new HistoricalCryptoQuotesRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now)).Result.Items;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Console.WriteLine($"Got History for {stock.Name}");
            stock.UpdateHostoricalData(bars[stock.Symbol], prices[stock.Symbol]);
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