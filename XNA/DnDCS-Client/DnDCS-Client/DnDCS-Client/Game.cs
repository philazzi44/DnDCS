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
        private bool isConnecting;
        private bool isConnected;
        private bool isConnectionClosed;

        private SpriteFont genericMessageFont;
        private bool isBlackoutOn;

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

            connection = new ClientSocketConnection(address, port);
            connection.OnConnectionEstablished += new Action(connection_OnConnectionEstablished);
            connection.OnMapReceived += new Action<byte[]>(connection_OnMapReceived);
            connection.OnFogReceived += new Action<byte[]>(connection_OnFogReceived);
            //connection.OnFogUpdateReceived += new Action<SimplePoint[], bool>(connection_OnFogUpdateReceived);
            connection.OnGridSizeReceived += new Action<bool, int>(connection_OnGridSizeReceived);
            connection.OnGridColorReceived += new Action<SimpleColor>(connection_OnGridColorReceived);
            connection.OnBlackoutReceived += new Action<bool>(connection_OnBlackoutReceived);
            connection.OnExitReceived += new Action(connection_OnExitReceived);

            isConnecting = true;
            connection.Start();

            base.Initialize();
        }

        private void connection_OnConnectionEstablished()
        {
            this.isConnected = true;
            this.updateTitle = true;
        }

        private void connection_OnMapReceived(byte[] mapImageBytes)
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
                    this.newMap = new Texture2D(GraphicsDevice, 1024, 768);
                    this.newMap.SetData(mapImageBytes);

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
            }
        }
        
        private void connection_OnFogReceived(byte[] fogImageBytes)
        {
            try
            {
                lock (newFogLock)
                {
                    // TODO: Width/Height needs to come from the arguments, since we can't infer it from the bytes.
                    this.newFog = new Texture2D(GraphicsDevice, 1024, 768);
                    this.newFog.SetData(fogImageBytes);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Fog received failure.", e);
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
            if (!currentKeyboardState.IsKeyDown(Keys.LeftControl))
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

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            try
            {
                if (isConnectionClosed)
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

        private void Draw_Connecting()
        {
            if (connection != null)
                DrawCenteredMessage(string.Format("Connecting to {0}:{1}...", connection.Address, connection.Port));
        }

        private void Draw_Exit()
        {
            DrawCenteredMessage("The server has closed the connection");
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
