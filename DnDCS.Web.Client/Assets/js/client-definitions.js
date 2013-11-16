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
    
    IsTouchScreen : (window.ontouchstart !== undefined),
    
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
    
    AssignedZoomFactor : 1.0,
    InverseZoomFactor : 1.0,
    VariableZoomFactor : 1.0,
    VariableZoomFactor_LastTouchClick : 1.0,
    IsZoomFactorInProgress : false,
                
    IsFlippedView : false,
    
    CenterMapFadingID : null,
    CenterMapFadingCurrentFadeOut : null,
    CenterMapFadingCurrentFadeFactor : null,
    CenterMapFadingFinalX : null,
    CenterMapFadingFinalY : null,
};

// A Queue of Messages that have been received which must be processed.
var messageQueue = [];
var messageIdCounter = new Number();

// The client's visible canvas. Values are set after a connection is established.
var clientCanvas;
var clientCanvasWidth;
var clientCanvasHeight;
var clientContext;

// A backing canvas used when Fog-related messages are processed.
var newFogCanvas = document.createElement('canvas');
var newFogContext = newFogCanvas.getContext("2d");

// Static assets loaded after a connection is established.
var StaticAssets = {
    DefaultServer : "192.168.2.4",
    DefaultPort : "11001",

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
    
    ZoomInstructionMessage1 : (ClientState.IsTouchScreen) ? "Tap to commit the zoom factor." : "Press Enter or Left Click to commit the zoom factor.",
    ZoomInstructionMessage2 : (ClientState.IsTouchScreen) ? "Long tap to cancel." : "Press Escape or Right Click to cancel.",    
    
    MinimumZoomFactor : 0.2,
    ZoomStep : 0.1,
    ZoomLargeStep : 0.2,
    MaximumZoomFactor : 10.0,
    
    // The fade goes from 0.0 to 1.0, so a 0.2 step size means that much every Interval on centerMapFadingTimer 
    // (which is 50), so 5 steps (250ms) fade out and then repeated to fade in.
    CenterMapStepSize : 0.2,
    CenterMapTimeout : 50,
};