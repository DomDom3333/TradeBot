using Alpaca.Markets;
using TradeBot.Objects;
using TradeBot.Objects.Stocks;

namespace TradeBot.CodeResources.Api
{
    internal static class ApiUtils
    {

        private static AuthStatus ScStatus { get; set; }
        private static AuthStatus DscStatus { get; set; }
        private static AuthStatus CscStatus { get; set; }
        internal static void InitApi()
        {
            //LoadKeys();
            LogIn();
            if (!TestConnection())
            {
                throw new Exception("Failed connection test!");
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

            if (ScStatus != AuthStatus.Authorized || DscStatus != AuthStatus.Authorized || CscStatus != AuthStatus.Authorized)
            {
                Console.WriteLine("A streaming client failed to Authenticate!");
                return false;
            }

            Console.WriteLine($"${account.BuyingPower} is available as buying power.{Environment.NewLine}");
            return true;
        }

        private static void LogIn()
        {
            if (Appsettings.Main.IsLive)
            {
                SecretKey secretKey = new SecretKey(Appsettings.Main.PaperApiId, Appsettings.Main.PaperApiSecret);
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
                SecretKey secretKey = new SecretKey(Appsettings.Main.LiveApiId, Appsettings.Main.LiveApiSecret);
                Console.WriteLine("Running on PAPER version!");
                ApiRecords.TradingClient = Environments.Paper.GetAlpacaTradingClient(secretKey);
                ApiRecords.CryptoDataClient = Environments.Paper.GetAlpacaCryptoDataClient(secretKey);
                ApiRecords.CryptoStreamingClient = Environments.Paper.GetAlpacaCryptoStreamingClient(secretKey);
                ApiRecords.DataClient = Environments.Paper.GetAlpacaDataClient(secretKey);
                ApiRecords.StreamingClient = Environments.Paper.GetAlpacaStreamingClient(secretKey);
                ApiRecords.DataStreamingClinet = Environments.Paper.GetAlpacaDataStreamingClient(secretKey);

            }

            WorkingData.StockClock = ApiRecords.TradingClient.GetClockAsync().Result;
            Program.TimeKeeper.AddSub(Program.TimeKeeper.MinutelySynced, () =>
            {
                WorkingData.StockClock = ApiRecords.TradingClient.GetClockAsync().Result;
            });
            ScStatus = ApiRecords.StreamingClient.ConnectAndAuthenticateAsync().Result;
            DscStatus = ApiRecords.DataStreamingClinet.ConnectAndAuthenticateAsync().Result;
            CscStatus = ApiRecords.CryptoStreamingClient.ConnectAndAuthenticateAsync().Result;

            WorkingData.AddAccount(ApiRecords.TradingClient.GetAccountAsync().Result);
        }
        
        internal static async void RefreshHistory()
        {
            Console.WriteLine("Refreshing History of all Stocks");

            CancellationTokenSource source = new CancellationTokenSource(60000 * 10);
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = ThreadPool.ThreadCount / 2;
            
            await Parallel.ForEachAsync(WorkingData.StockList, po, (stock, source) =>
            {
                if (stock.ProcessingLock)
                    return default;

                RefreshHistory(stock);
                return default;
            });
            Console.Clear();
            Console.SetCursorPosition(0,0);
        }

        internal static void RefreshHistory(Stock stock)
        {
            IReadOnlyDictionary<string, IReadOnlyList<IBar>> hourBars;
            IReadOnlyDictionary<string, IReadOnlyList<IQuote>> hourPrices;            
            IReadOnlyDictionary<string, IReadOnlyList<IBar>> minuteBars;
            
            switch (stock.SType)
            {
                case AssetClass.UsEquity:
                    hourBars = ApiRecords.DataClient
                        .GetHistoricalBarsAsync(new HistoricalBarsRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now, BarTimeFrame.Hour)).Result.Items;
                    minuteBars = ApiRecords.DataClient
                        .GetHistoricalBarsAsync(new HistoricalBarsRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now, BarTimeFrame.Minute)).Result.Items;
                    hourPrices = ApiRecords.DataClient
                        .GetHistoricalQuotesAsync(new HistoricalQuotesRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now)).Result.Items;
                    break;
                case AssetClass.Crypto:
                    hourBars = ApiRecords.CryptoDataClient
                        .GetHistoricalBarsAsync(new HistoricalCryptoBarsRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now, BarTimeFrame.Hour)).Result.Items;
                    minuteBars = ApiRecords.CryptoDataClient
                        .GetHistoricalBarsAsync(new HistoricalCryptoBarsRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now, BarTimeFrame.Minute)).Result.Items;
                    hourPrices = ApiRecords.CryptoDataClient
                        .GetHistoricalQuotesAsync(new HistoricalCryptoQuotesRequest(stock.Symbol, DateTime.Now.AddYears(-1),
                            DateTime.Now)).Result.Items;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Console.WriteLine($"Got History for {stock.Name}");
            stock.UpdateHostoricalData(hourBars[stock.Symbol], hourPrices[stock.Symbol], minuteBars[stock.Symbol]);
        }

        internal static Stock.PositionInformation? GetLatestPosition(Stock stock)
        {
            IPosition position;
            bool foundPos;
            try
            {
                position = ApiRecords.TradingClient.GetPositionAsync(stock.Symbol).Result;
                foundPos = true;
            }
            catch (Exception)
            {
                return null;
            }

            if (foundPos)
            {
                return new Stock.PositionInformation(position.Quantity, position.AverageEntryPrice,
                    position.AssetChangePercent, position.AssetCurrentPrice);
            }

            return null;
        }
    }
}