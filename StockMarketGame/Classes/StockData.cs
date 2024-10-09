namespace StockMarketGame.Classes;

public class StockData
{
    public string Pid { get; set; }
    public string LastDir { get; set; }
    public decimal LastNumeric { get; set; }
    public string Last { get; set; }
    public string Bid { get; set; }
    public string Ask { get; set; }
    public string High { get; set; }
    public string Low { get; set; }
    public string Pc { get; set; }
    public string Pcp { get; set; }
    public string PcCol { get; set; }
    public string Time { get; set; }
    public long Timestamp { get; set; }
}