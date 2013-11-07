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
        return;
    }
    
    clientContext.save();
    {
        clientContext.translate(-ClientState.ScrollPositionX, -ClientState.ScrollPositionY);
        
        if (ClientState.Map != null)
        {
            // Because our Context instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
            // take from the Map Image starting at the Translated Location and go the full width of our client view only. Note that we
            // explicitly Min/Max the values to prevent trying to source from off the image.
            clientContext.drawImage(ClientState.Map, 0, 0);
        }
            
        if (ClientState.Fog != null)
        {
            // Because our Context instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
            // take from the Fog Image starting at the Translated Location and go the full width of our client view only. Note that we
            // explicitly Min/Max the values to prevent trying to source from off the image.
            clientContext.drawImage(ClientState.Fog, 0, 0);
        }
        
        if (ClientState.ShowGrid)
        {
            // Because our Context instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
            // start at the Translated location, and go the full width of our client view only. Note that because our scroll
            // may be in between two grid lines, we'll need to pull back (or go forward) one full step in all directions to ensure
            // the client view is fully grid lined. The end result is that we'll only draw as many grid lines as we need, starting 
            // and ending just beyond what the user can actually see.
        }
    }   
    clientContext.restore();
}