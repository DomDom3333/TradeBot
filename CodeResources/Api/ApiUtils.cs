using Alpaca.Markets;
using CodeResources;

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
            var clock = Task.Run(() => ApiRecords.AlpacaTradingClient.GetClockAsync()).Result;
            if (clock != null)
            {
                Console.WriteLine(
                    "Timestamp: {0}, NextOpen: {1}, NextClose: {2}",
                    clock.TimestampUtc, clock.NextOpenUtc, clock.NextCloseUtc);
                return false;
            }
            return true;
        }

        private static void LogIn()
        {
            ApiRecords.AlpacaTradingClient = Environments.Paper.GetAlpacaTradingClient(new SecretKey(ApiRecords.Id, ApiRecords.Secret));
        }
    }
}