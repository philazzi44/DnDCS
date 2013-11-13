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
};
    
function tryConnect() {    
    ClientState.IsConnecting = true;
    
    var webSocketString = "ws://" + ClientState.Host + ":" + ClientState.Port + "/service";
    var webSocket = new WebSocket(webSocketString);
    webSocket.onopen = onConnectionOpened;
    webSocket.onmessage = onMessageReceived;
    webSocket.onclose = onConnectionClosed;
    webSocket.onerror = onConnectionError;
}
    
function onConnectionOpened(e){
    ClientState.IsConnecting = false;
    ClientState.IsConnected = true;
        
    document.title = 'Connected to ' + ClientState.Host + ":" + ClientState.Port;

    clientContext = clientCanvas.getContext("2d");
            
    // Set all the assets we need to load, which should also be checked in the below
    // connectInitWait interval.
    var blackoutImage = new Image();
    blackoutImage.onload = function() { StaticAssets.BlackoutImage = blackoutImage; }
    blackoutImage.src = document.URL.substring(0, document.URL.lastIndexOf("/")) + StaticAssets.BlackoutImagePath;
    
    // Check all the static assets and the bigger pieces of connection data being loaded before starting the actual application.
    var connectInitWait = window.setInterval(function() {
        if (StaticAssets.BlackoutImage == null)
            return;
        if (ClientState.Map == null)
            return;
        if (ClientState.Fog == null)
            return;
                                
        $('#connectingValues').fadeOut(function() {
            // Stop the connection initialization interval that kept checking to see if we were initialized.
            window.clearInterval(connectInitWait);

            // Add all events for the canvas that the user interacts with.
            clientCanvas.addEventListener('mousedown', clientCanvas_MouseDown);
            clientCanvas.addEventListener('click', clientCanvas_MouseLeftClick);
            clientCanvas.addEventListener('mousemove', clientCanvas_MouseMove);
            clientCanvas.addEventListener('mouseup', clientCanvas_MouseUp);
            clientCanvas.addEventListener('mousewheel', clientCanvas_MouseWheel);
            window.addEventListener('keydown', window_KeyDown);
            window.addEventListener('keyup', window_KeyUp);
            clientCanvas.oncontextmenu = clientCanvas_MouseRightClick;

            // Start the 30FPS Draw Loop now and show the canvas to the user.
            window.setInterval(drawClient, 33);
            ClientState.NeedsRedraw = true;
            $('#connectedValues').fadeIn("slow");
        });
    }, 33);
}
    
function onConnectionClosed(e){
    ClientState.IsConnected  = false;
    ClientState.IsClosed = true;
    
    $('#connectedValues').fadeOut();
        
    // If we're already Errored, then the Error message is being shown.
    if (!ClientState.IsErrored)
    {
        document.title = 'Disconnected from ' + ClientState.Host + ":" + ClientState.Port;

        $('#disconnectedValues').fadeIn();
    }
}

function onConnectionError(e){
    log(null, "Error: " + e.data);
    ClientState.IsErrored = true;
    
    if (ClientState.IsConnecting)
    {
        document.title = 'Failed to connect to ' + ClientState.Host + ":" + ClientState.Port;

        ClientState.IsConnecting = false;
        $('#connectingValues').fadeOut();
        $('#serverNotFoundValues').fadeIn();
    }
    else
    {
        document.title = 'Error from ' + ClientState.Host + ":" + ClientState.Port;

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
            onSliceCallback = processGridColorMessage;
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
    // The point that came in is raw on the map...
    var centerMapX = messageDataView.getInt32(0, true);
    var centerMapY = messageDataView.getInt32(4, true);

    setCenterMap(centerMapX, centerMapY);
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
                // logic as the server. Therefore, unfortunately, we're forced to do a crazy loop to manually apply the mask.
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
    var isClearing = (messageDataView.getUint8(0) == 1);
    
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
    var fogAll = (messageDataView.getUint8(0) == 1);
            
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
    ClientState.UseFogAlphaEffect = (messageDataView.getUint8(0) == 1);
}

function processGridSizeMessage(messageDataView) {
    // Next byte is the flag indicating whether to use the effect or not.
    var showGrid = (messageDataView.getUint8(0) == 1);
    
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
    var a = messageDataView.getUint8(0);
    var r = messageDataView.getUint8(1);
    var g = messageDataView.getUint8(2);
    var b = messageDataView.getUint8(3);
    
    ClientState.GridColor = "rgba(" + r + "," + g + "," + b + "," + a + ")";
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
    var socketActionByte = messageDataView.getUint8(MESSAGE_INDEXES.SocketAction);
    
    for (var socketAction in SOCKET_ACTIONS)
    {
        if (SOCKET_ACTIONS[socketAction].value == socketActionByte)
            return SOCKET_ACTIONS[socketAction];
    }
    
    return SOCKET_ACTIONS.Unknown;
}