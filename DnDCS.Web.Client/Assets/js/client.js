$(document).ready(function(){

    // TODO: Should these really be hardcoded like this?
    var defaultServer = "pazzi.parse3.local";
    // var defaultServer = "172.30.3.11";
    // var defaultServer = "desktop-win7";
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
        
        IsDragScrolling : false,
        LastMouseLocationX : 0,
        LastMouseLocationY : 0,
        ScrollPositionX : 0,
        ScrollPositionY : 0,
    };
    
	// A Queue of Messages that have been received which must be processed.
	var messageQueue = [];
	var messageIdCounter = new Number();
    
    // The client's visible canvas. Values are set after a connection is established.
    var clientCanvas = $('#clientCanvas')[0];
    var clientCanvasX;
    var clientCanvasY;
    var clientCanvasWidth;
    var clientCanvasHeight;
    var clientContext;
    
    // A backing canvas used when Fog-related messages are processed.
    var newFogCanvas = document.createElement('canvas');
    var newFogContext = newFogCanvas.getContext("2d");
    
    // Static assets loaded after a connection is established.
    var StaticAssets = {
        BlackoutImagePath : "/Assets/images/BlackoutImage.png",
        BlackoutImage : null,
        
        NewFogColor : "black",
        // While it would seem logical to assign this as being 100% Alpha, we're using Compositing
        // to properly "overwrite" the destination image, so any color is fine.
        NewFogClearColor : "white",
    };
    
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
                    StaticAssets.BlackoutImage = new Image();
                    StaticAssets.BlackoutImage.src = document.URL.substring(0, document.URL.lastIndexOf("/")) + StaticAssets.BlackoutImagePath;
                    
                    // Check all the assets being loaded before starting the actual application.
                    var connectInitWait = window.setInterval(function() {
                        if (StaticAssets.BlackoutImage == null)
                            return;
                                                
                        $('#connectedServerInfo').fadeOut();
                        $('#initializingValues').fadeOut(function() {
                            // Stop the connection initialization interval and instead start the 30FPS Draw Loop after init.
                            window.clearInterval(connectInitWait);
                                                        
                            clientCanvasX = clientCanvas.getBoundingClientRect().left;
                            clientCanvasY = clientCanvas.getBoundingClientRect().top;
                            clientCanvasWidth = clientCanvas.getBoundingClientRect().right - clientCanvas.getBoundingClientRect().left;
                            clientCanvasHeight = clientCanvas.getBoundingClientRect().bottom - clientCanvas.getBoundingClientRect().top;
                            clientContext = clientCanvas.getContext("2d");
    
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
        $('#clientValues').fadeOut();
            
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
            // We draw the newly obtained fog into the NewFog Context so we can
            // properly add/remove areas to it as new fog is received.
			// TODO: Validate width/height in some way?
            newFogCanvas.width = fogWidth;
            newFogCanvas.height = fogHeight;
            newFogContext.drawImage(testImg, 0, 0);
            var newFogImageData = newFogContext.getImageData(0, 0, fogWidth, fogHeight);
            for (var i = 0; i < fogHeight; i++)
            {
                for (var j = 0; j < fogWidth; j++)
                {
                    // Make anything that's non-black fully transparent white. The Server needs the data to be 'white' because it uses it as a Mask. The Win Forms Client uses the same
                    // logic as the server. Therefore, unfortunatel,y we're forced to do a crazy loop to manually apply the mask.
                    var r = newFogImageData.data[((i*(fogWidth*4)) + (j*4))];
                    var g = newFogImageData.data[((i*(fogWidth*4)) + (j*4)) + 1];
                    var b = newFogImageData.data[((i*(fogWidth*4)) + (j*4)) + 2];
                    if (r != 0 || g != 0 || b != 0)
                    {
                        newFogImageData.data[((i*(fogWidth*4)) + (j*4))] = 255;
                        newFogImageData.data[((i*(fogWidth*4)) + (j*4) + 1)] = 255;
                        newFogImageData.data[((i*(fogWidth*4)) + (j*4) + 2)] = 255;
                        newFogImageData.data[((i*(fogWidth*4)) + (j*4) + 3)] = 0;
                    }
                }
            }
            newFogContext.putImageData(newFogImageData, 0, 0);
                        
            var testImg2 = new Image();
            testImg2.onload = function() {
                ClientState.Fog = testImg2;
                ClientState.FogWidth = fogWidth;
                ClientState.FogHeight = fogHeight;
            };
            testImg2.src = newFogCanvas.toDataURL();
                        
            ClientState.NeedsRedraw = true;
		};
		testImg.src = "data:image/png;base64," + imgBase64;
    }
    
    function processFogUpdateMessage(messageDataView) {
        // Next byte is the flag indicating whether to fog or clear.
        var isClearing = (messageDataView.getInt8(0) == 1);
        
        var newFogPoints = [];
        // Subsequent bytes are a series of X (Int32) and Y (Int32) value pairs
        for (var i = 1; i < messageDataView.buffer.byteLength; i += 8)
        {
            var newFogPointX = messageDataView.getInt32(i, true);
            var newFogPointY = messageDataView.getInt32(i + 4, true);
            
            newFogPoints.push({x : newFogPointX, y : newFogPointY});
        }
                
        // For clearing, we'll use a composite operation to clear out fog. Otherwise, we'll
        // be drawing the fog on top of the image.
        var fillStyle;
        if (isClearing)
        {
            fillStyle = StaticAssets.NewFogClearColor;
            newFogContext.globalCompositeOperation = 'destination-out';
        }
        else
        {
            fillStyle = StaticAssets.NewFogColor;
        }
        
        newFogContext.beginPath();
        newFogContext.moveTo(newFogPoints[0].x, newFogPoints[0].y);
        for (var i = 1; i < newFogPoints.length; i++) {
            newFogContext.lineTo(newFogPoints[i].x, newFogPoints[i].y);
        }
        newFogContext.closePath();
        newFogContext.fillStyle = fillStyle;
        newFogContext.fill();
        newFogContext.globalCompositeOperation = 'source-over';
        
        setNewFogImage();
    }
    
    function processFogOrRevealAllMessage(messageDataView) {
        // Next byte is the flag indicating whether to fog all or reveal all.
        var fogAll = (messageDataView.getInt8(0) == 1);
                
        if (fogAll)
        {
            newFogContext.fillStyle = StaticAssets.NewFogColor;
            newFogContext.fillRect(0, 0, ClientState.FogWidth, ClientState.FogHeight);
        }
        else
        {
            newFogContext.clearRect(0, 0, ClientState.FogWidth, ClientState.FogHeight);
        }
        
        setNewFogImage();
    }
    
    function setNewFogImage()
    {
        var newFogData = newFogCanvas.toDataURL();
        var newFogImage = new Image();
        newFogImage.onload = function(){
            ClientState.Fog = newFogImage;
            ClientState.NeedsRedraw = true;
        };
        newFogImage.src = newFogData;
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
    
    function onMouseDown(e) {
        if (e.button != 0)
            return;
        enableDragScroll(e);
    }
    
    function onMouseMove(e) {
        if (!ClientState.IsDragScrolling)
            return;
        if (e.button != 1 && e.which != 1)
        {
            // If we had the Scroll logic set set but our button is no longer pressed, then we'll stop the drag altogether.
            disableDragScroll();
            return;
        }
            
        var newMouseLocationX = e.clientX - clientCanvasX;
        var newMouseLocationY = e.clientY - clientCanvasY;
        
        var deltaX = ClientState.LastMouseLocationX - newMouseLocationX;
        var deltaY = ClientState.LastMouseLocationY - newMouseLocationY;
        
        ClientState.ScrollPositionX = Math.min(ClientState.MapWidth - clientCanvasWidth, Math.max(0, ClientState.ScrollPositionX + deltaX));
        ClientState.ScrollPositionY = Math.min(ClientState.MapHeight - clientCanvasHeight, Math.max(0, ClientState.ScrollPositionY + deltaY));
              
        ClientState.LastMouseLocationX = newMouseLocationX;
        ClientState.LastMouseLocationY = newMouseLocationY;
        
        ClientState.NeedsRedraw = true;
    }
    
    function onMouseUp(e) {
        if (e.button != 0)
            return;
        disableDragScroll();
    }
    
    function enableDragScroll(e)
    {
        ClientState.IsDragScrolling = true;
        ClientState.LastMouseLocationX = e.clientX - clientCanvasX;
        ClientState.LastMouseLocationY = e.clientY - clientCanvasY;
        ClientState.NeedsRedraw = true;
    }
    
    function disableDragScroll()
    {
        ClientState.IsDragScrolling = false;
        ClientState.NeedsRedraw = true;
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
            clientContext.drawImage(StaticAssets.BlackoutImage, 
                                    clientCanvas.width / 2 - StaticAssets.BlackoutImage.width / 2, 
                                    clientCanvas.height / 2 - StaticAssets.BlackoutImage.height / 2);
            return;
        }
        
        clientContext.save();
        {
            clientContext.translate(-ClientState.ScrollPositionX, -ClientState.ScrollPositionY);
            
            if (ClientState.Map != null)
            {
                // Because our Context instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
                // take from the Map Image starting at the Translated Location and go the full width of our client view only. Note that we
                // explicitly Min/Max the values to prevent trying to source from off the image.
                clientContext.drawImage(ClientState.Map, 0, 0);
            }
                
            if (ClientState.Fog != null)
            {
                // Because our Context instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
                // take from the Fog Image starting at the Translated Location and go the full width of our client view only. Note that we
                // explicitly Min/Max the values to prevent trying to source from off the image.
                clientContext.drawImage(ClientState.Fog, 0, 0);
            }
            
            if (ClientState.ShowGrid)
            {
                // Because our Context instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
                // start at the Translated location, and go the full width of our client view only. Note that because our scroll
                // may be in between two grid lines, we'll need to pull back (or go forward) one full step in all directions to ensure
                // the client view is fully grid lined. The end result is that we'll only draw as many grid lines as we need, starting 
                // and ending just beyond what the user can actually see.
            }
        }   
        clientContext.restore();
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
    
    clientCanvas.addEventListener('mousedown', onMouseDown);
    clientCanvas.addEventListener('mousemove', onMouseMove);
    clientCanvas.addEventListener('mouseup', onMouseUp);
});