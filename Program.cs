using System;
using Alpaca.Markets;
using System.Threading.Tasks;

namespace AlpacaExample
{
    internal static class Program
    {
        private const String KEY_ID = "dbf226352186c20b6c8bc2614a38c149";

        private const String SECRET_KEY = "e218cf9ce029371c5e27a7ace5612675b32da8fd";

        public static async Task Main()
        {
            var key = new SecretKey(KEY_ID, SECRET_KEY);
            
            IAlpacaTradingClient client = Environments.Paper
                .GetAlpacaTradingClient(key);

            IClock? clock = await client.GetClockAsync();

            if (clock != null)
            {
                Console.WriteLine(
                    "Timestamp: {0}, NextOpen: {1}, NextClose: {2}",
                    clock.TimestampUtc, clock.NextOpenUtc, clock.NextCloseUtc);
            }
        }
    }
}