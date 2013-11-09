function btnConnect_Click() {
    var host = $('#host').val();
    var port = $('#port').val();
    if (!validateConnectValues(host, port))
        return;
        
    ClientState.Host = host;
    ClientState.Port = port;
    
    document.title = 'Connecting to ' + ClientState.Host + ":" + ClientState.Port + "...";
    
    $('#connectValues').fadeOut(function() {        
        $('#connectingValues').fadeIn(function() {
            tryConnect();
        });
    });
}