function log(messageId, message) {
    var now = new Date();
    console.log(now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds() + ":" + now.getMilliseconds() + " - " + messageId + " - " + message);
}

function getBoundedScrollX(x) {
    return Math.min(ClientState.MapWidth - clientCanvasWidth, Math.max(0, x));
}

function getBoundedScrollY(y) {
    return Math.min(ClientState.MapHeight - clientCanvasHeight, Math.max(0, y));
}

function setScroll(x, y){
    ClientState.ScrollPositionX = getBoundedScrollX(x);
    ClientState.ScrollPositionY = getBoundedScrollY(y);
    ClientState.NeedsRedraw = true;
}