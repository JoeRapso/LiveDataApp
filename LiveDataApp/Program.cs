using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Read supported symbols from configuration
var supportedSymbols = builder.Configuration.GetSection("SupportedSymbols").Get<string[]>() ?? Array.Empty<string>();


var app = builder.Build();

app.UseWebSockets();

app.Map("/livedata/{symbol}", async (HttpContext context, ILogger<Program> logger) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var symbol = context.Request.RouteValues["symbol"]?.ToString()?.ToLower();

        // Validation logic for the symbol
        if (!supportedSymbols.Contains(symbol))
        {
            logger.LogWarning("Invalid symbol requested: {Symbol}", symbol);
            var errorWebSocket = await context.WebSockets.AcceptWebSocketAsync();
            var errorMsg = System.Text.Encoding.UTF8.GetBytes($"Error: Invalid symbol. Supported symbols: {string.Join(", ", supportedSymbols)}"); 
            await errorWebSocket.SendAsync(new ArraySegment<byte>(errorMsg), WebSocketMessageType.Text, true, CancellationToken.None);
            await errorWebSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Error occurred", CancellationToken.None);
        }

        WebSocket? webSocket = null;
        try
        {
            // Accept the WebSocket request
            webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var cancellationToken = CancellationToken.None;

            // Connect to Binance WebSocket
            using var binanceSocket = new ClientWebSocket();
            var binanceUri = new Uri($"wss://stream.binance.com:9443/ws/{symbol}@trade");
            await binanceSocket.ConnectAsync(binanceUri, cancellationToken);

            // Buffer for receiving messages
            var buffer = new byte[4096];

            // Forward messages from Binance WebSocket to the frontend client
            while (webSocket.State == WebSocketState.Open && binanceSocket.State == WebSocketState.Open)
            {
                var result = await binanceSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Text, result.EndOfMessage, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions, log exception and send error messages to the client
            logger.LogError(ex, "Exception in /livedata/{Symbol}", symbol);
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                var errorMsg = System.Text.Encoding.UTF8.GetBytes($"Error: {ex.Message}");
                await webSocket.SendAsync(new ArraySegment<byte>(errorMsg), WebSocketMessageType.Text, true, CancellationToken.None);
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error occurred", CancellationToken.None);
            }
            else
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Server error: {ex.Message}");
            }
        }
    }
    else
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid WebSocket request.");
    }
});

app.Run();