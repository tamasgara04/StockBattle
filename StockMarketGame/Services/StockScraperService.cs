using HtmlAgilityPack;
using StockMarketGame.Classes;
using System.Globalization;

namespace StockMarketGame.Services;

public class StockScraperService
{
    private readonly HttpClient _httpClient;

    public StockScraperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<StockBasicData>> GetHistoricalStockPricesAsync(string url)
    {
        var content = await GetPageContent(url);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(content);

        // Select the table rows that contain the historical data
        var rows = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'freeze-column-w-1')]//tr");

        var historicalPrices = new List<StockBasicData>();

        foreach (var row in rows)
        {
            var cells = row.SelectNodes(".//td");

            // Ensure there are enough cells (skip header or invalid rows)
            if (cells != null && cells.Count >= 4)
            {
                var dateStr = cells[0].InnerText.Trim();
                var priceStr = cells[1].InnerText.Trim();
                string dateFormat = "MMM dd, yyyy"; // Format for "Oct 09, 2024"

                // Parse values (add further parsing and error checking if needed)
                if (DateTime.TryParseExact(dateStr, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
                    double.TryParse(priceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var price))
                {

                    historicalPrices.Add(new StockBasicData()
                    {
                        Date = date,
                        Price = price,
                    });
                }
            }
        }

        return historicalPrices.OrderBy(x=>x.Date).ToList();
    }

    public async Task<List<Equity>> GetEquitiesAsync(string url)
    {
        var equities = new List<Equity>();

        try
        {
            // Send HTTP request
            var content = await GetPageContent(url);

            // Load HTML document
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            // Select equity nodes
            var equityNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'datatable-v2_cell__wrapper__7O0wk')]/a");

            if (equityNodes != null)
            {
                foreach (var node in equityNodes)
                {
                    var displayName = node.GetAttributeValue("title", string.Empty);
                    var href = node.GetAttributeValue("href", string.Empty);
                    var realName = href.Substring(href.LastIndexOf("/") + 1); // Extract the real name from the URL

                    equities.Add(new Equity(displayName, realName));
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (logging, rethrowing, etc.)
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return equities;
    }
    
    public async Task<string> GetPageContent(string url)
    {
        // Create a request message
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        request.Headers.Add("Referer", "https://www.investing.com/");

        // Send the request and get the response
        var response = await _httpClient.SendAsync(request);

        // Ensure we got a successful status code
        response.EnsureSuccessStatusCode();

        // Load the page content into HtmlAgilityPack
        return await response.Content.ReadAsStringAsync();
    }
}

