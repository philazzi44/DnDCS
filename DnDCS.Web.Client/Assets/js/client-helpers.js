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

function setCenterMap(centerMapX, centerMapY, animate)
{
    if (animate == undefined)
        animate = true;
        
    // The point that came in is raw on the map...
    // We also need to account for the client's zoom factor (gives us the X/Y of a Zoomed map), to which we then "unzoom" the X/Y back to the raw map location for scroll purposes.
    var scrollX = Math.floor(((centerMapX * ClientState.AssignedZoomFactor) - (clientCanvasWidth / 2.0)) * ClientState.InverseZoomFactor);
    var scrollY = Math.floor(((centerMapY * ClientState.AssignedZoomFactor) - (clientCanvasHeight / 2.0)) * ClientState.InverseZoomFactor);

    if (animate)
    {
        // If we're already doing a Center Map fade, we're going to pop out the old values and kill off
        // that timeout, starting a new one.
        // It may glitch a bit to the end user, jumping from a fade % back to 0%, but the end result will be fine.
        // Our fade factor will go from True/0.0 to True/1.0, then flip to False/1.0 down to False/0.0
        ClientState.CenterMapFadingFinalX = scrollX;
        ClientState.CenterMapFadingFinalY = scrollY;
        ClientState.CenterMapFadingCurrentFadeOut = true;
        ClientState.CenterMapFadingCurrentFadeFactor = 0.0;
        
        // We capture the ID in the function, and pass it in for every call. If another SetCenterMap gets called,
        // the IDs won't match and the time-out will not be queued again.
        // DO NOT INLINE THE VAR (fadingID) BELOW OR THE VALUE WON'T BE CAPTURED BY THE FUNC
        var fadingID = new Date();
        ClientState.CenterMapFadingID = fadingID;
        window.setTimeout(function() {centerMapFadingTimeout_Tick(fadingID);}, StaticAssets.CenterMapTimeout);
    }
    else
    {
        setScroll(scrollX, scrollY);
    }
}

function centerMapFadingTimeout_Tick(fadingID)
{
    // If our timer has short circuited (by the ID being null or different) or our values are lost, we shouldn't even be here.
    if (ClientState.CenterMapFadingID == null ||
        ClientState.CenterMapFadingID != fadingID ||
        ClientState.CenterMapFadingCurrentFadeOut == null || 
        ClientState.CenterMapFadingCurrentFadeFactor == null || 
        ClientState.CenterMapFadingFinalX == null ||
        ClientState.CenterMapFadingFinalY == null)
    {
        return;
    }

    var repeatTimeout = true;
    if (ClientState.CenterMapFadingCurrentFadeOut)
    {
        // Fading out
        if (ClientState.CenterMapFadingCurrentFadeFactor < 1.0)
        {
            // Still fading out
            ClientState.CenterMapFadingCurrentFadeFactor = Number(Math.min(1.0, ClientState.CenterMapFadingCurrentFadeFactor + StaticAssets.CenterMapStepSize).toFixed(1));
        }
        else
        {
            // We've reached 100% fade out, so we'll actually set the new center and flip the direction now.
            var centerMapX = ClientState.CenterMapFadingFinalX;
            var centerMapY = ClientState.CenterMapFadingFinalY;
            if (centerMapX != null && centerMapY != null)
                setScroll(centerMapX, centerMapY);
            ClientState.CenterMapFadingCurrentFadeOut = false;
            ClientState.CenterMapFadingCurrentFadeFactor = 1.0;
        }
    }
    else
    {
        // Fading back in
        if (ClientState.CenterMapFadingCurrentFadeFactor > 0.0)
        {
            // Still fading in
            ClientState.CenterMapFadingCurrentFadeFactor = Number(Math.max(0.0, ClientState.CenterMapFadingCurrentFadeFactor - StaticAssets.CenterMapStepSize).toFixed(1));
        }
        else
        {
            // Full faded in now, stop the timer from repeating.
            repeatTimeout = false;
            ClientState.CenterMapFadingID = null;
            ClientState.CenterMapFadingCurrentFadeOut = null;
            ClientState.CenterMapFadingCurrentFadeFactor = null;
            ClientState.CenterMapFadingFinalX = null;
            ClientState.CenterMapFadingFinalY = null;
        }
    }

    if (repeatTimeout)
        window.setTimeout(function() {centerMapFadingTimeout_Tick(fadingID);}, StaticAssets.CenterMapTimeout);
    
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
        // The ScrollPosition we have is in real map coordinates, so we add the appropriate amount of Width as per how much the map is actually showing.
        var oldCenterMapX = ClientState.ScrollPositionX;
        oldCenterMapX += (clientCanvasWidth / 2 * ClientState.InverseZoomFactor);
        var oldCenterMapY = ClientState.ScrollPositionY;
        oldCenterMapY += (clientCanvasHeight / 2 * ClientState.InverseZoomFactor);

        ClientState.AssignedZoomFactor = ClientState.VariableZoomFactor;
        ClientState.InverseZoomFactor = 1.0 / ClientState.AssignedZoomFactor;
        
        // This will attempt to re-center on the center we had, and will adjust as needed to fit the new zoom factor.
        setCenterMap(oldCenterMapX, oldCenterMapY, false);
    }
    else
    {
        ClientState.VariableZoomFactor = ClientState.AssignedZoomFactor;
    }
    ClientState.NeedsRedraw = true;
}