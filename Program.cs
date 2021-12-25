using Alpaca.Markets;
using Microsoft.VisualBasic.FileIO;
using Objects.Stocks;

namespace TradeBot
{

    internal static class Program
    {
        public static void Main(string[] args)
        {
            ReadAllHistoricalData();
        }

        private static void ReadAllHistoricalData()
        {
            CodeResources.CurrentStockData.Cryptos.AddRange(ReadStockFiles(@"/home/dominik/Downloads/Stock History/Crypto", StockType.Crypto));
            CodeResources.CurrentStockData.CompanyStock.AddRange(ReadStockFiles(@"/home/dominik/Downloads/Stock History/Stocks", StockType.Stock));
        }

        private static List<Stock> ReadStockFiles(string stockPath, StockType type)
        {
            DirectoryInfo stockDir = new DirectoryInfo(stockPath);
            if (!stockDir.Exists)
            {
                //if no directory, then no files
                stockDir.Create();
                return new List<Stock>();
            }

            List<Stock> stocks = new List<Stock>();
            FileInfo[] stockFiles = stockDir.GetFiles();
            foreach (FileInfo stockFile in stockFiles)
            {
                stocks.Add(ReadStock(stockFile.FullName, type));
            }

            return stocks;
        }

        private static Stock ReadStock(string path, StockType type)
        {
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new[] {"#"};
                csvParser.SetDelimiters(",");
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                string fn = Path.GetFileNameWithoutExtension(path);

                string stockShort = fn.Substring(0, (fn.Contains('-') ? fn.LastIndexOf('-') : fn.Length));
                
                Stock newStock = new Stock(Path.GetFileNameWithoutExtension(path),stockShort,stockShort, type);
                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    var fields = csvParser.ReadFields();
                    var day = new StockDay();
                    if (fields != null && !fields.Any(s => string.IsNullOrWhiteSpace(s)) && !fields.Any(s => s == "null"))
                    {
                        day.Date = DateTime.Parse(fields[0]);
                        day.Open = decimal.Parse(fields[1]);
                        day.High = decimal.Parse(fields[2]);
                        day.Low = decimal.Parse(fields[3]);
                        day.Close = decimal.Parse(fields[4]);
                        day.CloseAdj = decimal.Parse(fields[5]);
                        day.Volume = decimal.Parse(fields[6]);
                    }

                    newStock.Daily.Add(day);
                }

                return newStock;
            }
        }
    }
    internal static class ProgramV2
    {
        private const String KeyId = "991b05de8e437ef62d3a7a871c4439f6";

        private const String SecretKey = "3d99c4aca4935f083f15cc1d314e3ae280981eda";

        public static async Task MainV2()
        {
            // First, open the API connection
            var client = Environments.Paper
                .GetAlpacaTradingClient(new SecretKey(KeyId, SecretKey));

            var test = client.PostOrderAsync(new NewOrderRequest("BTC", OrderQuantity.Notional(1000),
                OrderSide.Buy, OrderType.Market, TimeInForce.Fok)).Result;
            
            // Get our account information.
            var account = await client.GetAccountAsync();

            // Check if our account is restricted from trading.
            if (account.IsTradingBlocked)
            {
                Console.WriteLine("Account is currently restricted from trading.");
            }

            Console.WriteLine(account.BuyingPower + " is available as buying power.");

            Console.Read();
        }
    }
}