using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using DnDCS.Libs.PersistenceObjects;


namespace DnDCS_Client.MenuLogic
{
    public class Menu : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public event Action<ServerAddress> OnConnect;
        public event Action OnExit;

        private MenuConstants.MenuOption selectedMenuOption;

        public Menu() : base(SharedResources.Game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }
        

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            MenuConstants.MenuItemFont = SharedResources.ContentManager.Load<SpriteFont>("MenuItem");
            MenuConstants.MenuSelectorImage = SharedResources.ContentManager.Load<Texture2D>("MenuSelector");
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                this.selectedMenuOption = (MenuConstants.MenuOption)Math.Max((int)this.selectedMenuOption - 1, 0);
            }
            else if (keyboardState.IsKeyDown(Keys.Down))
            {
                this.selectedMenuOption = (MenuConstants.MenuOption)Math.Min((int)this.selectedMenuOption + 1, MenuConstants.MenuOptions.Count - 1);
            }
            else if (keyboardState.IsKeyDown(Keys.Enter))
            {
                switch (selectedMenuOption)
                {
                    case MenuConstants.MenuOption.Connect:
                        // TODO: Prompt for connection information
                        if (OnConnect != null)
                        {
                            var address = "desktop-win7";
                            var port = 11000;

                            OnConnect(new ServerAddress() { Address = address, Port = port});
                        }
                        break;
                    case MenuConstants.MenuOption.Exit:
                        TryExit();
                        break;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Escape))
            {
                TryExit();
            }
            base.Update(gameTime);
        }

        private void TryExit()
        {
            // TODO: Prompt to exit?
            if (OnExit != null)
                OnExit();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var spriteBatch = SharedResources.SpriteBatch;
            spriteBatch.Begin();

            var x = SharedResources.GameWindow.ClientBounds.Width / 6;
            var y = SharedResources.GameWindow.ClientBounds.Height / 6;

            // Draw all menu options
            var drawMenuOptions = (MenuConstants.MenuOption[])Enum.GetValues(typeof(MenuConstants.MenuOption));
            for (var i = 0; i < drawMenuOptions.Length; i++)
            {
                spriteBatch.DrawString(MenuConstants.MenuItemFont, MenuConstants.MenuOptions[drawMenuOptions[i]], new Vector2(x, y + MenuConstants.MenuItemFont.LineSpacing * i), Color.Aqua);
            }

            // Draw the menu selector
            spriteBatch.Draw(MenuConstants.MenuSelectorImage, new Vector2(x - MenuConstants.MenuSelectorImage.Width, y + MenuConstants.MenuItemFont.LineSpacing * (int)selectedMenuOption), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
