namespace TradeBot.CodeResources;

public static class Appsettings
{
    internal static class Main
    {
        internal static bool IsLive { get; set; }
        internal static bool TradeCrypto { get; set; }
        internal static bool TradeStock { get; set; }
        internal static string PaperApiId { get; set; }
        internal static string PaperApiSecret { get; set; }        
        internal static string LiveApiId { get; set; }
        internal static string LiveApiSecret { get; set; }
        internal static int Aggression { get; set; } = 3;
        internal static int MaximumHoldings { get; set; } = 5;
        public static string[] MonitoringList { get; set; }
    }
}