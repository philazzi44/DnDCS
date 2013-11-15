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
    handleCursorDown(e.button, e.clientX, e.clientY);
}

function clientCanvas_TouchStart(e) {
    if (e.touches.length < 1)
        return;
        
    // Button of 0 signifies left click.
    handleCursorDown(0, e.touches[0].clientX, e.touches[0].clientY);
}

function handleCursorDown(button, clientX, clientY) {
    if (ClientState.IsZoomFactorInProgress)
        return;
    if (button == 0)
        enableDragScroll(clientX, clientY);
}

function clientCanvas_MouseMove(e) {
    handleCursorMove(e.button, e.which, e.clientX, e.clientY);
}

function clientCanvas_TouchMove(e) {
    if (e.touches.length < 1)
        return;
        
    // Button of 0 and Which of 1 signifies left click.
    handleCursorMove(0, 1, e.touches[0].clientX, e.touches[0].clientY);
}

function handleCursorMove(button, which, cursorX, cursorY) {
    if (ClientState.IsDragScrolling)
    {
        // If we need to cause a minimum-threshold for dragging, we can put it here.
        var moveThreshold = 0;
        
        if (button == 0 && which == 1)
        {
            var bounding = clientCanvas.getBoundingClientRect();
            var newMouseLocationX = cursorX - bounding.left;
            var newMouseLocationY = cursorY - bounding.top;
            
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
    handleCursorUp(e.button);
}

function clientCanvas_TouchEnd(e) {
    // Button of 0 signifies left click.
    handleCursorUp(0);
}

function handleCursorUp(button) {
    if (button == 0)
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
            // Not switching on these because of ambiguity in case.
            // Z key - Toggle Zoom
            // F key - Toggle Flip
            if (String.fromCharCode(e.keyCode).toUpperCase() == "Z")
                tryEnableZoom();
            if (String.fromCharCode(e.keyCode).toUpperCase() == "F")
                toggleFlip();
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

function toggleFlip() {
    ClientState.IsFlippedView = !ClientState.IsFlippedView;
    ClientState.NeedsRedraw = true;
}

function enableDragScroll(clientX, clientY) {
    if (ClientState.IsDragScrolling)
        return;
    // Never allow scrolling while we're in the middle of a zoom.
    if (ClientState.IsZoomFactorInProgress)
        return;
    
    var bounding = clientCanvas.getBoundingClientRect();
            
    ClientState.IsDragScrolling = true;
    ClientState.LastMouseLocationX = clientX - bounding.left;
    ClientState.LastMouseLocationY = clientY - bounding.top;
    ClientState.NeedsRedraw = true;
}

function disableDragScroll() {
    if (!ClientState.IsDragScrolling)
        return;
    ClientState.IsDragScrolling = false;
    ClientState.NeedsRedraw = true;
}