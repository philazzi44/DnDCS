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
        clientContext.save();
        {
            clientContext.translate(-ClientState.ScrollPositionX, -ClientState.ScrollPositionY);
            if (ClientState.ZoomFactor != 1.0)
                clientContext.scale(ClientState.ZoomFactor, ClientState.ZoomFactor);
            
            drawClient_Map();
            drawClient_Grid();
            drawClient_Fog();
        }   
        clientContext.restore();
    }
    
    drawClient_ZoomFactor();
}

function drawClient_Map() {
    if (ClientState.Map != null)
    {
        // Because our Context instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
        // take from the Map Image starting at the Translated Location and go the full width of our client view only. Note that we
        // explicitly Min/Max the values to prevent trying to source from off the image.
        var sourceX = Math.max(0, ClientState.ScrollPositionX);
        var sourceY = Math.max(0, ClientState.ScrollPositionY);
        var sourceWidth = Math.min(ClientState.MapWidth - sourceX, clientCanvasWidth);
        var sourceHeight = Math.min(ClientState.MapHeight - sourceY, clientCanvasHeight);
        var destinationX = Math.max(0, ClientState.ScrollPositionX);
        var destinationY = Math.max(0, ClientState.ScrollPositionY);
        var destinationWidth = Math.min(ClientState.MapWidth - sourceX, clientCanvasWidth);
        var destinationHeight = Math.min(ClientState.MapHeight - sourceY, clientCanvasHeight);
        
        clientContext.drawImage(ClientState.Map,
                                sourceX, sourceY, sourceWidth, sourceHeight, 
                                destinationX, destinationY, destinationWidth, destinationHeight);
    }
}

function drawClient_Grid() {
    if (ClientState.ShowGrid)
    {
        // Because our Context instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
        // start at the Translated location, and go the full width of our client view only. Note that because our scroll
        // may be in between two grid lines, we'll need to pull back (or go forward) one full step in all directions to ensure
        // the client view is fully grid lined. The end result is that we'll only draw as many grid lines as we need, starting 
        // and ending just beyond what the user can actually see.
        var startX = ClientState.ScrollPositionX;
        startX = startX - (startX % ClientState.GridSize);
        var endX = ClientState.ScrollPositionX + clientCanvasWidth;
        endX = endX + (ClientState.GridSize - (endX % ClientState.GridSize));

        var startY = ClientState.ScrollPositionY;
        startY = startY - (startY % ClientState.GridSize);
        var endY = ClientState.ScrollPositionY + clientCanvasHeight;
        endY = endY + (ClientState.GridSize - (endY % ClientState.GridSize));

        // Constrain our start/end to within the map itself.
        startX = Math.max(0, Math.min(startX, ClientState.MapWidth));
        startY = Math.max(0, Math.min(startY, ClientState.MapHeight));
        endX = Math.min(endX, ClientState.MapWidth);
        endY = Math.min(endY, ClientState.MapHeight);

        clientContext.strokeStyle = ClientState.GridColor;
        clientContext.beginPath();
        var x = startX;
        var y = startY;
        while (x < endX || y < endY)
        {
            if (x < endX)
            {
                clientContext.moveTo(x, startY);
                clientContext.lineTo(x, endY);
                x += ClientState.GridSize;
            }
            if (y < endY)
            {
                clientContext.moveTo(startX, y);
                clientContext.lineTo(endX, y);
                y += ClientState.GridSize;
            }
        }
        clientContext.stroke();
    }
}

function drawClient_Fog() {
    if (ClientState.Fog != null)
    {
        // Because our Context instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
        // take from the Fog Image starting at the Translated Location and go the full width of our client view only. Note that we
        // explicitly Min/Max the values to prevent trying to source from off the image.
        var sourceX = Math.max(0, ClientState.ScrollPositionX);
        var sourceY = Math.max(0, ClientState.ScrollPositionY);
        var sourceWidth = Math.min(ClientState.FogWidth - sourceX, clientCanvasWidth);
        var sourceHeight = Math.min(ClientState.FogHeight - sourceY, clientCanvasHeight);
        var destinationX = Math.max(0, ClientState.ScrollPositionX);
        var destinationY = Math.max(0, ClientState.ScrollPositionY);
        var destinationWidth = Math.min(ClientState.FogWidth - sourceX, clientCanvasWidth);
        var destinationHeight = Math.min(ClientState.FogHeight - sourceY, clientCanvasHeight);
        
        clientContext.drawImage(ClientState.Fog,
                                sourceX, sourceY, sourceWidth, sourceHeight, 
                                destinationX, destinationY, destinationWidth, destinationHeight);
    }
}

function drawClient_ZoomFactor() {
    if (ClientState.IsZoomFactorInProgress)
    {
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
}