// TODO: Should these really be hardcoded like this?
var defaultServer = "pazzi.parse3.local";
// var defaultServer = "172.30.3.11";
// var defaultServer = "desktop-win7";
var defaultPort = "11001";

// TODO: Definitely should be defined elsewhere, maybe in a way that we don't have to keep it updated
// if the C# version updates?
var SOCKET_ACTIONS = {
    Unknown : {value: 0, name: "Unknown"}, 
    Acknowledge: {value: 1, name: "Acknowledge"}, 
    Ping: {value: 2, name: "Ping"}, 
    Map : {value: 3, name: "Map"},
    CenterMap : {value: 4, name: "CenterMap"},
    Fog : {value: 5, name: "Fog"},
    FogUpdate : {value: 6, name: "FogUpdate"},
    FogOrRevealAll : {value: 7, name: "FogOrRevealAll"},
    UseFogAlphaEffect : {value: 8, name: "UseFogAlphaEffect"},
    GridSize : {value: 9, name: "GridSize"},
    GridColor : {value: 10, name: "GridColor"},
    BlackoutOn : {value: 11, name: "BlackoutOn"},
    BlackoutOff : {value: 12, name: "BlackoutOff"},
    Exit : {value: 13, name: "Exit"}        
};

// Constant values.
var MESSAGE_INDEXES = {
    MessageSize : 0,
    SocketAction : 4,
    Remainder : 5,
};

// The current state of the Client instance.
var ClientState = {
    Host : null,
    Port : null,
    IsConnecting : false,
    IsConnected : false,
    IsClosed : false,
    IsErrored : false,
    
    NeedsRedraw : false,
    
    AcknowledgedReceived : false,
    IsBlackoutOn : false,
    ShowGrid : false,
    UseFogAlphaEffect : false,
    GridSize : 0,
    GridColor : "rgba(0,255,255,255)",
    
    Map : null,
    MapWidth : 0,
    MapHeight : 0,
    Fog : null,
    FogWidth : 0,
    FogHeight : 0,
    
    IsDragScrolling : false,
    LastMouseLocationX : 0,
    LastMouseLocationY : 0,
    ScrollPositionX : 0,
    ScrollPositionY : 0,
    KeyboardScrollAccel : 1.0,
    
    ZoomFactor : 1.0,
    VariableZoomFactor : 1.0,
    IsZoomFactorInProgress : false,
};

// A Queue of Messages that have been received which must be processed.
var messageQueue = [];
var messageIdCounter = new Number();

// The client's visible canvas. Values are set after a connection is established.
var clientCanvas;
var clientCanvasX;
var clientCanvasY;
var clientCanvasWidth;
var clientCanvasHeight;
var clientContext;

// A backing canvas used when Fog-related messages are processed.
var newFogCanvas = document.createElement('canvas');
var newFogContext = newFogCanvas.getContext("2d");

// Static assets loaded after a connection is established.
var StaticAssets = {
    BlackoutImagePath : "/Assets/images/BlackoutImage.png",
    BlackoutImage : null,
    
    NewFogColor : "black",
    // While it would seem logical to assign this as being 100% Alpha, we're using Compositing
    // to properly "overwrite" the destination image, so any color is fine.
    NewFogClearColor : "white",
    
    ScrollWheelStepScrollPercent : 0.05,
    
    KeyboardScrollAccelStep : 0.5,
    KeyboardScrollAccelDefault : 1.0,
    
    ZoomMessageFont : "24px sans-serif",
    // Since nothing can tell us the height of a text given a font, we'll simply guess based on the Font we assign.
    ZoomMessageHeight : 25,
    ZoomMessageColor : "aqua",
    
    ZoomInstructionMessage1 : "Press Enter or Left Click to commit the zoom factor.",
    ZoomInstructionMessage2 : "Press Escape or Right Click to cancel.",    
    
    MinimumZoomFactor : 0.2,
    ZoomStep : 0.1,
    ZoomLargeStep : 0.2,
    MaximumZoomFactor : 10.0,
};