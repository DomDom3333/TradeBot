using Alpaca.Markets;

namespace CodeResources.Api
{
    static class ApiRecords
    {
        internal static List<string> SubbedItems = new List<string>() {"BTCUSD", "BCHUSD", "ETHUSD", "LTCUSD"};
        internal static List<IAlpacaDataSubscription<ITrade>> Subs = new List<IAlpacaDataSubscription<ITrade>>();
        internal static IAlpacaTradingClient TradingClient { get; set; }
        public static IAlpacaCryptoDataClient CryptoDataClient { get; set; }
        public static IAlpacaCryptoStreamingClient CryptoStreamingClient { get; set; }
        public static IAlpacaDataClient DataClient { get; set; }
    }
}