
var noSupportMessage = "Your browser cannot support WebSocket!";
var ws;

function appendMessage(message) {
    $('#response').append(message);
}

function connectSocketServer(collectorUrl) {
    var support = "MozWebSocket" in window ? 'MozWebSocket' : ("WebSocket" in window ? 'WebSocket' : null);

    if (support == null) {
        appendMessage("* " + noSupportMessage + "<br/>");
        return;
    }

    appendMessage("* Connecting to server ..<br/>");
    // create a new websocket and connect
    var value = window[support];
    var serverUrl = 'ws://' + collectorUrl.collectorUrl.value + '/';
    ws = new value(serverUrl);

    // when data is coming from the server, this method is called
    ws.onmessage = function (evt) {
        appendMessage("# " + evt.data + "<br />");
    };

    // when the connection is established, this method is called
    ws.onopen = function () {
        $('#collectorUrl').attr("disabled", "");
        $('#connectButton').attr("disabled", "disabled");
        $('#disconnectButton').attr("disabled", "");
    };

    // when the connection is closed, this method is called
    ws.onclose = function () {
        $('#collectorUrl').attr("disabled", "disabled");
        $('#connectButton').attr("disabled", "");
        $('#disconnectButton').attr("disabled", "disabled");
    }
}

function disconnectWebSocket() {
    if (ws) {
        ws.close();
    }
}

function connectWebSocket() {
    var collectorUrl = document.getElementsByName('collectorUrl').valueOf();
    connectSocketServer(collectorUrl);
}

window.onload = function () {
    $('#collectorUrl').attr("disabled", "disabled");
    $('#sendButton').attr("disabled", "disabled");
    $('#disconnectButton').attr("disabled", "disabled");
}
