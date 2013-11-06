$(document).ready(function(){

    // TODO: Should these really be hardcoded like this?
    // var defaultServer = "pazzi.parse3.local";
    // var defaultServer = "172.30.3.11";
    var defaultServer = "desktop-win7";
    var defaultPort = "11001";
    
    var blackoutImagePath = "/Assets/images/BlackoutImage.png";
    
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
    
	// Constant values.
    var MESSAGE_INDEXES = {
        MessageSize : 0,
        SocketAction : 4,
        Remainder : 5,
    };
    
	// The current state of the Client instance.
    var ClientState = {
        IsConnecting : false,
        IsConnected : false,
        IsClosed : false,
        IsErrored : false,
        
        NeedsRedraw : false,
        
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
		MapWidth : 0,
		MapHeight : 0,
        Fog : null,
		FogWidth : 0,
		FogHeight : 0,
    };
    
	// A Queue of Messages that have been received which must be processed.
	var messageQueue = [];
	var messageIdCounter = new Number();
    var clientCanvas = $('#clientCanvas')[0];
    var clientContext = clientCanvas.getContext("2d");
	var blackoutImage;
    
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
            
        var connectingMessage = 'Connecting to ' + host + ":" + port + "...";
        document.title = connectingMessage;
        
        $('#connectValues').fadeOut(function() {        
            $('#connectingServerInfo').text(connectingMessage);            
            $('#connectingValues').fadeIn(function() {
                tryConnect(host, port);
            });
        });
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
            
        var connectedMessage = 'Connected to ' + $('#host').val() + ":" + $('#port').val();
        document.title = connectedMessage;
            
        $('#connectingValues').fadeOut(function() {
            $('#connectedServerInfo').text(connectedMessage);
            $('#connectedValues').fadeIn(function() {
                $('#initializingValues').fadeIn(function() {                
                    // Set all the assets we need to load, which should also be checked in the below
                    // connectInitWait interval.
                    blackoutImage = new Image();
                    blackoutImage.src = document.URL.substring(0, document.URL.lastIndexOf("/")) + blackoutImagePath;
                    
                    // Check all the assets being loaded before starting the actual application.
                    var connectInitWait = window.setInterval(function() {
                        if (blackoutImage == null)
                            return;
                                                
                        $('#connectedServerInfo').fadeOut();
                        $('#initializingValues').fadeOut(function() {
                            // Stop the connection initialization interval and instead start the 30FPS Draw Loop.
                            window.clearInterval(connectInitWait);
                            window.setInterval(drawClient, 33);
                            ClientState.NeedsRedraw = true;
                            $('#clientValues').fadeIn("slow");
                        });
                    }, 33);
                });
            });
        });
    }
    
    function onConnectionClosed(e){
        ClientState.IsConnected  = false;
        ClientState.IsClosed = true;
        
        $('#connectedValues').fadeOut();
            
        // If we're already Errored, then the Error message is being shown.
        if (!ClientState.IsErrored)
            $('#disconnectedValues').fadeIn();
    }
    
    function onConnectionError(e){
        console.log(e.data);
        ClientState.IsErrored = true;
        
        if (ClientState.IsConnecting)
        {
            ClientState.IsConnecting = false;
            $('#connectingValues').fadeOut();
            $('#serverNotFoundValues').fadeIn();
        }
        else
        {
            $('#connectedValues').fadeOut();
            $('#errorValues').fadeIn();
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
        
        ClientState.NeedsRedraw = true;
    }
    
    function processMapMessage(messageDataView) {
		mapWidth = messageDataView.getInt32(0, true);
		mapHeight = messageDataView.getInt32(4, true);
		
		var imgSlice = messageDataView.buffer.slice(8);
        var imgByteArray = new Uint8Array(imgSlice);
        
        // This call explodes as a stack overflow, but the For Loop below works instead.
        // var imgBinary = String.fromCharCode.apply(window, imgByteArray);
        
        var imgBinary = '';
        for (var i = 0; i < imgByteArray.length; i++) {
            imgBinary += String.fromCharCode(imgByteArray[i]);
        }        
        var imgBase64 = btoa(imgBinary);
        
		var testImg = new Image();
		testImg.onload = function() {
			ClientState.Map = testImg;
			ClientState.MapWidth = mapWidth;
			ClientState.MapHeight = mapHeight;
			// TODO: Validate width/height in some way?
            
            ClientState.NeedsRedraw = true;
		};
		testImg.src = "data:image/png;base64," + imgBase64;
    }
	
    function processCenterMapMessage(messageDataView)
    {    
    }
    
    function processFogMessage(messageDataView) {
		fogWidth = messageDataView.getInt32(0, true);
		fogHeight = messageDataView.getInt32(4, true);
		
		var imgSlice = messageDataView.buffer.slice(8);
        var imgByteArray = new Uint8Array(imgSlice);
        
        var imgBinary = '';
        for (var i = 0; i < imgByteArray.length; i++) {
            imgBinary += String.fromCharCode(imgByteArray[i]);
        }
        
        // This call explodes as a stack overflow, but the For Loop above works instead.
        // var imgBinary = String.fromCharCode.apply(window, imgByteArray);
        var imgBase64 = btoa(imgBinary);
        
		var testImg = new Image();
		testImg.onload = function() {
			ClientState.Fog = testImg;
			ClientState.FogWidth = fogWidth;
			ClientState.FogHeight = fogHeight;
			// TODO: Validate width/height in some way?
            
            ClientState.NeedsRedraw = true;
		};
		testImg.src = "data:image/png;base64," + imgBase64;
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
    
    // Redraws the client based on the new values.
	function drawClient() {
        if (!ClientState.NeedsRedraw)
            return;
            
        ClientState.NeedsRedraw = false;
        
        clientContext.clearRect(0, 0, clientCanvas.width, clientCanvas.height);
        
        if (ClientState.IsBlackoutOn)
        {
            clientContext.fillStyle = "black";
            clientContext.fillRect(0, 0, clientCanvas.width, clientCanvas.height);
            clientContext.drawImage(blackoutImage, 
                                    clientCanvas.width / 2 - blackoutImage.width / 2, 
                                    clientCanvas.height / 2 - blackoutImage.height / 2);
            return;
        }
        
        if (ClientState.Map != null)
            clientContext.drawImage(ClientState.Map, 0, 0);
            
        if (ClientState.Fog != null)
            clientContext.drawImage(ClientState.Fog, 0, 0);
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
    
    // One-time initialization logic
    $('#host').val(defaultServer);
    $('#port').val(defaultPort);
});