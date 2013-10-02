using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using DnDCS.Libs;

namespace DnDCS_Client
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private const float scrollDeltaPercent = 0.1f;

        private ClientSocketConnection connection;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D map;
        private Texture2D blackoutImage;

        private Nullable<int> gridSize;

        // TODO: Should be prompted.
        private string address;
        // TODO: Should be prompted.
        private int port;

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

        private bool isConnectionClosed;
        private SpriteFont exitFont;
        private bool isBlackoutOn;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            // connection.OnMapReceived += new Action<Image>(connection_OnMapReceived);
            // connection.OnFogReceived += new Action<Image>(connection_OnFogReceived);
            // connection.OnFogUpdateReceived += new Action<Point[], bool>(connection_OnFogUpdateReceived);
            connection.OnGridSizeReceived += new Action<bool, int>(connection_OnGridSizeReceived);
            // connection.OnGridColorReceived += new Action<Color>(connection_OnGridColorReceived);
            connection.OnBlackoutReceived += new Action<bool>(connection_OnBlackoutReceived);
            connection.OnExitReceived += new Action(connection_OnExitReceived);

            base.Initialize();
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

            this.map = this.Content.Load<Texture2D>("fatty");
            this.blackoutImage = this.Content.Load<Texture2D>("BlackoutImage");
            this.debugFont = this.Content.Load<SpriteFont>("Debug");
            this.exitFont = this.Content.Load<SpriteFont>("Exit");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            this.map.Dispose();
            if (connection != null)
                connection.Stop();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            debugText.Clear();

            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            if (currentMouseState.ScrollWheelValue != lastWheelValue)
            {
                Update_HandleScroll();
                Update_HandleZoom();
            }

            lastWheelValue = currentMouseState.ScrollWheelValue;

            debugText.Add("Zoom Factor: " + zoomFactor);
            debugText.Add("Vertical Scroll Position: " + verticalScrollPosition);
            debugText.Add("Horizontal Scroll Position: " + horizontalScrollPosition);
            debugText.Add("Map Size: " + map.Width + "x" + map.Height);
            debugText.Add("Map Bounds: " + ActualMapWidth + "x" + ActualMapHeight);
            debugText.Add("Logical Map Bounds: " + LogicalMapWidth + "x" + LogicalMapHeight);
            debugText.Add("Client Bounds: " + ActualClientWidth + "x" + ActualClientHeight);
            debugText.Add("Logical Client Bounds: " + LogicalClientWidth + "x" + LogicalClientHeight);
            base.Update(gameTime);
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
                    return;
                }

                if (!isBlackoutOn)
                {
                    Draw_Blackout(gameTime);
                    return;
                }

                spriteBatch.Draw(map, new Vector2(-horizontalScrollPosition, -verticalScrollPosition), null, Color.White, 0f, Vector2.Zero, zoomFactor, SpriteEffects.None, 0);
                if (gridSize.HasValue)
                {
                    for (var x = 0; x < 0; x += gridSize.Value)
                    {
                        // g.DrawLine(gridPen, x, 0, x, receivedMapHeight);
                    }
                    for (var y = 0; y < 0; y += gridSize.Value)
                    {
                        //g.DrawLine(gridPen, 0, y, receivedMapWidth, y);
                    }
                }

                // TODO: Draw fog overtop

                spriteBatch.DrawString(debugFont, FullDebugText, Vector2.Zero, Color.Aqua);
            }
            finally
            {
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void Draw_Exit()
        {
            const string msg = "The server has closed the connection.";
            var msgSize = exitFont.MeasureString(msg);
            spriteBatch.DrawString(exitFont, msg, new Vector2((int)((this.ActualClientWidth / 2) - (msgSize.X / 2)), (int)((this.ActualClientHeight / 2) - (msgSize.Y / 2))), Color.Aqua);
        }

        private void Draw_Blackout(GameTime gameTime)
        {
            var color = (gameTime.TotalGameTime.Seconds % 2 == 0) ? Color.White : Color.Wheat;
            spriteBatch.Draw(blackoutImage, new Vector2(this.ActualClientWidth / 2 - blackoutImage.Width / 2, this.ActualClientHeight / 2 - blackoutImage.Height / 2), color);
        }
    }
}
