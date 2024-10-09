namespace StockMarketGame.Classes;

public class StockData
{
    public string pid { get; set; }
    public string lastDir { get; set; }
    public decimal LastNumeric { get; set; }
    public string last { get; set; }
    public string bid { get; set; }
    public string ask { get; set; }
    public string high { get; set; }
    public string low { get; set; }
    public string pc { get; set; }
    public string pcp { get; set; }
    public string pcCol { get; set; }
    public string time { get; set; }
    public long timestamp { get; set; }
}