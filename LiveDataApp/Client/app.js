
const statusEl = document.getElementById('status');
const outputEl = document.getElementById('output');

function renderTable(payload) {

}

function connect() {

    const socket = new WebSocket('ws://localhost:5120/livedata');

    socket.onopen = function () {
        statusEl.textContent = 'Connected';

    };

    socket.onmessage = function (event) {
        const payload = JSON.parse(event.data);
        renderTable(payload);
        console.log(payload);
    };

    socket.onclose = function () {
        statusEl.textContent = 'Disconnected';
    };

    socket.onerror = function (error) {
        statusEl.textContent = 'Error: ' + error.message;
    };

}

connect();