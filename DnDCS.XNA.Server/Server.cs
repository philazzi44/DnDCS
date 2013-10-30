using System;
using System.Collections.Generic;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using DnDCS.XNA.Libs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DnDCS.Libs.ServerEvents;

namespace DnDCS.XNA.Server
{
    public partial class ServerComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private readonly ServerState gameState;

        private BasicEffect effect;

        private Nullable<int> gridSize;
        private Color gridTileColor;

        private readonly object fogUpdatesLock = new object();
        private readonly IList<FogUpdate> fogUpdates = new List<FogUpdate>();

        private string initialParentFormText;

        #region Init and Cleanup

        public ServerComponent() : base(SharedResources.Game)
        {
            this.gameState = new ServerState();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            ((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(SharedResources.GameWindow.Handle)).Icon = DnDCS.Win.Libs.Assets.AssetsLoader.ServerIcon;
            ((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(SharedResources.GameWindow.Handle)).Text = initialParentFormText = "DnDCS - Server";
            Logger.FileSuffix = "Server";

            gameState.Connection = new ServerSocketConnection(DnDCS.Libs.ConfigValues.DefaultServerPort);
            gameState.Connection.OnClientConnected += connection_OnClientConnected;
            gameState.Connection.OnClientCountChanged += new Action<int>(connection_OnClientCountChanged);
            gameState.Connection.OnSocketEvent += new Action<ServerEvent>(connection_OnSocketEvent);

            SharedResources.GameWindow.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            
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
        
        #endregion Init and Cleanup

        #region Connection Logic and Callbacks

        private void connection_OnClientConnected()
        {
            if (gameState.Connection.IsStopping)
                return;
            SendAll(true);
        }

        private void connection_OnClientCountChanged(int count)
        {
            if (gameState.Connection.IsStopping)
                return;

            ((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(SharedResources.GameWindow.Handle)).Text = initialParentFormText + string.Format(" ({0} client{1} connected)", count, (count == 1) ? string.Empty : "s");
        }

        private void connection_OnSocketEvent(ServerEvent socketEvent)
        {
        }

        private void SendAll(bool sendBlackout)
        {
        }

        #endregion Connection Logic and Callbacks

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
        }

    }
}
