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
    }
    else
    {    
        if (ClientState.IsFlippedView)
        {
            clientContext.save();        
            clientContext.translate(clientCanvasWidth, 0);
            clientContext.scale(-1, 1);
        }
        
        drawClient_Map();
        drawClient_Grid();
        drawClient_Fog();
        
        if (ClientState.IsFlippedView)
        {
            clientContext.restore();
        }   
    }
    
    drawClient_ZoomFactor();
}

function drawClient_Map() {
    if (ClientState.Map == null)
        return;

    var sourceX = Math.max(0, ClientState.ScrollPositionX);
    var sourceY = Math.max(0, ClientState.ScrollPositionY);
    var sourceWidth = Math.floor(clientCanvasWidth * ClientState.InverseZoomFactor);
    var sourceHeight = Math.floor(clientCanvasHeight * ClientState.InverseZoomFactor);
    var destinationX = 0;
    var destinationY = 0;
    var destinationWidth = clientCanvasWidth;
    var destinationHeight = clientCanvasHeight;
    
    clientContext.drawImage(ClientState.Map,
                            sourceX, sourceY, sourceWidth, sourceHeight, 
                            destinationX, destinationY, destinationWidth, destinationHeight);
}

function drawClient_Grid() {
    if (!ClientState.ShowGrid)
        return;
    
    // To take into account the Zooming, we'll force our Grid Size to be the zoomed in/out amount.
    var logicalGridSize = Math.round(ClientState.GridSize * ClientState.AssignedZoomFactor);
    var logicalMapWidth = Math.round(ClientState.MapWidth * ClientState.AssignedZoomFactor);
    var logicalMapHeight = Math.round(ClientState.MapHeight * ClientState.AssignedZoomFactor);

    // Our starting points will be however much of the grid (backwards) we're cutting off based on how much has been scrolled.
    // Our ending points will be the full size of what is visible to the user (full canvas, or the full map that fits on the larger canvas).
    var startX = -((ClientState.ScrollPositionX * ClientState.AssignedZoomFactor) % logicalGridSize);
    var endX = Math.min(logicalMapWidth, clientCanvasWidth);

    var startY = -((ClientState.ScrollPositionY * ClientState.AssignedZoomFactor) % logicalGridSize);
    var endY = Math.min(logicalMapHeight, clientCanvasHeight);

    clientContext.save();
    {
        clientContext.strokeStyle = ClientState.GridColor;
        clientContext.beginPath();
        var x = startX;
        var y = startY;
        while (x <= endX || y <= endY)
        {
            if (x <= endX)
            {
                clientContext.moveTo(x, startY);
                clientContext.lineTo(x, endY);
                x += logicalGridSize;
            }
            if (y <= endY)
            {
                clientContext.moveTo(startX, y);
                clientContext.lineTo(endX, y);
                y += logicalGridSize;
            }
        }
        clientContext.stroke();
    }
    clientContext.restore();
}

function drawClient_Fog() {
    if (ClientState.Fog == null)
        return;
    
    var sourceX = Math.max(0, ClientState.ScrollPositionX);
    var sourceY = Math.max(0, ClientState.ScrollPositionY);
    var sourceWidth = Math.floor(clientCanvasWidth * ClientState.InverseZoomFactor);
    var sourceHeight = Math.floor(clientCanvasHeight * ClientState.InverseZoomFactor);
    var destinationX = 0;
    var destinationY = 0;
    var destinationWidth = clientCanvasWidth;
    var destinationHeight = clientCanvasHeight;
    
    clientContext.drawImage(ClientState.Fog,
                            sourceX, sourceY, sourceWidth, sourceHeight, 
                            destinationX, destinationY, destinationWidth, destinationHeight);
}

function drawClient_ZoomFactor() {
    if (!ClientState.IsZoomFactorInProgress)
        return;

    clientContext.save();
    {
        clientContext.font = StaticAssets.ZoomMessageFont;
        clientContext.fillStyle = StaticAssets.ZoomMessageColor;
        
        var zoomMsgs = [];
        zoomMsgs.push("Zoom: " + ClientState.VariableZoomFactor + "x");
        zoomMsgs.push(StaticAssets.ZoomInstructionMessage1);
        zoomMsgs.push(StaticAssets.ZoomInstructionMessage2);
        
        for (var i = 0; i < zoomMsgs.length; i++)
        {
            // Draw each line one after the other, separating them by the height of the message, centered on the screen.
            var msgSize = clientContext.measureText(zoomMsgs[i]);
            msgSize.height = StaticAssets.ZoomMessageHeight;
            var x = (clientCanvasWidth / 2.0) - (msgSize.width / 2.0);
            var y = (clientCanvasHeight / 2.0) - (msgSize.height / 2.0) + msgSize.height * i;
            
            if (ClientState.IsBlackoutOn)
                y += StaticAssets.BlackoutImage.height;

            clientContext.fillText(zoomMsgs[i], x, y);
        }
    }
    clientContext.restore();
}