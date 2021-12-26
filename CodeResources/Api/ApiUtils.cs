using Alpaca.Markets;
using CodeResources;
using TradeBot.CodeResources;

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
            if (Appsettings.Main.IsLive)
            {
                Console.WriteLine("Running on LIVE version!");
                SecretKey secretKey = new SecretKey(ApiRecords.Id, ApiRecords.Secret);
                ApiRecords.TradingClient = Environments.Live
                    .GetAlpacaTradingClient(secretKey);
            }
            else
            {
                SecretKey secretKey = new SecretKey(ApiRecords.Id, ApiRecords.Secret);
                Console.WriteLine("Running on PAPER version!");
                ApiRecords.TradingClient = Environments.Paper.GetAlpacaTradingClient(new SecretKey(ApiRecords.Id, ApiRecords.Secret));
                ApiRecords.CryptoDataClient = Environments.Paper.GetAlpacaCryptoDataClient(secretKey);
                ApiRecords.CryptoStreamingClient = Environments.Paper.GetAlpacaCryptoStreamingClient(secretKey);


                ApiRecords.CryptoStreamingClient.ConnectAndAuthenticateAsync();
            }
        }
    }
}