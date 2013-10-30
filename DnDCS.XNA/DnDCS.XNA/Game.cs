using System.Collections.Generic;
using System.Linq;
using DnDCS.Libs.SimpleObjects;
using DnDCS.XNA.Client;
using DnDCS.XNA.Libs;
using DnDCS.XNA.Libs.Shared;
using DnDCS.XNA.MenuLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DnDCS.XNA.Server;

namespace DnDCS.XNA
{
    public partial class Game : Microsoft.Xna.Framework.Game
    {
        private readonly List<GameComponent> activeGameComponents = new List<GameComponent>();

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

        protected override void Initialize()
        {
            SharedResources.GraphicsDevice = this.GraphicsDevice;

            ShowMenuComponent();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            SharedResources.SpriteBatch = new SpriteBatch(GraphicsDevice);

            Debug.Font = SharedResources.ContentManager.Load<SpriteFont>("Debug");

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
        }

        private void ShowMenuComponent()
        {
            var menuComponent = new Menu();
            menuComponent.OnServer += new System.Action(menu_OnServer);
            menuComponent.OnClientConnect += new System.Action<SimpleServerAddress>(menu_OnClientConnect);
            menuComponent.OnExit += new System.Action(menu_OnExit);
            this.SwitchGameComponent(menuComponent);
        }
        
        private void ShowServerComponent()
        {
            // TODO: For now, the Server is a Win Forms app that is run in Server mode.
            System.Diagnostics.Process.Start(DnDCS.XNA.Libs.XNAConfigValues.WinFormsApp, ((int)DnDCS.Libs.Constants.RunMode.Server).ToString());
            this.Exit();

            var serverComponent = new ServerComponent();
            serverComponent.OnEscape += new System.Action(clientComponent_OnEscape);
            serverComponent.Initialize();

            this.SwitchGameComponent(serverComponent);
        }

        private void ShowClientComponent(SimpleServerAddress serverAddress)
        {
            // TODO: For now, the Client is a Win Forms app that is run in Client mode.
            //System.Diagnostics.Process.Start(DnDCS.XNA.Libs.ConfigValues.WinFormsApp, string.Join(" ", ((int)DnDCS.Libs.Constants.RunMode.Client).ToString(), serverAddress.Address, serverAddress.Port.ToString()));
            //this.Exit();
            //return;

            var clientComponent = new ClientComponent(serverAddress.Address, serverAddress.Port);
            clientComponent.OnEscape += new System.Action(clientComponent_OnEscape);
            clientComponent.Initialize();

            this.SwitchGameComponent(clientComponent);
        }

        private void SwitchGameComponent(params GameComponent[] newGameComponents)
        {
            if (activeGameComponents.Any())
            {
                foreach (var c in activeGameComponents)
                {
                    this.Components.Remove(c);
                    c.Dispose();
                }
                activeGameComponents.Clear();
            }

            foreach (var c in newGameComponents)
            {
                activeGameComponents.Add(c);
                this.Components.Add(c);
            }
        }

        private void menu_OnServer()
        {
            ShowServerComponent();
        }

        private void menu_OnClientConnect(SimpleServerAddress serverAddress)
        {
            ShowClientComponent(serverAddress);
        }

        private void menu_OnExit()
        {
            this.Exit();
        }

        private void menuConnect_OnEnter(SimpleServerAddress serverAddress)
        {
            ShowClientComponent(serverAddress);
        }

        private void menuConnect_OnExit()
        {
            ShowMenuComponent();
        }

        private void clientComponent_OnEscape()
        {
            ShowMenuComponent();
        }

        protected override void Update(GameTime gameTime)
        {
            Debug.Clear();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SharedResources.SpriteBatch.Begin();
            SharedResources.SpriteBatch.DrawString(Debug.Font, Debug.FullDebugText, Vector2.Zero, Color.Red);
            SharedResources.SpriteBatch.End();
        }
    }
}
