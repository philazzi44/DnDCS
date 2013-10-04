using System;
using System.Collections.Generic;
using System.IO;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DnDCS_Client.GameLogic
{
    public partial class Game : Microsoft.Xna.Framework.Game
    {
        private readonly GameState gameState = new GameState();
        private readonly GraphicsDeviceManager graphics;
        private readonly List<string> debugText = new List<string>();

        private BasicEffect effect;

        private SpriteBatch spriteBatch;
        private Texture2D map;
        // Used as a placeholder for a new image coming in, so the next Update cycle will switch the image.
        private readonly object newMapLock = new object();
        private Texture2D newMap;
        private Texture2D fog;
        // Used as a placeholder for a new image coming in, so the next Update cycle will switch the image.
        private readonly object newFogLock = new object();
        private Texture2D newFog;

        private Nullable<int> gridSize;
        private Color gridTileColor;

        // TODO: Should be prompted.
        private string address = "pazzi.parse3.local";
        //private string address = "desktop-win7";
        // TODO: Should be prompted.
        private int port = 11000;

        private int lastWheelValue;

        private int verticalScrollPosition;
        private int horizontalScrollPosition;

        private float zoomFactor = 1.0f;

        private int ActualMapWidth { get { return this.map.Width; } }
        private int ActualMapHeight { get { return this.map.Height; } }
        private int LogicalMapWidth { get { return (int)(ActualMapWidth * zoomFactor); } }
        private int LogicalMapHeight { get { return (int)(ActualMapHeight * zoomFactor); } }

        private int ActualClientWidth { get { return this.Window.ClientBounds.Width; } }
        private int ActualClientHeight { get { return this.Window.ClientBounds.Height; } }
        private int LogicalClientWidth { get { return (int)(ActualClientWidth * zoomFactor); } }
        private int LogicalClientHeight { get { return (int)(ActualClientHeight * zoomFactor); } }

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

            gameState.Connection = new ClientSocketConnection(address, port);
            gameState.Connection.OnConnectionEstablished += new Action(connection_OnConnectionEstablished);
            gameState.Connection.OnServerNotFound += new Action(connection_OnServerNotFound);
            gameState.Connection.OnMapReceived += new Action<SimpleImage>(connection_OnMapReceived);
            gameState.Connection.OnFogReceived += new Action<SimpleImage>(connection_OnFogReceived);
            gameState.Connection.OnFogUpdateReceived += new Action<FogUpdate>(connection_OnFogUpdateReceived);
            gameState.Connection.OnGridSizeReceived += new Action<bool, int>(connection_OnGridSizeReceived);
            gameState.Connection.OnGridColorReceived += new Action<SimpleColor>(connection_OnGridColorReceived);
            gameState.Connection.OnBlackoutReceived += new Action<bool>(connection_OnBlackoutReceived);
            gameState.Connection.OnExitReceived += new Action(connection_OnExitReceived);

            gameState.IsConnecting = true;
            gameState.Connection.Start();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            var aspect = (float)Window.ClientBounds.Width / (float)Window.ClientBounds.Height;
            effect = new BasicEffect(GraphicsDevice)
                         {
                             World = Matrix.Identity,
                             View = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up),
                             Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 1, 100),
                             VertexColorEnabled = true
                         };

            GameConstants.BlackoutImage = this.Content.Load<Texture2D>("BlackoutImage");
            GameConstants.NoMapImage = this.Content.Load<Texture2D>("NoMapImage");

            GameConstants.DebugFont = this.Content.Load<SpriteFont>("Debug");
            GameConstants.GenericMessageFont = this.Content.Load<SpriteFont>("GenericMessage");
            GameConstants.GridTileImage = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            GameConstants.GridTileImage.SetData<Color>(new[] { Color.White });
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
            if (GameConstants.BlackoutImage != null)
                GameConstants.BlackoutImage.Dispose();
            if (GameConstants.NoMapImage != null)
                GameConstants.NoMapImage.Dispose();
            if (this.gameState.Connection != null)
                this.gameState.Connection.Stop();
        }
    }
}
