namespace Objects.Stocks
{
    class Stock
    {
        public Stock(string name, string symbolName, string code, StockType type)
        {
            Name = name;
            Symbol = symbolName;
            Code = code;
            SType = type;
        }

        public string Name { get; init; }
        public string Symbol { get; init; }
        public string Code { get; init; }
        public StockType SType { get; init; }

        internal List<StockMinute> Minutely { get; init; } = new List<StockMinute>();
        internal List<StockDay> Daily { get; init; } = new List<StockDay>();
        internal List<StockWeek> Weekly { get; init; } = new List<StockWeek>();
        internal List<StockYear> Yearly { get; init; } = new List<StockYear>();
          
        private StockDay Day {
            get { return GetLastX(60 * 24) as StockDay; }
        }
        private StockWeek Week
        {
            get { return GetLastX(60 * 24 * 7) as StockWeek; }
        }
        private StockYear Year
        {
            get { return GetLastX(60 * 24 * 7 * 52) as StockYear; }
        }
        
        private StockPeriode GetLastX(int minToGroup)
        {
            if (Minutely.Count < 1)
            {
                return null;
            }
            if (Minutely.Count < minToGroup)
            {
                minToGroup = Minutely.Count;
            }
            List<StockMinute> lastMinutes = Minutely.TakeLast(minToGroup).ToList();

            StockPeriode lastPeriode = new StockHour();

            lastPeriode.Open = lastMinutes.First().Open;
            foreach (StockMinute minute in lastMinutes)
            {
                if (lastPeriode.High < minute.High)
                {
                    lastPeriode.High = minute.High;
                }

                if (lastPeriode.Low > minute.Low)
                {
                    lastPeriode.Low = minute.Low;
                }
            }
            lastPeriode.Close = lastMinutes.Last().Close;
            lastPeriode.CloseAdj = lastMinutes.Last().CloseAdj;

            return lastPeriode;
        }
    }
}
