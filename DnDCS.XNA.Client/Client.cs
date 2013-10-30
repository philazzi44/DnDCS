using System;
using System.Collections.Generic;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DnDCS.XNA.Libs;

namespace DnDCS.XNA.Client
{
    public partial class ClientComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private readonly ClientState gameState;

        private BasicEffect effect;

        private Nullable<int> gridSize;
        private Color gridTileColor;

        private readonly object fogUpdatesLock = new object();
        private readonly IList<FogUpdate> fogUpdates = new List<FogUpdate>();

        public ClientComponent(string address, int port) : base(SharedResources.Game)
        {
            this.gameState = new ClientState(address, port);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            ((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(SharedResources.GameWindow.Handle)).Icon = DnDCS.Win.Libs.Assets.AssetsLoader.ClientIcon;
            ((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(SharedResources.GameWindow.Handle)).Text = "DnDCS - Client";
            Logger.FileSuffix = "Client";

            SharedResources.GameWindow.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

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

        public void Reset()
        {
        }
        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (this.gameState != null)
                this.gameState.Dispose();
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            gameState.CreateEffect = true;
        }

    }
}
