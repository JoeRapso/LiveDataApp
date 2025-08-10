# LiveDataApp

A minimal real-time price tracker for Binance symbols.

**How it works**
```
Browser  <--WS-->  ASP.NET Core Server  <--WS-->  Binance Trade Stream
```
- Client connects to `ws://<server>/livedata/{symbol}` (e.g., `btcusdt`).
- Server proxies the Binance WebSocket stream `wss://stream.binance.com:9443/ws/{symbol}@trade`.
- Price updates are sent instantly to the browser.

---

## Run

**Server**
```bash
cd LiveDataApp/LiveDataApp
dotnet run
```
Default WS URL: `ws://localhost:5120/livedata/{symbol}`
Default symbol: `btcusdt`
Allowed symbols are in `appsettings.json`.

**Client**  
Open `Client/index.html` in a browser, enter a symbol (e.g., `btcusdt`).

---

## Config
`LiveDataApp/LiveDataApp/appsettings.json`:
```json
{
  "SupportedSymbols": ["btcusdt", "ethusdt", "bnbusdt", "adausdt"]
}
```
