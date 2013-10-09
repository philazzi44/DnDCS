using DnDCS.Libs;
using DnDCS_Client.ClientLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DnDCS_Client.MenuLogic;

namespace DnDCS_Client
{
    public partial class Game : Microsoft.Xna.Framework.Game
    {
        private GameComponent activeGameComponent;

        public Game()
        {
            SharedResources.Game = this;
            SharedResources.GraphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024,
                PreferredBackBufferHeight = 768,
            };
            SharedResources.ContentManager = Content;
            SharedResources.GameWindow = this.Window;

            SharedResources.ContentManager.RootDirectory = "Content";
            SharedResources.GameWindow.AllowUserResizing = true;
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
            SharedResources.GraphicsDevice = this.GraphicsDevice;

            ShowMenuComponent();

            base.Initialize();
        }
        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            SharedResources.SpriteBatch = new SpriteBatch(GraphicsDevice);

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        private void ShowMenuComponent()
        {
            var menuComponent = new Menu();
            menuComponent.OnConnect += new System.Action<DnDCS.Libs.PersistenceObjects.ServerAddress>(menu_OnConnect);
            menuComponent.OnExit += new System.Action(menuComponent_OnExit);
            this.SwitchGameComponent(menuComponent);
        }


        private void ShowClientComponent(DnDCS.Libs.PersistenceObjects.ServerAddress serverAddress)
        {
            var clientComponent = new Client(serverAddress.Address, serverAddress.Port);
            clientComponent.OnEscape += new System.Action(clientComponent_OnEscape);
            clientComponent.Initialize();

            this.SwitchGameComponent(clientComponent);
        }

        private void SwitchGameComponent(GameComponent newGameComponent)
        {
            if (this.activeGameComponent != null)
            {
                this.Components.Remove(this.activeGameComponent);
                this.activeGameComponent.Dispose();
                this.activeGameComponent = null;
            }

            this.Components.Add(this.activeGameComponent = newGameComponent);
        }

        private void menu_OnConnect(DnDCS.Libs.PersistenceObjects.ServerAddress serverAddress)
        {
            ShowClientComponent(serverAddress);
        }

        private void menuComponent_OnExit()
        {
            this.Exit();
        }

        private void clientComponent_OnEscape()
        {
            ShowMenuComponent();
        }
    }
}
