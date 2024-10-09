using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using StockMarketGame.Classes;

namespace StockMarketGame.Services;

public class WebSocketService : IDisposable
{
    private readonly ClientWebSocket _clientWebSocket;
    private CancellationTokenSource _cts;
    private Uri _serverUri;
    public event Action<string> MessageReceived;
    public WebSocketService()
    {
        _clientWebSocket = new ClientWebSocket();
        _serverUri = new Uri("wss://streaming.forexpros.com/echo/109/cfhdfe5h/websocket");
        _cts = new CancellationTokenSource();
    }

    public async Task ConnectAsync()
    {
        try
        {
            await _clientWebSocket.ConnectAsync(_serverUri, _cts.Token);
            Console.WriteLine("WebSocket connection established");

            // Subscribe to stock data
            await SubscribeToStockDataAsync();

            // Start receiving messages and heartbeat
            await Task.WhenAll(ReceiveMessagesAsync(), SendHeartbeatAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket connection error: {ex.Message}");
        }
    }

    private async Task SubscribeToStockDataAsync()
    {
        var subscribeMessage = "[\"{\\\"_event\\\":\\\"bulk-subscribe\\\",\\\"tzID\\\":8,\\\"message\\\":\\\"isOpenExch-2:%%isOpenExch-1:%%pid-8849:%%isOpenExch-1004:%%pid-8833:%%pid-8862:%%pid-8830:%%pid-8836:%%pid-8831:%%pid-8916:%%pid-6408:%%pid-6369:%%pid-13994:%%pid-6435:%%pid-13063:%%pid-26490:%%pid-243:%%pid-1175152:%%isOpenExch-152:%%pid-1175153:%%pid-169:%%pid-166:%%pid-14958:%%pid-44336:%%isOpenExch-97:%%pid-8827:%%pid-6497:%%pid-941155:%%pid-23705:%%pid-23706:%%pid-23703:%%pid-23698:%%pid-8880:%%isOpenExch-118:%%pid-8895:%%pid-1141794:%%pid-20:%%pid-172:%%isOpenExch-4:%%pid-27:%%isOpenExch-3:%%pid-167:%%isOpenExch-9:%%pid-178:%%isOpenExch-20:%%pid-8832:%%pid-1:%%isOpenExch-1002:%%pid-2:%%pid-3:%%pid-5:%%pid-7:%%pid-9:%%pid-10:%%isOpenExch-NaN:%%pid-17195:%%pid-1166239:%%pid-1177781:%%pid-252:%%pid-16678:%%pid-8274:%%pid-1096032:%%pid-251:%%pidExt-6408:%%cmt-1-5-6408:%%pid-8318:%%pid-271:\\\"}\"]";
        // Send subscription message to receive stock data
        var messageBytes = Encoding.UTF8.GetBytes(subscribeMessage);
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cts.Token);

        Console.WriteLine("Subscription message sent");
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];

        while (_clientWebSocket.State == WebSocketState.Open)
        {
            var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            Console.WriteLine($"Message received: {message}");

            // Check if it's not a heartbeat message
            if (!message.Contains("heartbeat"))
            {
                Console.WriteLine($"Non-heartbeat message: {message}");
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cts.Token);
                Console.WriteLine("WebSocket connection closed");
            }

            // Further parse messages if they contain stock data
            if (message.Contains("pid"))
            {
                Console.WriteLine($"Stock Data Message: {message}");

            }
        }
    }

    private async Task SendHeartbeatAsync()
    {
        while (_clientWebSocket.State == WebSocketState.Open)
        {
            var heartbeatMessage = "{\"_event\":\"heartbeat\",\"message\":\"keep-alive\"}";
            var messageBytes = Encoding.UTF8.GetBytes(heartbeatMessage);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cts.Token);

            await Task.Delay(10000);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _clientWebSocket?.Dispose();
    }
}
