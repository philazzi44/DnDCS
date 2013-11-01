var ws;

function connect() {
	ws = new WebSocket("ws://desktop-win7:11001/service");
	ws.onopen = function () {
		alert("Connection opened...");
	};

	ws.onmessage = function (evt) {
		var received_msg = evt.data;
		alert("Message Received: '" + received_msg + "'.");
	};

	ws.onclose = function () {
		alert("Connection closed...");
	};

	ws.onerror = function (evt) {
		alert("Connection errored... " + evt.data);
	};
};

function sendInfo() {
	var value = document.getElementById("sendText").value;
	alert("Sending '" + value + "'...");
	ws.send(value);
}