﻿using DnDCS.Libs.SimpleObjects;
using DnDCS.XNA.Client;
using DnDCS.XNA.Libs;
using DnDCS.XNA.Libs.Shared;
using DnDCS.XNA.MenuLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS.XNA
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
            menuComponent.OnClient += new System.Action<SimpleServerAddress>(menu_OnClient);
            menuComponent.OnExit += new System.Action(menuComponent_OnExit);
            this.SwitchGameComponent(menuComponent);
        }

        private void ShowServerComponent()
        {
            // TODO: For now, the Server is a Win Forms app that is run in Server mode.
            System.Diagnostics.Process.Start(DnDCS.XNA.Libs.ConfigValues.WinFormsApp, ((int)DnDCS.Libs.Constants.RunMode.Server).ToString());
            this.Exit();
        }

        private void ShowClientComponent(SimpleServerAddress serverAddress)
        {
            // TODO: For now, the Client is a Win Forms app that is run in Client mode.
            System.Diagnostics.Process.Start(DnDCS.XNA.Libs.ConfigValues.WinFormsApp, string.Join(" ", ((int)DnDCS.Libs.Constants.RunMode.Client).ToString(), serverAddress.Address, serverAddress.Port.ToString()));
            this.Exit();
            return;

            var clientComponent = new ClientComponent(serverAddress.Address, serverAddress.Port);
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

        private void menu_OnServer()
        {
            ShowServerComponent();
        }

        private void menu_OnClient(SimpleServerAddress serverAddress)
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

        protected override void Update(GameTime gameTime)
        {
            Debug.Clear();

            // TODO: Add Keyboard state and other global state things to here to be captured.

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
