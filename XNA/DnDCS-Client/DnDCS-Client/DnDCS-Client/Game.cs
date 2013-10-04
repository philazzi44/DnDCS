using System;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using System.Linq;

namespace DnDCS_Client
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private const float scrollDeltaPercent = 0.1f;

        private ClientSocketConnection connection;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D map;
        // Used as a placeholder for a new image coming in, so the next Update cycle will switch the image.
        private object newMapLock = new object();
        private Texture2D newMap;
        private Texture2D fog;
        // Used as a placeholder for a new image coming in, so the next Update cycle will switch the image.
        private object newFogLock = new object();
        private Texture2D newFog;
        private Texture2D blackoutImage;
        private Texture2D noMapImage;

        private Nullable<int> gridSize;
        private Texture2D gridTileImage;
        private Color gridTileColor;

        // TODO: Should be prompted.
        // private string address = "pazzi.parse3.local";
        private string address = "desktop-win7";
        // TODO: Should be prompted.
        private int port = 11000;

        private int lastWheelValue;

        private int verticalScrollPosition;
        private int horizontalScrollPosition;

        private float zoomFactor = 1.0f;
        private const float zoomFactorDelta = 0.1f;

        private const float zoomMinimumFactor = 0.2f;
        private const float zoomMaximumFactor = 5.0f;

        private KeyboardState currentKeyboardState;
        private MouseState currentMouseState;

        private int ActualMapWidth { get { return this.map.Width; } }
        private int ActualMapHeight { get { return this.map.Height; } }
        private int LogicalMapWidth { get { return (int)(ActualMapWidth * zoomFactor); } }
        private int LogicalMapHeight { get { return (int)(ActualMapHeight * zoomFactor); } }

        private int ActualClientWidth { get { return this.Window.ClientBounds.Width; } }
        private int ActualClientHeight { get { return this.Window.ClientBounds.Height; } }
        private int LogicalClientWidth { get { return (int)(ActualClientWidth * zoomFactor); } }
        private int LogicalClientHeight { get { return (int)(ActualClientHeight * zoomFactor); } }

        private SpriteFont debugFont;
        private readonly List<string> debugText = new List<string>();
        private string FullDebugText { get { return string.Join("\n", debugText); } }

        private bool updateTitle;
        private bool isServerNotFound;
        private bool isConnecting;
        private bool isConnected;
        private bool isConnectionClosed;

        private SpriteFont genericMessageFont;
        private bool isBlackoutOn;

        private readonly object fogUpdatesLock = new object();
        private readonly IList<FogUpdate> fogUpdates = new List<FogUpdate>();

        public Game()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024,
                PreferredBackBufferHeight = 768,
            };
            Content.RootDirectory = "Content";
            this.Window.AllowUserResizing = true;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Logger.FileSuffix = "Client";

            connection = new ClientSocketConnection(address, port);
            connection.OnConnectionEstablished += new Action(connection_OnConnectionEstablished);
            connection.OnServerNotFound += new Action(connection_OnServerNotFound);
            connection.OnMapReceived += new Action<SimpleImage>(connection_OnMapReceived);
            connection.OnFogReceived += new Action<SimpleImage>(connection_OnFogReceived);
            connection.OnFogUpdateReceived += new Action<FogUpdate>(connection_OnFogUpdateReceived);
            connection.OnGridSizeReceived += new Action<bool, int>(connection_OnGridSizeReceived);
            connection.OnGridColorReceived += new Action<SimpleColor>(connection_OnGridColorReceived);
            connection.OnBlackoutReceived += new Action<bool>(connection_OnBlackoutReceived);
            connection.OnExitReceived += new Action(connection_OnExitReceived);

            isConnecting = true;
            connection.Start();

            base.Initialize();
        }

        private void connection_OnFogUpdateReceived(FogUpdate fogUpdate)
        {
            lock (fogUpdatesLock)
            {
                fogUpdates.Add(fogUpdate);
            }
        }

        private void connection_OnConnectionEstablished()
        {
            this.isConnected = true;
            this.updateTitle = true;
        }

        private void connection_OnServerNotFound()
        {
            this.isServerNotFound = true;
        }

        private void connection_OnMapReceived(SimpleImage mapImage)
        {
            try
            {
                lock (newMapLock)
                {
                    if (this.newMap != null)
                    {
                        this.newMap.Dispose();
                        this.newMap = null;
                    }

                    // TODO: Width/Height needs to come from the arguments, since we can't infer it from the bytes.
                    this.newMap = new Texture2D(GraphicsDevice, mapImage.Width, mapImage.Height);
                    this.newMap.SetData(mapImage.Bytes);

                    lock (newFogLock)
                    {
                        // Since we received a new map, we'll automatically black out everything with fog until the Server tells us otherwise.
                        this.newFog = new Texture2D(GraphicsDevice, newMap.Width, newMap.Height);
                        this.newFog.SetData<Color>(Enumerable.Repeat(Color.Black, newMap.Width * newMap.Height).ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Map Received Failure", e);
                if (this.newMap != null)
                    this.newMap.Dispose();
                this.newMap = null;

            }
        }
        
        private void connection_OnFogReceived(SimpleImage fogImage)
        {
            try
            {
                lock (newFogLock)
                {
                    // TODO: Width/Height needs to come from the arguments, since we can't infer it from the bytes.
                    this.newFog = new Texture2D(GraphicsDevice, fogImage.Width, fogImage.Height);
                    this.newFog.SetData(fogImage.Bytes);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Fog received failure.", e);
                if (this.newFog != null)
                    this.newFog.Dispose();
                this.newFog = null;
            }
        }

        private void connection_OnGridColorReceived(SimpleColor gridColor)
        {
            gridTileColor = new Color(gridColor.R, gridColor.G, gridColor.B, gridColor.A);
        }

        private void connection_OnGridSizeReceived(bool showGrid, int gridSize)
        {
            this.gridSize = (showGrid) ? gridSize : new Nullable<int>();
        }

        private void connection_OnBlackoutReceived(bool isBlackoutOn)
        {
            this.isBlackoutOn = isBlackoutOn;
        }

        private void connection_OnExitReceived()
        {
            isConnectionClosed = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            this.blackoutImage = this.Content.Load<Texture2D>("BlackoutImage");
            this.noMapImage = this.Content.Load<Texture2D>("NoMapImage");
            this.debugFont = this.Content.Load<SpriteFont>("Debug");
            this.genericMessageFont = this.Content.Load<SpriteFont>("GenericMessage");

            gridTileImage = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            gridTileImage.SetData<Color>(new[] { Color.White });

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (this.map != null)
                this.map.Dispose();
            if (this.newMap != null)
                this.newMap.Dispose();
            if (this.fog != null)
                this.fog.Dispose();
            if (this.newFog != null)
                this.newFog.Dispose();
            if (this.blackoutImage != null)
                this.blackoutImage.Dispose();
            if (this.connection != null)
                this.connection.Stop();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!this.isConnected)
                return;

            debugText.Clear();

            // TODO: This stinks, because we need to check the condition every single update... Have to do this until I figure out how to post to the Game thread.
            if (updateTitle)
            {
                updateTitle = false;
                this.Window.Title = string.Format("DnDCS Client - Connected to {0}:{1}", connection.Address, connection.Port);
            }

            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();
            
            Update_TryUseNewMap();
            Update_TryUseNewFog();
            
            if (currentMouseState.ScrollWheelValue != lastWheelValue)
            {
                Update_HandleScroll();
                Update_HandleZoom();
            }

            lastWheelValue = currentMouseState.ScrollWheelValue;

            debugText.Add("Zoom Factor: " + zoomFactor);
            debugText.Add("Vertical Scroll Position: " + verticalScrollPosition);
            debugText.Add("Horizontal Scroll Position: " + horizontalScrollPosition);
            if (map != null)
            {
                debugText.Add("Map Size: " + map.Width + "x" + map.Height);
                debugText.Add("Map Bounds: " + ActualMapWidth + "x" + ActualMapHeight);
                debugText.Add("Logical Map Bounds: " + LogicalMapWidth + "x" + LogicalMapHeight);
            }
            debugText.Add("Client Bounds: " + ActualClientWidth + "x" + ActualClientHeight);
            debugText.Add("Logical Client Bounds: " + LogicalClientWidth + "x" + LogicalClientHeight);
            base.Update(gameTime);
        }

        private void Update_TryUseNewMap()
        {
            if (this.newMap != null)
            {
                lock (newMapLock)
                {
                    if (this.map != null)
                        this.map.Dispose();
                    this.map = this.newMap;
                    this.newMap = null;
                }
            }
        }

        private void Update_TryUseNewFog()
        {
            if (this.newFog != null)
            {
                lock (newFogLock)
                {
                    this.newFog.SaveAsPng(new System.IO.FileStream("newfog.png", System.IO.FileMode.Create), newFog.Width, newFog.Height);
                    if (this.fog != null)
                        this.fog.Dispose();
                    this.fog = this.newFog;
                    this.newFog = null;
                }
            }
        }

        private void Update_HandleScroll()
        {
            // TODO: Add support for scrolling off screen, so we don't know when the map actually ends. Cap it at Window.Width/Height offscreen though - no reason to know exactly where it ends.
            // Control forces a Zoom, so overrides all Scrolling.
            if (this.map != null && !currentKeyboardState.IsKeyDown(Keys.LeftControl))
            {
                Update_HandleVerticalScroll();
                Update_HandleHorizontalScroll();
            }
        }

        private void Update_HandleVerticalScroll()
        {
            var wheelDelta = (currentMouseState.ScrollWheelValue - lastWheelValue);
            if (!currentKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                if (wheelDelta > 0)
                {
                    // Up
                    verticalScrollPosition = Math.Max(0, verticalScrollPosition - (int)Math.Abs(map.Height * scrollDeltaPercent));
                }
                else if (wheelDelta < 0)
                {
                    // Down
                    verticalScrollPosition = Math.Min(verticalScrollPosition + (int)Math.Abs(map.Height * scrollDeltaPercent), LogicalMapHeight - ActualClientHeight);
                }
            }

        }

        private void Update_HandleHorizontalScroll()
        {
            var wheelDelta = (currentMouseState.ScrollWheelValue - lastWheelValue);
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                if (wheelDelta > 0)
                {
                    // Left
                    horizontalScrollPosition = Math.Max(0, horizontalScrollPosition - (int)Math.Abs(map.Width * scrollDeltaPercent));
                }
                else if (wheelDelta < 0)
                {
                    // Right
                    horizontalScrollPosition = Math.Min(horizontalScrollPosition + (int)Math.Abs(map.Width * scrollDeltaPercent), LogicalMapWidth - ActualClientWidth);
                }
            }

        }

        private void Update_HandleZoom()
        {
            if (currentKeyboardState.IsKeyDown(Keys.LeftControl))
            {
                var wheelDelta = (currentMouseState.ScrollWheelValue - lastWheelValue);

                if (wheelDelta > 0)
                {
                    // In
                    zoomFactor = Math.Min((float)Math.Round(zoomFactor + zoomFactorDelta, 1), zoomMaximumFactor);
                }
                else if (wheelDelta < 0)
                {
                    // Out
                    zoomFactor = Math.Max((float)Math.Round(zoomFactor - zoomFactorDelta, 1), zoomMinimumFactor);
                }
                else
                {
                    return;
                }

                // After any zoom, we need to re-bound the Scroll Positions so we're not over-showing the map.
                horizontalScrollPosition = Math.Max(0, Math.Min(horizontalScrollPosition, LogicalMapWidth - ActualClientWidth));
                verticalScrollPosition = Math.Max(0, Math.Min(verticalScrollPosition, LogicalMapHeight - ActualClientHeight));
            }
        }

        public struct VertexPositionNormalColor : IVertexType
        {
            public VertexPositionNormalColor(Vector3 pos, Color c)
            {
                Position = pos;
                Color = new Vector4(c.A, c.R, c.G, c.B);
                Normal = Vector3.One;
            }

            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Color;

            public VertexDeclaration VertexDeclaration
            {
                get
                {
                    return new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                                                 new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                                                 new VertexElement(24, VertexElementFormat.Vector4, VertexElementUsage.Color, 0));
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // If we have any Fog Updates, we need to now render them onto the Fog texture.
            if (fogUpdates.Count > 0)
            {
                FogUpdate[] newFogUpdates;
                lock (fogUpdatesLock)
                {
                    newFogUpdates = fogUpdates.ToArray();
                    fogUpdates.Clear();
                }
                var pp = GraphicsDevice.PresentationParameters;
                var renderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, true, GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
                GraphicsDevice.SetRenderTarget(renderTarget);
                GraphicsDevice.Clear(Color.Black);
                foreach (var newFogUpdate in newFogUpdates)
                {
                    var points = newFogUpdate.Points;
                    var firstPoint = points[0];

                    var vertices = new List<VertexPositionColor>()
                    {
                        new VertexPositionColor(new Vector3(firstPoint.X, firstPoint.Y, 0), Color.Red),
                    };
                    
                    
                    // [0] is [0]
                    // [1] is [1]
                    // [2] is [2]
                    // [3] is [0]
                    // [4] is [3]
                    // [5] is [4]
                    // [6] is [0]
                    // [7] is [5]
                    // [8] is [6]
                    // ...
                    for (var i = 1; i < points.Length - 1; i++)
                    {
                        vertices.Add(new VertexPositionColor(new Vector3(points[0].X, points[0].Y, 0), Color.Red));
                        vertices.Add(new VertexPositionColor(new Vector3(points[i].X, points[i].Y, 0), Color.Red));
                        vertices.Add(new VertexPositionColor(new Vector3(points[i + 1].X, points[i + 1].Y, 0), Color.Red));
                    }
                    var basicEffect = new BasicEffect(graphics.GraphicsDevice);
                    basicEffect.VertexColorEnabled = true;
                    basicEffect.Projection = Matrix.CreateOrthographicOffCenter
                       (0, graphics.GraphicsDevice.Viewport.Width,     // left, right
                        graphics.GraphicsDevice.Viewport.Height, 0,    // bottom, top
                        0, 1);                                         // near, far plane

                    VertexBuffer vb = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
                    vb.SetData<VertexPositionColor>(vertices.ToArray());
                    GraphicsDevice.SetVertexBuffer(vb);
                    RasterizerState rs = new RasterizerState();
                    rs.CullMode = CullMode.None;
                    GraphicsDevice.RasterizerState = rs;
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Count / 3);

                    //BasicEffect effect = new BasicEffect(GraphicsDevice);
                    //effect.VertexColorEnabled = true;
                    //effect.TextureEnabled = false;
                    //effect.LightingEnabled = false;
                    //GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3, VertexPositionColor.VertexDeclaration);

                }
                GraphicsDevice.SetRenderTarget(null);
                lock (newFogLock)
                {
                    if (newFog != null)
                        newFog.Dispose();
                    newFog = renderTarget;
                }
            }

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            try
            {
                if (isServerNotFound)
                {
                    Draw_ServerNotFound();
                }
                else if (isConnectionClosed)
                {
                    Draw_Exit();
                }
                else if (!this.isConnected)
                {
                    if (isConnecting)
                        Draw_Connecting();
                    else
                        Draw_NotConnected();
                }
                else if (isConnectionClosed)
                {
                    Draw_Exit();
                }

                else if (isBlackoutOn)
                {
                    Draw_Blackout(gameTime);
                }
                else if (map == null)
                {
                    Draw_NoMap(gameTime);
                }
                else 
                {
                    spriteBatch.Draw(map, new Vector2(-horizontalScrollPosition, -verticalScrollPosition), null, Color.White, 0f, Vector2.Zero, zoomFactor, SpriteEffects.None, 0);

                    if (gridSize.HasValue)
                    {
                        // TODO: We can change the math to only draw what's visible, if necessary.
                        var gridSizeStep = (int)(gridSize.Value * zoomFactor);
                        for (var x = -horizontalScrollPosition; x < ActualMapWidth; x += gridSizeStep)
                        {
                            spriteBatch.Draw(gridTileImage, new Rectangle(x, 0, 1, ActualClientHeight + verticalScrollPosition), gridTileColor);
                        }
                        for (var y = -verticalScrollPosition; y < ActualMapHeight + verticalScrollPosition; y += gridSizeStep)
                        {
                            spriteBatch.Draw(gridTileImage, new Rectangle(0, y, ActualClientWidth + horizontalScrollPosition, 1), gridTileColor);
                        }
                    }

                    // TODO: Draw fog overtop
                }

                spriteBatch.DrawString(debugFont, FullDebugText, Vector2.Zero, Color.Aqua);
            }
            finally
            {
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void Draw_NotConnected()
        {
            DrawCenteredMessage("Not connected");
        }

        private void Draw_ServerNotFound()
        {
            DrawCenteredMessage(string.Format("Server at {0}:{1} could not be found", connection.Address, connection.Port));
        }

        private void Draw_Connecting()
        {
            if (connection != null)
                DrawCenteredMessage(string.Format("Connecting to {0}:{1}...", connection.Address, connection.Port));
        }

        private void Draw_Exit()
        {
            DrawCenteredMessage("Server has closed the connection");
        }

        private void Draw_Blackout(GameTime gameTime)
        {
            var color = (gameTime.TotalGameTime.Seconds % 2 == 0) ? Color.White : Color.Wheat;
            spriteBatch.Draw(blackoutImage, new Vector2(this.ActualClientWidth / 2 - blackoutImage.Width / 2, this.ActualClientHeight / 2 - blackoutImage.Height / 2), color);
        }

        private void Draw_NoMap(GameTime gameTime)
        {
            var color = (gameTime.TotalGameTime.Seconds % 2 == 0) ? Color.White : Color.Wheat;
            spriteBatch.Draw(noMapImage, new Vector2(this.ActualClientWidth / 2 - noMapImage.Width / 2, this.ActualClientHeight / 2 - noMapImage.Height / 2), color);
        }

        private void DrawCenteredMessage(string msg)
        {
            var msgSize = genericMessageFont.MeasureString(msg);
            spriteBatch.DrawString(genericMessageFont, msg, new Vector2((int)((this.ActualClientWidth / 2) - (msgSize.X / 2)), (int)((this.ActualClientHeight / 2) - (msgSize.Y / 2))), Color.Aqua);
        }
    }
}
