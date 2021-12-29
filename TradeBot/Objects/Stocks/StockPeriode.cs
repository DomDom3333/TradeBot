namespace Objects.Stocks;

public class StockPeriode
{
    public DateTime Date { get; set; }
    public decimal Open { get; set; } = 0;
    public decimal Close { get; set; } = 0;
    public decimal High { get; set; } = 0;
    public decimal Low { get; set; } = 0;
    public decimal CloseAdj { get; set; } = 0;
    public decimal Volume { get; set; } = 0;

}