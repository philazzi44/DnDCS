using DnDCS.Libs;
using DnDCS_Client.ClientLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS_Client
{
    public partial class Game : Microsoft.Xna.Framework.Game
    {
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

            this.Components.Add(new Client());

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
    }
}
