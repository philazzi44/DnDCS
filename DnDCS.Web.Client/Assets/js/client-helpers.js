function log (messageId, message) {
    var now = new Date();
    console.log(now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds() + ":" + now.getMilliseconds() + " - " + messageId + " - " + message);
}