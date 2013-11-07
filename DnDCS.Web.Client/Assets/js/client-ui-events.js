function btnConnect_Click() {
    var host = $('#host').val();
    var port = $('#port').val();
    if (!validateConnectValues(host, port))
        return;
        
    ClientState.Host = host;
    ClientState.Port = port;
    
    var connectingMessage = 'Connecting to ' + ClientState.Host + ":" + ClientState.Port + "...";
    document.title = connectingMessage;
    
    $('#connectValues').fadeOut(function() {        
        $('#connectingServerInfo').text(connectingMessage);            
        $('#connectingValues').fadeIn(function() {
            tryConnect();
        });
    });
}