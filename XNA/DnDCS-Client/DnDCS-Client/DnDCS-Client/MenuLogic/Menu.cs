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

        private TranslationAnimation menuSelectorTranslationAnimation;

        private FrameAnimation<Texture2D> currentMenuSelectorFrameAnimation;
        private FrameAnimation<Texture2D> menuSelectorIdleFrameAnimation;
        private FrameAnimation<Texture2D> menuSelectorEnterFrameAnimation;

        private FrameAnimation<Texture2D> menuEnterFrameAnimation;
        private TranslationAnimation menuEnterTranslationAnimation;

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

            MenuConstants.MenuSelectorImages = SharedResources.ContentManager.LoadMany<Texture2D>(@"Menu Selector\SelectorIdle{0}", 1, 3);
            MenuConstants.MenuSelectorEnterImages = SharedResources.ContentManager.LoadMany<Texture2D>(@"Menu Selector\SelectorEnter{0}", 1, 2);
            MenuConstants.MenuEnterImages = SharedResources.ContentManager.LoadMany<Texture2D>(@"Menu Selector\Enter{0}", 1, 7);

            // This is a standing icon that blinks
            this.menuSelectorIdleFrameAnimation = new FrameAnimation<Texture2D>(MenuConstants.MenuSelectorImages,
                                                                                new Tuple<float, int>[]
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
            this.menuSelectorIdleFrameAnimation.SetRepeat(0.1f);


            // This is a Shooting animation that the Selector does when Enter is clicked. After it completes, the selector reverts back to the Idle Animation until we transition.
            this.menuSelectorEnterFrameAnimation = new FrameAnimation<Texture2D>(MenuConstants.MenuSelectorEnterImages,
                                                                                 new Tuple<float, int>[]
                                                                                     {
                                                                                         new Tuple<float, int>(0.0f, 0),
                                                                                         new Tuple<float, int>(0.05f, 1),
                                                                                         new Tuple<float, int>(0.8f, 1),
                                                                                     });
            this.menuSelectorEnterFrameAnimation.OnComplete = () =>
            {
                this.currentMenuSelectorFrameAnimation = this.menuSelectorIdleFrameAnimation;
                this.menuSelectorEnterFrameAnimation.Reset();
            };

            // This is a bullet animation that both animates in place, with a related translation to have it move over the chosen menu item. The translation
            // is created only when required, as it depends on the location.
            this.menuEnterFrameAnimation = new FrameAnimation<Texture2D>(MenuConstants.MenuEnterImages,
                                                                         new Tuple<float, int>[]
                                                                             {
                                                                                 new Tuple<float, int>(0.0f, 0),
                                                                                 new Tuple<float, int>(0.15f, 1),
                                                                                 new Tuple<float, int>(0.3f, 2),
                                                                                 new Tuple<float, int>(0.45f, 3),
                                                                                 new Tuple<float, int>(0.55f, 4),
                                                                                 new Tuple<float, int>(0.65f, 5),
                                                                                 new Tuple<float, int>(0.75f, 6),
                                                                             });
            this.menuEnterFrameAnimation.SetRepeat(0.1f, 3);


            this.currentMenuSelectorFrameAnimation = menuSelectorIdleFrameAnimation;
        }

        public override void Update(GameTime gameTime)
        {
            this.currentMenuSelectorFrameAnimation.Update(gameTime, true);

            Update_Keyboard(gameTime);

            base.Update(gameTime);
        }

        private void Update_Keyboard(GameTime gameTime)
        {
            // If we're currently translating the menu selector, then ignore any keyboard events.
            if (menuSelectorTranslationAnimation != null && !menuSelectorTranslationAnimation.IsComplete)
            {
                menuSelectorTranslationAnimation.Update(gameTime);
                return;
            }
            
            // If we're currently doing the Enter Animation, then any further keyboard events are not relevant because after it completes, we'll be transitioning the menu.
            if (menuEnterFrameAnimation != null && menuEnterTranslationAnimation != null)
            {
                menuEnterFrameAnimation.Update(gameTime, true);
                menuEnterTranslationAnimation.Update(gameTime);
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
                        DoEnter(gameTime, () =>
                                              {
                                                  // TODO: Prompt for connection information
                                                  if (OnConnect != null)
                                                  {
                                                      var address = "desktop-win7";
                                                      var port = 11000;

                                                      OnConnect(new SimpleServerAddress() { Address = address, Port = port });
                                                  }
                                              });
                        break;
                    case MenuConstants.MenuOption.Exit:
                        DoEnter(gameTime, TryExit);
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
            menuSelectorTranslationAnimation = new TranslationAnimation(menuStart.X, menuStart.Y, menuEnd.X, menuEnd.Y, 0.0f, MenuConstants.MenuTranslationTotalDuration)
                                          {
                                              OnComplete = () =>
                                              {
                                                  this.selectedMenuOption = newMenuItem;
                                                  this.menuSelectorTranslationAnimation = null;
                                              },
                                          };

            menuSelectorTranslationAnimation.AddVerticalEasing(0.0f, 0.5f, 0.1f, 1.0f);
            menuSelectorTranslationAnimation.AddVerticalEasing(0.5f, 1.0f, 1.0f, 0.1f);
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
            menuSelectorTranslationAnimation = new TranslationAnimation(menuStart.X, menuStart.Y, menuEnd.X, menuEnd.Y, 0.0f, MenuConstants.MenuTranslationTotalDuration)
                                          {
                                              OnComplete = () =>
                                              {
                                                  this.selectedMenuOption = newMenuItem;
                                                  this.menuSelectorTranslationAnimation = null;
                                              }
                                          };

            menuSelectorTranslationAnimation.AddVerticalEasing(0.0f, 0.5f, 0.1f, 1.0f);
            menuSelectorTranslationAnimation.AddVerticalEasing(0.5f, 1.0f, 1.0f, 0.1f);
        }

        private void DoEnter(GameTime gameTime, Action onComplete)
        {
            // No need to start the new Enter Frame Animation as the next Update round will do that. We also don't need to stop the Idle Frame Animation
            // because it's not noticeable enough if it were to have restarted.
            this.currentMenuSelectorFrameAnimation = this.menuSelectorEnterFrameAnimation;

            // When the translation is complete, we'll invoke the underlying action that was supposed to run as a result of the Enter key being pressed.
            // Note that the Frame Animation never ends, so there's no action to take when it completes.
            var menuSelectorEnterPositions = GetMenuSelectorEnterPositions(this.selectedMenuOption);
            this.menuEnterTranslationAnimation = new TranslationAnimation(menuSelectorEnterPositions[0], menuSelectorEnterPositions[1], 2.0f)
                                                     {
                                                         OnComplete = () =>
                                                                          {
                                                                              this.menuEnterTranslationAnimation = null;
                                                                              this.menuEnterFrameAnimation.Stop(true);
                                                                              //onComplete();
                                                                          }
                                                     };

            // Appears to be the same as the 3 breakdown below, but keeping the other one in case it becomes useful one day.
            menuEnterTranslationAnimation.AddHorizontalEasing(0.0f, 0.1f, 0.0015f, 2.0f);
                
            //menuEnterTranslationAnimation.AddHorizontalEasing(0.0f, 0.04f, 0.0015f, 0.9f);
            //menuEnterTranslationAnimation.AddHorizontalEasing(0.04f, 0.05f, 0.9f, 1.5f);
            //menuEnterTranslationAnimation.AddHorizontalEasing(0.05f, 0.1f, 1.5f, 2.0f);
        }

        private void TryExit()
        {
            // TODO: Prompt to exit?
            if (OnExit != null)
                OnExit();
        }

        /// <summary> Top-Left of the Menu Text Entry. </summary>
        private Vector2 GetMenuTextPosition(MenuConstants.MenuOption menuOption)
        {
            return new Vector2(MenuStartX, MenuStartY + (MenuConstants.MenuItemFont.LineSpacing * (int)menuOption));
        }

        /// <summary> Returns the top left of the Frame Animation to allow for the center of the image to be vertically centered with the text, but with some pixels between the right of the image and the left of the text. </summary>
        private Vector2 GetMenuSelectorPosition(MenuConstants.MenuOption menuOption)
        {
            var menuTextPosition = GetMenuTextPosition(menuOption);
            return new Vector2(menuTextPosition.X - this.currentMenuSelectorFrameAnimation.CurrentFrame.Width - 25, menuTextPosition.Y + (MenuConstants.MenuItemFont.LineSpacing / 2) - this.currentMenuSelectorFrameAnimation.CurrentFrame.Height / 2);
        }

        /// <summary> Returns the top left of the Enter Animation to allow for the center of the image to be vertically centered with the Selector's arm cannon, as well as the top left of where it should end. </summary>
        private Vector2[] GetMenuSelectorEnterPositions(MenuConstants.MenuOption menuOption)
        {
            var menuSelectorPosition = GetMenuSelectorPosition(menuOption);
            var menuSelectorEnterPositionStart = new Vector2(menuSelectorPosition.X + this.currentMenuSelectorFrameAnimation.CurrentFrame.Width - 8, menuSelectorPosition.Y);
            // It ends all the way beyond the screen boundaries
            var menuSelectorEnterPositionEnd = new Vector2(SharedResources.GameWindow.ClientBounds.Width, menuSelectorEnterPositionStart.Y);

            return new Vector2[] { menuSelectorEnterPositionStart, menuSelectorEnterPositionEnd };
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
            if (menuSelectorTranslationAnimation == null || menuSelectorTranslationAnimation.IsComplete)
            {
                var menuSelectorPosition = GetMenuSelectorPosition(this.selectedMenuOption);
                spriteBatch.Draw(this.currentMenuSelectorFrameAnimation.CurrentFrame, menuSelectorPosition, Color.White);
            }
            else
            {
                spriteBatch.Draw(this.currentMenuSelectorFrameAnimation.CurrentFrame, new Vector2(menuSelectorTranslationAnimation.CurrentX, menuSelectorTranslationAnimation.CurrentY), Color.White);
            }

            if (menuEnterFrameAnimation != null && menuEnterTranslationAnimation != null && menuEnterTranslationAnimation.IsRunning)
            {
                spriteBatch.Draw(menuEnterFrameAnimation.CurrentFrame, menuEnterTranslationAnimation.Current, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
