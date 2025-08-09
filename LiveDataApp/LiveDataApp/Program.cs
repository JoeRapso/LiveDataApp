using LiveDataApp.Models;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseWebSockets();


app.Map("/livedata", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        using var httpClient = new HttpClient();

        var cancellationToken = CancellationToken.None;

        while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<SessionSummary[]>("http://dev-sample-api.tsl-timing.com/sessions");
                var json = System.Text.Json.JsonSerializer.Serialize(response);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                await webSocket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken);
            }
            catch (Exception ex)
            {
                var errorMsg = System.Text.Encoding.UTF8.GetBytes($"Error: {ex.Message}");
                await webSocket.SendAsync(errorMsg, System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken);
            }

            // Wait for 5 seconds before sending the next update
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();



