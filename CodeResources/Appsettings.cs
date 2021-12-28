namespace TradeBot.CodeResources;

public static class Appsettings
{
    internal static class Main
    {
        internal static bool IsLive { get; set; }
        internal static bool TradeCrypto { get; set; }
        internal static bool TradeStock { get; set; }
        internal static string HistoricDataPathCrypto { get; set; }
        internal static string HistoricDatapathStocks { get; set; }
        internal static string ApiId { get; set; }
        internal static string ApiSecret { get; set; }
        internal static int Aggression { get; set; } = 3;
        internal static int MaximumHoldings { get; set; } = 5;
        public static string[] MonitoringList { get; set; }
    }
}