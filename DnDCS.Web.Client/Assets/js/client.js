$(document).ready(function(){    
    // Default Values for UI fields.
    $('#host').val(StaticAssets.DefaultServer);
    $('#port').val(StaticAssets.DefaultPort);
    
    // Wire up all events that make sense for now.
    $('#btnConnect').click(btnConnect_Click);
});