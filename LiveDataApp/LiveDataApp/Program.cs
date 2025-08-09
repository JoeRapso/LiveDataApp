using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseWebSockets();


app.Map("/livedata/{symbol}", async context =>
{
    // Check if the request is a WebSocket request
    if (context.WebSockets.IsWebSocketRequest)
    {
        var symbol = context.Request.RouteValues["symbol"]?.ToString()?.ToLower() ?? "btcusdt";

        // Validation logic for the symbol

        if (!(symbol == "btcusdt" || symbol == "ethusdt"))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid symbol. Only 'btcusdt or ethusdt' is supported.");
            return;
        }

        // Accept the WebSocket request
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var cancellationToken = CancellationToken.None;

        // Connect to Binance WebSocket
        using var binanceSocket = new ClientWebSocket();
        var binanceUri = new Uri($"wss://stream.binance.com:9443/ws/{symbol}@trade");
        await binanceSocket.ConnectAsync(binanceUri, cancellationToken);

        var buffer = new byte[4096];

        while (webSocket.State == WebSocketState.Open && binanceSocket.State == WebSocketState.Open)
        {
            // Receive message from Binance WebSocket
            var result = await binanceSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            // Forward Binance message to frontend client
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Text, result.EndOfMessage, cancellationToken);
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();