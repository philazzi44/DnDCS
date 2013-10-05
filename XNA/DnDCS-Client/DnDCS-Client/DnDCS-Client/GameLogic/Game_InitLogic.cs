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
        private readonly GameState gameState;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private BasicEffect effect;

        private Nullable<int> gridSize;
        private Color gridTileColor;

        private readonly object fogUpdatesLock = new object();
        private readonly IList<FogUpdate> fogUpdates = new List<FogUpdate>();

        public Game()
        {
            this.gameState = new GameState(this.Window);

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

            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            gameState.Connection = new ClientSocketConnection(gameState.Address, gameState.Port);
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
            if (this.gameState != null)
                this.gameState.Dispose();
            if (GameConstants.BlackoutImage != null)
                GameConstants.BlackoutImage.Dispose();
            if (GameConstants.NoMapImage != null)
                GameConstants.NoMapImage.Dispose();
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            gameState.CreateEffect = true;
        }

    }
}
