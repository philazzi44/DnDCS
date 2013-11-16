function clientCanvas_MouseLeftClick(e) {
    if (ClientState.IsTouchScreen)
    {
        // Left click on a touch device will commit the zoom only. Note that the event will fire upon release of the zoom, so we need to ignore the first one that
        // happens by watching the zoom value changing.
        if (ClientState.IsZoomFactorInProgress && (e.button == 0))
        {
            // if (ClientState.VariableZoomFactor == ClientState.VariableZoomFactor_LastTouchClick)
            // {
                commitOrRollBackZoom(e.button == 0);
                
            // }
            // else
            // {
                // // Next time a left click occurs, we'll commit the zoom.
                // ClientState.VariableZoomFactor_LastTouchClick = ClientState.VariableZoomFactor;
            // }
        }            
    }
    else
    {
        // Left click on a desktop will commit the zoom, or enable zooming (if it's the right click being pushed in)
        if (ClientState.IsZoomFactorInProgress && (e.button == 0 || e.button == 2)) {
            commitOrRollBackZoom(e.button == 0);
        }
        else if (!ClientState.IsZoomFactorInProgress && e.button == 2)
        {
            tryEnableZoom();
        }
    }
}

// This is actually wired up to the Context Menu of the Canvas, but we're hijacking it and pretending it's a Right Click.
function clientCanvas_MouseRightClick(e) {
    if (ClientState.IsTouchScreen)
    {
        // Start, or roll back the zoom when a Right Click is detected.
        if (ClientState.IsZoomFactorInProgress)
            commitOrRollBackZoom(false);
        else
            tryEnableZoom();
    }
    else
    {
        // For desktop app, all right clicks are treated as a left click.
        clientCanvas_MouseLeftClick(e);
    }
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
    if (!ClientState.IsZoomFactorInProgress && button == 0)
        enableDragScroll(clientX, clientY);
    setLastMouseLocation(clientX, clientY)
}

function clientCanvas_MouseMove(e) {
    handleCursorMove(e.button, e.which, e.clientX, e.clientY);
}

function clientCanvas_TouchMove(e) {
    if (e.touches.length < 1)
        return;
        
    // Button of 0 and Which of 1 signifies left click.
    e.preventDefault();
    handleCursorMove(0, 1, e.touches[0].clientX, e.touches[0].clientY);
}

function handleCursorMove(button, which, cursorX, cursorY) {
    // If we're zooming with a Touch device, then moving up/down causing the zoom to go up or down.
    // If we're dragging, then we need to drag up/down/left/right as needed.
    if (ClientState.IsDragScrolling || (ClientState.IsTouchScreen && ClientState.IsZoomFactorInProgress))
    {
        // If we need to cause a minimum-threshold for dragging, we can put it here.
        var moveThreshold = 0;
        
        if (button == 0 && which == 1)
        {
            var oldMouseLocationX = ClientState.LastMouseLocationX;
            var oldMouseLocationY = ClientState.LastMouseLocationY;
            setLastMouseLocation(cursorX, cursorY)
            var newMouseLocationX = ClientState.LastMouseLocationX;
            var newMouseLocationY = ClientState.LastMouseLocationY;
            
            // Scroll (or zoom) based on the amount of movement.
            var diffY = Math.abs(newMouseLocationY - oldMouseLocationY);
            var diffX = Math.abs(newMouseLocationX - oldMouseLocationX);
            
            if (ClientState.IsTouchScreen && ClientState.IsZoomFactorInProgress)
            {
                if (diffY > moveThreshold) {
                    if (newMouseLocationY < oldMouseLocationY)
                        zoomInOrOut(true, false);
                    else if (newMouseLocationY > oldMouseLocationY)
                        zoomInOrOut(false, false);
                }
            }
            else
            {
                if (diffY > moveThreshold) {
                    if (newMouseLocationY < oldMouseLocationY)
                        scrollUpOrDown(false, diffY);
                    else if (newMouseLocationY > oldMouseLocationY)
                        scrollUpOrDown(true, diffY);
                }
                
                if (diffX > moveThreshold)
                {
                    if (newMouseLocationX < oldMouseLocationX)
                        scrollLeftOrRight(false, diffX);
                    else if (newMouseLocationX > oldMouseLocationX)
                        scrollLeftOrRight(true, diffX);
                }
            }
        
            ClientState.NeedsRedraw = true;
        }
        else
        {        
            // If we had the Scroll logic set set but our button is no longer pressed, then we'll stop the drag altogether.
            disableDragScroll();
            setLastMouseLocation(clientX, clientY);
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
        
        // When using a touch screen, the first flip of zooming must also keep track of the LastTouchClick value.
        if (ClientState.IsTouchScreen)
            ClientState.VariableZoomFactor_LastTouchClick = ClientState.VariableZoomFactor;
    
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
    
    ClientState.IsDragScrolling = true;
    ClientState.NeedsRedraw = true;
}

function disableDragScroll() {
    if (!ClientState.IsDragScrolling)
        return;
    ClientState.IsDragScrolling = false;
    ClientState.NeedsRedraw = true;
}

function setLastMouseLocation(clientX, clientY) {
    var bounding = clientCanvas.getBoundingClientRect();
    ClientState.LastMouseLocationX = clientX - bounding.left;
    ClientState.LastMouseLocationY = clientY - bounding.top;
}