namespace Objects.Stocks
{
    class Stock
    {
        public Stock()
        {
            Name = string.Empty;
            Short = string.Empty;
            Code = string.Empty;
        }

        public string Name { get; init; }
        public string Short { get; init; }
        public string Code { get; init; }

        private StockMinute[] Minutely { get; set; }
        private StockHour[] Hourly { get; set; }
        private StockDay[] Daily { get; set; }
        private StockWeek[] Weekly { get; set; }
        private StockYear[] Yearly { get; set; }

        internal StockPeriode[] GroupInto(StockPeriode[] stackToGroup, int groupSize)
        {
            int returnStackLength = (stackToGroup.Length / groupSize) + 1;
            StockPeriode[] returnStack = new StockPeriode[stackToGroup.Length / groupSize +1];

            for (int k = 0; k < returnStackLength; k++)
            {
                decimal open = stackToGroup.First().Open;
                decimal close = stackToGroup.Last().Close;
                decimal high = 0;
                decimal low = 0;
                for (int i = 0; i < stackToGroup.Length; i++)
                {
                    StockPeriode current = stackToGroup[i];

                    if (current.High > high)
                    {
                        high = current.High;
                    }
                    if (current.Low < low)
                    {
                        low = current.Low;
                    }
                }
                
            }


        }
        
    }
}
