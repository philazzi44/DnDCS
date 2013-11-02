$(document).ready(function(){

	// TODO: Should these really be hardcoded like this?
	var defaultServer = "desktop-win7";
	var defaultPort = "11001";
	
	// TODO: Definitely should be defined elsewhere, maybe in a way that we don't have to keep it updated
	// if the C# version updates?
	var SOCKET_ACTIONS = {
		Unknown : {value: 0, name: "Unknown"}, 
		Acknowledge: {value: 1, name: "Acknowledge"}, 
		Ping: {value: 2, name: "Ping"}, 
		Map : {value: 3, name: "Map"},
		CenterMap : {value: 4, name: "CenterMap"},
		Fog : {value: 5, name: "Fog"},
		FogUpdate : {value: 6, name: "FogUpdate"},
		FogOrRevealAll : {value: 7, name: "FogOrRevealAll"},
		UseFogAlphaEffect : {value: 8, name: "UseFogAlphaEffect"},
		GridSize : {value: 9, name: "GridSize"},
		GridColor : {value: 10, name: "GridColor"},
		BlackoutOn : {value: 11, name: "BlackoutOn"},
		BlackoutOff : {value: 12, name: "BlackoutOff"},
		Exit : {value: 13, name: "Exit"}		
	};	
	
	$('#host').val(defaultServer);
	$('#port').val(defaultPort);
	
	function validateConnectValues(host, port)
	{
		// TODO: Add some real validation to the values. Port must
		// be all digits
		if (host == '' || port == '')
		{
			$('#lblBadValues').show();
			return false;
		}
		$('#lblBadValues').hide();
		return true;
	}
	
	$('#btnConnect').click(function() {
		var host = $('#host').val();
		var port = $('#port').val();
		if (!validateConnectValues(host, port))
			return;
			
		tryConnect(host, port);
	});
	
	function tryConnect(host, port) {
		var webSocketString = "ws://" + host + ":" + port + "/service";
		var webSocket = new WebSocket(webSocketString);
		webSocket.onopen = onConnectionOpened;
		webSocket.onmessage = onMessageReceived;
		webSocket.onclose = onConnectionClosed;
		webSocket.onerror = onConnectionError;
	};
	
	function onConnectionOpened(e){
		console.log("Connection opened.");
	}
	
	function onConnectionClosed(e){
		console.log("Connection closed.");
	}
	
	function onConnectionError(e){
		console.log("Connection error.");
	}
	
	
	function onMessageReceived(e){
		console.log("Message received.");
		var bytesBlob = e.data;
		var messageSizeSlice = getMessageSizeSlice(bytesBlob);
		
		var messageSizeReader = new FileReader();
		messageSizeReader.onload = function(){
			processMessage_MessageSize(new DataView(messageSizeReader.result), bytesBlob);
		};
		messageSizeReader.readAsArrayBuffer(messageSizeSlice);			
	}

	function processMessage_MessageSize(messageSizeDataView, bytesBlob)
	{
		var messageSize = messageSizeDataView.getInt32(0, true);
		console.log("MessageSize: " + messageSize);
		
		// TODO: Fault for unexpected message sizes?
		
		var socketActionSlice = getSocketActionSlice(bytesBlob);
		
		var socketActionReader = new FileReader();
		socketActionReader.onload = function() {
			processMessage_SocketAction(new DataView(socketActionReader.result), bytesBlob);
		};
		socketActionReader.readAsArrayBuffer(socketActionSlice);
	}
	
	function processMessage_SocketAction(socketActionDataView, bytesBlob)
	{
		var socketActionByte = socketActionDataView.getInt8(0);
		console.log("SocketActionByte: " + socketActionByte);
		
		var socketAction = getSocketAction(socketActionByte);
		console.log("SocketAction: " + socketAction.name);
		
		// TODO: Ignore Unknowns.
	}
	
	// Gets the first 4 bytes (Int32) of the Bytes Blob, which holds the remaining Message Size integer.
	function getMessageSizeSlice(bytesBlob)
	{
		return bytesBlob.slice(0, 4);
	}
	
	// Gets the 5th byte of the Bytes Blob, which holds the remaining Socket Action byte.
	function getSocketActionSlice(bytesBlob)
	{
		return bytesBlob.slice(4, 5);
	}
	
	function getSocketAction(socketActionByte)
	{
		for (var socketAction in SOCKET_ACTIONS)
		{
			if (SOCKET_ACTIONS[socketAction].value == socketActionByte)
				return SOCKET_ACTIONS[socketAction];
		}
		
		return SOCKET_ACTIONS.Unknown;
	}	
});