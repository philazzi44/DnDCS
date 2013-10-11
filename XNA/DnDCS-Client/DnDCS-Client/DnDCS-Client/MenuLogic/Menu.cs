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
using DnDCS.Libs.SimpleObjects;
using DnDCS_Client.Shared;


namespace DnDCS_Client.MenuLogic
{
    public class Menu : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public event Action<SimpleServerAddress> OnConnect;
        public event Action OnExit;

        private MenuConstants.MenuOption selectedMenuOption;
        private TranslationAnimation menuSelectorTranslation;
        private FrameAnimation<Texture2D> menuSelectorAnimation;
        private int MenuStartX { get { return SharedResources.GameWindow.ClientBounds.Width / 6; } }
        private int MenuStartY { get { return SharedResources.GameWindow.ClientBounds.Height / 6; } }

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
            MenuConstants.MenuSelectorImage = SharedResources.ContentManager.Load<Texture2D>(@"Menu Selector\Icon1");

            MenuConstants.MenuSelectorImages = new Texture2D[]
            {
                SharedResources.ContentManager.Load<Texture2D>(@"Menu Selector\Icon1"),
                SharedResources.ContentManager.Load<Texture2D>(@"Menu Selector\Icon2"),
                SharedResources.ContentManager.Load<Texture2D>(@"Menu Selector\Icon3"),
            };

            menuSelectorAnimation = new FrameAnimation<Texture2D>(MenuConstants.MenuSelectorImages, new Tuple<float, int>[]
            {
                new Tuple<float, int>(0.0f, 0),
                new Tuple<float, int>(3.0f, 1),
                new Tuple<float, int>(3.1f, 2),
                new Tuple<float, int>(3.2f, 0),
                new Tuple<float, int>(5.5f, 1),
                new Tuple<float, int>(5.6f, 2),
                new Tuple<float, int>(5.7f, 0),
                new Tuple<float, int>(5.9f, 1),
                new Tuple<float, int>(6.0f, 2),
            });
            menuSelectorAnimation.SetRepeat(0.1f);
        }

        public override void Update(GameTime gameTime)
        {
            if (!menuSelectorAnimation.IsStarted)
                this.menuSelectorAnimation.Start(gameTime);
            else
                this.menuSelectorAnimation.Update(gameTime);

            Update_Keyboard(gameTime);

            base.Update(gameTime);
        }

        private void Update_Keyboard(GameTime gameTime)
        {
            // If we're currently translating the menu selector, then ignore any keyboard events.
            if (menuSelectorTranslation != null && !menuSelectorTranslation.IsComplete)
            {
                menuSelectorTranslation.Update(gameTime);
                return;
            }

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                SelectUp(gameTime);
            }
            else if (keyboardState.IsKeyDown(Keys.Down))
            {
                SelectDown(gameTime);
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

                            OnConnect(new SimpleServerAddress() { Address = address, Port = port });
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
        }

        private void SelectUp(GameTime gameTime)
        {
            var newMenuItem = (MenuConstants.MenuOption)Math.Max((int)this.selectedMenuOption - 1, 0);

            // If the menu isn't going to change, then do nothing.
            if (newMenuItem == this.selectedMenuOption)
                return;

            var menuStart = GetMenuSelectorPosition(this.selectedMenuOption);
            var menuEnd = GetMenuSelectorPosition(newMenuItem);
            menuSelectorTranslation = new TranslationAnimation(menuStart.X, menuStart.Y, menuEnd.X, menuEnd.Y, 0.0f,
                                                               MenuConstants.MenuTranslationTotalDuration, gameTime)
                                          {
                                              OnComplete = () =>
                                              {
                                                  this.selectedMenuOption = newMenuItem;
                                                  this.menuSelectorTranslation = null;
                                              },
                                          };

            menuSelectorTranslation.AddVerticalEasing(0.0f, 0.5f, 0.1f, 1.0f);
            menuSelectorTranslation.AddVerticalEasing(0.5f, 1.0f, 1.0f, 0.1f);
        }

        private void SelectDown(GameTime gameTime)
        {
            var newMenuItem =
                (MenuConstants.MenuOption)
                Math.Min((int)this.selectedMenuOption + 1, MenuConstants.MenuOptions.Count - 1);

            // If the menu isn't going to change, then do nothing.
            if (newMenuItem == this.selectedMenuOption)
                return;

            var menuStart = GetMenuSelectorPosition(this.selectedMenuOption);
            var menuEnd = GetMenuSelectorPosition(newMenuItem);
            menuSelectorTranslation = new TranslationAnimation(menuStart.X, menuStart.Y, menuEnd.X, menuEnd.Y, 0.0f,
                                                               MenuConstants.MenuTranslationTotalDuration, gameTime)
                                          {
                                              OnComplete = () =>
                                              {
                                                  this.selectedMenuOption = newMenuItem;
                                                  this.menuSelectorTranslation = null;
                                              }
                                          };

            menuSelectorTranslation.AddVerticalEasing(0.0f, 0.5f, 0.1f, 1.0f);
            menuSelectorTranslation.AddVerticalEasing(0.5f, 1.0f, 1.0f, 0.1f);
        }

        private void TryExit()
        {
            // TODO: Prompt to exit?
            if (OnExit != null)
                OnExit();
        }

        private Vector2 GetMenuTextPosition(MenuConstants.MenuOption menuOption)
        {
            return new Vector2(MenuStartX, MenuStartY + (MenuConstants.MenuItemFont.LineSpacing * (int)menuOption));
        }

        private Vector2 GetMenuSelectorPosition(MenuConstants.MenuOption menuOption)
        {
            var menuTextPosition = GetMenuTextPosition(menuOption);
            return new Vector2(menuTextPosition.X - menuSelectorAnimation.CurrentFrame.Width - 15, menuTextPosition.Y + (MenuConstants.MenuItemFont.LineSpacing / 2) - menuSelectorAnimation.CurrentFrame.Height / 2);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var spriteBatch = SharedResources.SpriteBatch;
            spriteBatch.Begin();

            // Draw all menu options
            foreach (var t in (MenuConstants.MenuOption[])Enum.GetValues(typeof(MenuConstants.MenuOption)))
            {
                spriteBatch.DrawString(MenuConstants.MenuItemFont, MenuConstants.MenuOptions[t], GetMenuTextPosition(t), Color.Aqua);
            }

            // Draw the menu selector
            if (menuSelectorTranslation == null || menuSelectorTranslation.IsComplete)
            {
                var menuSelectorPosition = GetMenuSelectorPosition(this.selectedMenuOption);
                spriteBatch.Draw(menuSelectorAnimation.CurrentFrame, menuSelectorPosition, Color.White);
            }
            else
            {
                spriteBatch.Draw(menuSelectorAnimation.CurrentFrame, new Vector2(menuSelectorTranslation.CurrentX, menuSelectorTranslation.CurrentY), Color.White);
            }

            spriteBatch.Draw(menuSelectorAnimation.CurrentFrame, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 5.0f, SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
