$(document).ready(function(){

    // TODO: Should these really be hardcoded like this?
    var defaultServer = "pazzi.parse3.local";
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
    
    var MESSAGE_INDEXES = {
        MessageSize : 0,
        SocketAction : 4,
        Remainder : 5,
    };
    
    
    $('#host').val(defaultServer);
    $('#port').val(defaultPort);
    
    var ClientState = {
        IsConnecting : false,
        IsConnected : false,
        IsClosed : false,
        IsErrored : false,
        
        AcknowledgedReceived : false,
        IsBlackoutOn : false,
        ShowGrid : false,
        UseFogAlphaEffect : false,
        GridSize : 0,
        GridColor : {
            A : 255,
            R : 0,
            G : 255,
            B : 255
        },
        
        Map : null,
        Fog : null,
    };
    
	// A Queue of Messages that have been received which must be processed.
	var messageQueue = [];
	var messageIdCounter = new Number();
	
    function validateConnectValues(host, port) {
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
            
        $('#connectValues').hide();
        $('#connectingValues').show();
        
        var msg = 'Connecting to ' + $('#host').val() + ":" + $('#port').val() + "...";
        document.title = msg;
        $('#connectingServerInfo').text(msg);
        
        tryConnect(host, port);
    });
    
    function tryConnect(host, port) {    
        ClientState.IsConnecting = true;
        
        var webSocketString = "ws://" + host + ":" + port + "/service";
        var webSocket = new WebSocket(webSocketString);
        webSocket.onopen = onConnectionOpened;
        webSocket.onmessage = onMessageReceived;
        webSocket.onclose = onConnectionClosed;
        webSocket.onerror = onConnectionError;
    }
    
    function onConnectionOpened(e){
        ClientState.IsConnecting = false;
        ClientState.IsConnected = true;
        
        $('#connectingValues').hide();
        $('#connectedValues').show();
        
        var msg = 'Connected to ' + $('#host').val() + ":" + $('#port').val();
        document.title = msg;
        $('#connectedServerInfo').text(msg);
        $('#connectedServerInfo').fadeOut(2000);
    }
    
    function onConnectionClosed(e){
        ClientState.IsConnected  = false;
        ClientState.IsClosed = true;
        
        $('#connectedValues').hide();
            
        // If we're already Errored, then the Error message is being shown.
        if (!ClientState.IsErrored)
            $('#disconnectedValues').show();
    }
    
    function onConnectionError(e){
        ClientState.IsErrored = true;
        
        if (ClientState.IsConnecting)
        {
            ClientState.IsConnecting = false;
            $('#connectingValues').hide();
            $('#serverNotFoundValues').show();
        }
        else
        {
            $('#connectedValues').hide();
            $('#errorValues').show();
        }
    }

    function onMessageReceived(e){
		var messageId = messageIdCounter;
		messageIdCounter++;
		
        log(messageId, "Raw message received (data length = " + e.data.size + ")");
        
		var queueValue = new Object();
		queueValue.messageId = messageId;
		queueValue.dataView = null;
		messageQueue.push(queueValue);
		
        var messageReader = new FileReader();
        messageReader.onload = function(){
			// We now populate the previously enqueued value with the DataView for the read bytes.
			queueValue.dataView = new DataView(messageReader.result);
			processMessageFromQueue();
        };
        messageReader.readAsArrayBuffer(e.data);
    }

	// Called when a FileReader pulling in the bytes from a message are read in and ready.
	// TODO: Note that this pattern only works because onMessageReceived is single-threaded behind the scenes, but
	// we should really come up with some kind of locking mechanism or flagging or timer system instead of this.
	function processMessageFromQueue() {
		// If multiple messages are now available to be handled, we'll keep dealing with them.
		while (messageQueue.length > 0)
		{
			var peekMessage = messageQueue[0];
			
			log(peekMessage.messageId, "Processing...");
			
			// If the DataView value isn't set, then we'll break out entirely because we're waiting on a File
			// Reader to complete, at which point this func will be called again.
			if (peekMessage.dataView == null)
			{
				log(peekMessage.messageId, "Data View not created yet.");
				break;
			}
				
			var queueValue = messageQueue.shift();
			processMessage(queueValue.messageId, queueValue.dataView);
			log(peekMessage.messageId, "Processed.");
		}
	}
	
    function processMessage(messageId, messageDataView) {
        var actualMessageSize = messageDataView.byteLength;
        var specifiedMessageSize = getMessageSize(messageDataView);

        if (actualMessageSize != (specifiedMessageSize + 4))
        {
            log(messageId, "ERROR: " + actualMessageSize + " bytes received on socket, and " + (specifiedMessageSize + 4) + " defined on received message.");
            return;
        }
        else
        {
            log(messageId, "OK: " + actualMessageSize + " bytes received on socket, and " + (specifiedMessageSize + 4) + " defined on received message.");
        }

        var socketAction = getSocketAction(messageDataView);
        log(messageId, "Socket Action Received: " + socketAction.name);
        
		// Either run the necessary func for the action, or set up the Callback to use
		// for the Remainder data slice.
		var onSliceCallback = null;
        switch(socketAction.value)
        {
            case SOCKET_ACTIONS.Unknown.value:
                log(messageId, "ERROR: Unexpected Socket Value 'Unknown'");
                break;
            case SOCKET_ACTIONS.Acknowledge.value:
                if (ClientState.IsAcknowleged)
                    log(messageId, "ERROR: Acknowledge received when already acknowledged.");
                else
                    ClientState.IsAcknowleged = true;
                break;
            case SOCKET_ACTIONS.Ping.value:
                break;
            case SOCKET_ACTIONS.Map.value:
				onSliceCallback = processMapMessage;
                break;
            case SOCKET_ACTIONS.CenterMap.value:
				onSliceCallback = processCenterMapMessage;
                break;
            case SOCKET_ACTIONS.Fog.value:
				onSliceCallback = processFogMessage;
                break;
            case SOCKET_ACTIONS.FogUpdate.value:
				onSliceCallback = processFogUpdateMessage;
                break;
            case SOCKET_ACTIONS.FogOrRevealAll.value:
				onSliceCallback = processFogOrRevealAllMessage;
                break;
            case SOCKET_ACTIONS.UseFogAlphaEffect.value:
				onSliceCallback = processUseFogAlphaEffectMessage;
                break;
            case SOCKET_ACTIONS.GridSize.value:
				onSliceCallback = processGridSizeMessage;
                break;
            case SOCKET_ACTIONS.GridColor.value:
				onSliceCallback = processGridSizeMessage;
                break;
            case SOCKET_ACTIONS.BlackoutOn.value:
                processBlackoutOnMessage();
                break;
            case SOCKET_ACTIONS.BlackoutOff.value:
                processBlackoutOffMessage();
                break;
            case SOCKET_ACTIONS.Exit.value:
                processExitMessage();
                break;
        }
		
		if (onSliceCallback != null)
		    onSliceCallback(new DataView(messageDataView.buffer.slice(MESSAGE_INDEXES.Remainder)));
    }
    
    function processMapMessage(messageDataView) {
		var width = messageDataView.getInt32(0, true);
		var height = messageDataView.getInt32(4, true);
		
		var imgSlice = messageDataView.buffer.slice(8);		
		testImg.src = "data:image/png;base64," + btoa(imgSlice.buffer);
    }
	
    function processCenterMapMessage(messageDataView) {
    }
    
    function processFogMessage(messageDataView) {
    }
    
    function processFogUpdateMessage(messageDataView) {
    }
    
    function processFogOrRevealAllMessage(messageDataView) {
        // Next byte is the flag indicating whether to fog all or reveal all.
        var fogAll = (messageDataView.getInt8(0) == 1);
        if (fogAll)
        {
            // TODO: Push out a Fog Update that fogs everything.
        }
        else
        {
            // TODO: Push out a Fog Update that clears everything.
        }
    }
    
    function processUseFogAlphaEffectMessage(messageDataView) {
        // Next byte is the flag indicating whether to use the effect or not.
        ClientState.UseFogAlphaEffect = (messageDataView.getInt8(0) == 1);
    }
    
    function processGridSizeMessage(messageDataView) {
        // Next byte is the flag indicating whether to use the effect or not.
        var showGrid = (messageDataView.getInt8(0) == 1);
        
        // Next Int32 is the actual grid size to use (only relevant when showing the grid)
        if (showGrid)
        {
            ClientState.ShowGrid = true;
            ClientState.GridSize = messageDataView.getInt32(1, true);
        }
        else
        {
            ClientState.ShowGrid = false;
        }
    }
    
    function processGridColorMessage(messageDataView) {
        // Next 4 bytes are the ARGB values for the color.
        var a = messageDataView.getInt8(0);
        var r = messageDataView.getInt8(1);
        var g = messageDataView.getInt8(2);
        var b = messageDataView.getInt8(3);
        
        ClientState.GridColor = {
            A : a,
            R : r,
            G : g,
            B : b
        };
    }
    
    function processBlackoutOnMessage() {
        ClientState.IsBlackoutOn = true;
    }
    
    function processBlackoutOffMessage() {
        ClientState.IsBlackoutOn = false;
    }
    
    function processExitMessage(messageDataView) {
        // Nothing to do here, as the Socket will raise up a Close
        // event when the server socket has actually closed.
    }
    
    function getMessageSize(messageDataView) {
        return messageDataView.getInt32(MESSAGE_INDEXES.MessageSize, true);
    }
    
    function getSocketAction(messageDataView) {
        var socketActionByte = messageDataView.getInt8(MESSAGE_INDEXES.SocketAction);
        
        for (var socketAction in SOCKET_ACTIONS)
        {
            if (SOCKET_ACTIONS[socketAction].value == socketActionByte)
                return SOCKET_ACTIONS[socketAction];
        }
        
        return SOCKET_ACTIONS.Unknown;
    }
    
    function log(messageId, message) {
		var now = new Date();
        console.log(now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds() + ":" + now.getMilliseconds() + " - " + messageId + " - " + message);
    }    
});