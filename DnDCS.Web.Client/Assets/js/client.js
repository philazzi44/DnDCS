$(document).ready(function(){    
    // Default Values for UI fields.
    $('#host').val(defaultServer);
    $('#port').val(defaultPort);
    
    // Wire up all events that make sense for now.
    $('#btnConnect').click(btnConnect_Click);
});