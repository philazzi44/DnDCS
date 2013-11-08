function log(messageId, message) {
    var now = new Date();
    console.log(now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds() + ":" + now.getMilliseconds() + " - " + messageId + " - " + message);
}

function scrollLeftOrRight(isLeft, distance, factor)
{
    if (distance == undefined)
        distance = null;
    if (factor == undefined)
        factor = 1.0;
        
    // Scroll left/right
    var newValue;
    if (isLeft)
    {
        if (distance == null)
            newValue = ClientState.ScrollPositionX - (ClientState.MapWidth * StaticAssets.ScrollWheelStepScrollPercent * factor);
        else
            newValue = ClientState.ScrollPositionX - (distance * factor);
    }
    else
    {
        if (distance == null)
            newValue = ClientState.ScrollPositionX + (ClientState.MapWidth * StaticAssets.ScrollWheelStepScrollPercent * factor);
        else
            newValue = ClientState.ScrollPositionX + (distance * factor);
    }
    setScroll(newValue, null);
}

function scrollUpOrDown(isUp, distance, factor)
{
    if (distance == undefined)
        distance = null;
    if (factor == undefined)
        factor = 1.0;
        
    // Scroll up/down
    var newValue;
    if (isUp)
    {    
        if (distance == null)
            newValue = ClientState.ScrollPositionY - (ClientState.MapHeight * StaticAssets.ScrollWheelStepScrollPercent * factor);
        else
            newValue = ClientState.ScrollPositionY - (distance * factor);        
    }
    else
    {
        if (distance == null)
            newValue = ClientState.ScrollPositionY + (ClientState.MapHeight * StaticAssets.ScrollWheelStepScrollPercent * factor);
        else
            newValue = ClientState.ScrollPositionY + (distance * factor);
    }
    setScroll(null, newValue);
}

function setScroll(desiredX, desiredY) {
    if (desiredX == undefined || desiredX == null)
        desiredX = ClientState.ScrollPositionX;
    if (desiredY == undefined || desiredY == null)
        desiredY = ClientState.ScrollPositionY;
        
    // Do not allow negative scrolling in any way.
    if (desiredX < 0)
        desiredX = 0;
    if (desiredY < 0)
        desiredY = 0;
              
    var logicalMapWidth = ClientState.MapWidth * ClientState.ZoomFactor;
    var logicalMapHeight = ClientState.MapHeight * ClientState.ZoomFactor;

    // If the map we are showing is smaller than the width/height, then no X/Y scrolling is allowed at all.
    // Otherwise, enforce that the value is at most the amount that would be needed to show the full map given the current size of the visible area.
    if (logicalMapWidth < clientCanvasWidth)
        desiredX = 0;
    else
        desiredX = Math.min(desiredX, ((logicalMapWidth - clientCanvasWidth) * ClientState.InverseZoomFactor));

    if (logicalMapHeight < clientCanvasHeight)
        desiredY = 0;
    else
        desiredY = Math.min(desiredY, ((logicalMapHeight - clientCanvasHeight) * ClientState.InverseZoomFactor));

    desiredX = Math.floor(desiredX);
    desiredY = Math.floor(desiredY);
    ClientState.ScrollPositionX = desiredX;
    ClientState.ScrollPositionY = desiredY;
    ClientState.NeedsRedraw = true;
}

function zoomInOrOut(zoomIn, doubleFactor)
{
    var step = StaticAssets.ZoomStep;
    if (doubleFactor)
        step = StaticAssets.ZoomLargeStep;
        
    if (zoomIn)
        ClientState.VariableZoomFactor = Number(Math.min(ClientState.VariableZoomFactor + step, StaticAssets.MaximumZoomFactor).toFixed(1));
    else
        ClientState.VariableZoomFactor = Number(Math.max(ClientState.VariableZoomFactor - step, StaticAssets.MinimumZoomFactor).toFixed(1));

    ClientState.IsZoomFactorInProgress = true;
    ClientState.NeedsRedraw = true;
}

function commitOrRollBackZoom(commit)
{
    // Commit or rollback the zoom factor.
    ClientState.IsZoomFactorInProgress = false;
    if (commit)
    {
        ClientState.ZoomFactor = ClientState.VariableZoomFactor;
        ClientState.InverseZoomFactor = 1.0 / ClientState.ZoomFactor;
        // This will validate that the current scroll values aren't too large for the new zoom factor.
        setScroll();
    }
    else
    {
        ClientState.VariableZoomFactor = ClientState.ZoomFactor;
    }
    ClientState.NeedsRedraw = true;
}