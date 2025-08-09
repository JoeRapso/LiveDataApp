const statusEl = document.getElementById('status');
const outputEl = document.getElementById('output');
const inputEl = document.getElementById('input');

let socket = null;

function renderTable(payload, symbol) {
    outputEl.innerHTML = `
        <table>
            <thead>
                <tr>
                    <th>Price (${symbol.toUpperCase()})</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>${Number(payload.p).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                </tr>
            </tbody>
        </table>
    `;
}

function connect(symbol) {
    // Close previous socket if open
    if (socket && socket.readyState === WebSocket.OPEN) {
        socket.close();
    }

    statusEl.textContent = 'Connecting…';
    outputEl.innerHTML = '';

    // Default to btcusdt if no symbol provided
    symbol = symbol ? symbol.trim().toLowerCase() : 'btcusdt';

    socket = new WebSocket(`ws://localhost:5120/livedata/${symbol}`);

    socket.onopen = function () {
        statusEl.textContent = 'Connected';
    };

    socket.onmessage = function (event) {
        const payload = JSON.parse(event.data);
        renderTable(payload, symbol);
        console.log(payload);
    };

    socket.onclose = function () {
        statusEl.textContent = 'Disconnected';
    };

    socket.onerror = function (error) {
        statusEl.textContent = 'Error: ' + error.message;
    };
}

// Listen for user input and connect on Enter key
inputEl.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') {
        connect(inputEl.value);
    }
});

// Initial connection with default symbol
connect('btcusdt');