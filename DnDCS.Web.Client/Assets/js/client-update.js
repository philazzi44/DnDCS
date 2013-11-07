function clientCanvas_MouseDown(e) {
    if (e.button != 0)
        return;
    enableDragScroll(e);
}

function clientCanvas_MouseMove(e) {
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
    
    setScroll(ClientState.ScrollPositionX + deltaX, ClientState.ScrollPositionY + deltaY);
          
    ClientState.LastMouseLocationX = newMouseLocationX;
    ClientState.LastMouseLocationY = newMouseLocationY;
    
    ClientState.NeedsRedraw = true;
}

function clientCanvas_MouseUp(e) {
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