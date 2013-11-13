function log(messageId, message) {
    var now = new Date();
    console.log(now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds() + ":" + now.getMilliseconds() + " - " + messageId + " - " + message);
}

function scrollLeftOrRight(isLeft, distance, factor)
{
    if (ClientState.IsFlippedView)
        isLeft = !isLeft;
        
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
              
    var logicalMapWidth = ClientState.MapWidth * ClientState.AssignedZoomFactor;
    var logicalMapHeight = ClientState.MapHeight * ClientState.AssignedZoomFactor;

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

function setCenterMap(centerMapX, centerMapY)
{
    // The point that came in is raw on the map...
    // We also need to account for the client's zoom factor (gives us the X/Y of a Zoomed map), to which we then "unzoom" the X/Y back to the raw map location for scroll purposes.
    var scrollX = Math.floor(((centerMapX * ClientState.AssignedZoomFactor) - (clientCanvasWidth / 2.0)) * ClientState.InverseZoomFactor);
    var scrollY = Math.floor(((centerMapY * ClientState.AssignedZoomFactor) - (clientCanvasHeight / 2.0)) * ClientState.InverseZoomFactor);

    setScroll(scrollX, scrollY);
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
        // The ScrollPosition we have is in real map coordinates, so we add the appropriate amount of Width as per how much the map is actually showing.
        var oldCenterMapX = ClientState.ScrollPositionX;
        oldCenterMapX += (clientCanvasWidth / 2 * ClientState.InverseZoomFactor);
        var oldCenterMapY = ClientState.ScrollPositionY;
        oldCenterMapY += (clientCanvasHeight / 2 * ClientState.InverseZoomFactor);

        ClientState.AssignedZoomFactor = ClientState.VariableZoomFactor;
        ClientState.InverseZoomFactor = 1.0 / ClientState.AssignedZoomFactor;
        
        // This will attempt to re-center on the center we had, and will adjust as needed to fit the new zoom factor.
        setCenterMap(oldCenterMapX, oldCenterMapY);
    }
    else
    {
        ClientState.VariableZoomFactor = ClientState.AssignedZoomFactor;
    }
    ClientState.NeedsRedraw = true;
}