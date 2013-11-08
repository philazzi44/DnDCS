function clientCanvas_MouseLeftClick(e) {
    if (ClientState.IsZoomFactorInProgress && (e.button == 0 || e.button == 2)) {
        commitOrRollBackZoom(e.button == 0);
    }
    else if (!ClientState.IsZoomFactorInProgress && e.button == 2)
    {
        tryEnableZoom();
    }
}

// This is actually wired up to the Context Menu of the Canvas, but we're simulating it.
function clientCanvas_MouseRightClick(e) {
    clientCanvas_MouseLeftClick(e);
    return false;    
}

function clientCanvas_MouseDown(e) {
    if (ClientState.IsZoomFactorInProgress)
        return;
        
    if (e.button == 0)
        enableDragScroll(e);
}

function clientCanvas_MouseMove(e) {
    if (ClientState.IsDragScrolling)
    {
        // If we need to cause a minimum-threshold for dragging, we can put it here.
        var moveThreshold = 0;
        
        if (e.button == 0 || e.which == 1)
        {
            var newMouseLocationX = e.clientX - clientCanvasX;
            var newMouseLocationY = e.clientY - clientCanvasY;
            
            // Scroll based on the amount of movement.
            var diffY = Math.abs(newMouseLocationY - ClientState.LastMouseLocationY);
            if (diffY > moveThreshold) {
                if (newMouseLocationY < ClientState.LastMouseLocationY)
                    scrollUpOrDown(false, diffY);
                else if (newMouseLocationY > ClientState.LastMouseLocationY)
                    scrollUpOrDown(true, diffY);
            }
            
            var diffX = Math.abs(newMouseLocationX - ClientState.LastMouseLocationX);
            if (diffX > moveThreshold)
            {
                if (newMouseLocationX < ClientState.LastMouseLocationX)
                    scrollLeftOrRight(false, diffX);
                else if (newMouseLocationX > ClientState.LastMouseLocationX)
                    scrollLeftOrRight(true, diffX);
            }

            ClientState.LastMouseLocationX = newMouseLocationX;
            ClientState.LastMouseLocationY = newMouseLocationY;            
            ClientState.NeedsRedraw = true;
        }
        else
        {        
            // If we had the Scroll logic set set but our button is no longer pressed, then we'll stop the drag altogether.
            disableDragScroll();
        }
    }
}

function clientCanvas_MouseUp(e) {
    if (e.button == 0)
        disableDragScroll();
}

function clientCanvas_MouseWheel(e) {
    if (e.wheelDelta == 0)
        return;

    var isShift = e.shiftKey;
    
    if (ClientState.IsZoomFactorInProgress)
    {
        zoomInOrOut((e.wheelDelta > 0), isShift);
    }
    else if (isShift)
    {
        scrollLeftOrRight((e.wheelDelta > 0));
    }
    else
    {
        scrollUpOrDown((e.wheelDelta > 0));
    }  
}

function window_KeyDown(e) {
    var code = e.keyCode;
    switch (code) {
        //Left key
        case 37: 
            if (!ClientState.IsZoomFactorInProgress)
            {
                scrollLeftOrRight(true, null, ClientState.KeyboardScrollAccel);
                ClientState.KeyboardScrollAccel += StaticAssets.KeyboardScrollAccelStep;
            }
            break;
        //Up key
        case 38: 
            if (ClientState.IsZoomFactorInProgress)
            {
                zoomInOrOut(true, e.shiftKey);
            }
            else
            {
                scrollUpOrDown(true, null, ClientState.KeyboardScrollAccel);
                ClientState.KeyboardScrollAccel += StaticAssets.KeyboardScrollAccelStep;
            }
            break;
        //Right key
        case 39: 
            if (!ClientState.IsZoomFactorInProgress)
            {
                scrollLeftOrRight(false, null, ClientState.KeyboardScrollAccel);
                ClientState.KeyboardScrollAccel += StaticAssets.KeyboardScrollAccelStep;
            }
            break;
        //Down key
        case 40:
            if (ClientState.IsZoomFactorInProgress)
            {
                zoomInOrOut(false, e.shiftKey);
            }
            else
            {
                scrollUpOrDown(false, null, ClientState.KeyboardScrollAccel);
                ClientState.KeyboardScrollAccel += StaticAssets.KeyboardScrollAccelStep;
            }
            break;
            
        // Escape key
        case 27:
            if (ClientState.IsZoomFactorInProgress)
                commitOrRollBackZoom(false);
            break;
            
        // Enter key
        case 13:
            if (ClientState.IsZoomFactorInProgress)
                commitOrRollBackZoom(true);
            break;
            
        default: 
            // Z key - Not switching because of ambiguity in case.
            if (String.fromCharCode(e.keyCode).toUpperCase() == "Z")
                tryEnableZoom();
            break;
    }    
}

function window_KeyUp(e) {
    var code = e.keyCode;
    switch (code) {
        //Left key
        //Up key
        //Right key
        //Down key
        case 37: 
        case 38: 
        case 39: 
        case 40:
            ClientState.KeyboardScrollAccel = StaticAssets.KeyboardScrollAccelDefault;
            break;
        default:
            return;
    }
}

function tryEnableZoom() {
    if (!ClientState.IsZoomFactorInProgress)
    {
        ClientState.IsZoomFactorInProgress = true;
        ClientState.NeedsRedraw = true;
    }
}
function enableDragScroll(e) {
    if (ClientState.IsDragScrolling)
        return;
    // Never allow scrolling while we're in the middle of a zoom.
    if (ClientState.IsZoomFactorInProgress)
        return;
    
    ClientState.IsDragScrolling = true;
    ClientState.LastMouseLocationX = e.clientX - clientCanvasX;
    ClientState.LastMouseLocationY = e.clientY - clientCanvasY;
    ClientState.NeedsRedraw = true;
}

function disableDragScroll() {
    if (!ClientState.IsDragScrolling)
        return;
    ClientState.IsDragScrolling = false;
    ClientState.NeedsRedraw = true;
}