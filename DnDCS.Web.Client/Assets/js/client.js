$(document).ready(function(){    
    // Default Values for UI fields.
    $('#host').val(StaticAssets.DefaultServer);
    $('#port').val(StaticAssets.DefaultPort);
    
    // Pull down the position and size of the Canvas before we hide it.
    clientCanvas = $('#clientCanvas')[0];                        
    clientCanvasWidth = clientCanvas.getBoundingClientRect().right - clientCanvas.getBoundingClientRect().left;
    clientCanvasHeight = clientCanvas.getBoundingClientRect().bottom - clientCanvas.getBoundingClientRect().top;
    $('#connectedValues').hide();
    
    // Wire up all events that make sense for now.
    $('#btnConnect').click(btnConnect_Click);
});