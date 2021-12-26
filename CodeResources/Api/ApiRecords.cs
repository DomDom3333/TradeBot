using Alpaca.Markets;

namespace CodeResources.Api
{
    static class ApiRecords
    {
        internal static string Id => "PK36DFD8I4I6Q9HF5I00";
        internal static string Secret => "cC8yhRL0UJaPkFbF6NyLrmbeYOQCpOfFDj7moKoa";
        internal static List<string> SubbedItems = new List<string>() {"BTCUSD", "ETHUSD"};
        internal static List<IAlpacaDataSubscription<ITrade>> Subs = new List<IAlpacaDataSubscription<ITrade>>();
        internal static IAlpacaTradingClient TradingClient { get; set; }
        public static IAlpacaCryptoDataClient CryptoDataClient { get; set; }
        public static IAlpacaCryptoStreamingClient CryptoStreamingClient { get; set; }
    }
}