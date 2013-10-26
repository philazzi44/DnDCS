using System;
using DnDCS.Libs.SimpleObjects;
using DnDCS.XNA.Client.Shared;
using DnDCS.XNA.Client.Shared.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace DnDCS.XNA.Client.MenuLogic
{
    public class Menu : Microsoft.Xna.Framework.DrawableGameComponent
    {
        /// <summary> Event raised when Client is invoked. </summary>
        public event Action<SimpleServerAddress> OnClient;
        /// <summary> Event raised when Server is invoked. </summary>
        public event Action OnServer;
        /// <summary> Event raised when Exit is invoked. </summary>
        public event Action OnExit;

        private MenuConstants.MenuOption selectedMenuOption;

        /// <summary> Intro animation for the selector. </summary>
        private Frame2DTranslationAnimation menuSelectorIntroAnimation;

        /// <summary> Translation animation created when Up or Down are pressed. </summary>
        private TranslationAnimation menuSelectorUpDownTranslationAnimation;
        
        /// <summary> Frame Animation where 'true' is the Enter Frame and 'false' is the Idle Frame. </summary>
        private readonly ConditionalAnimation<Frame2DAnimation> menuSelectorFrameAnimation = new ConditionalAnimation<Frame2DAnimation>();

        /// <summary> Animation when Enter is invoked. </summary>
        private readonly Frame2DTranslationAnimation menuEnterAnimation = new Frame2DTranslationAnimation();

        /// <summary> If true, we should use the Enter animation for the selector instead of the Idle animation. </summary>
        private bool UseSelectorEnterAnimation { get; set; }

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
            MenuConstants.MenuSelectorIntroImages = SharedResources.ContentManager.LoadMany<Texture2D>(@"Menu Selector\SelectorIntro{0}", 1, 7);
            MenuConstants.MenuSelectorIdleImages = SharedResources.ContentManager.LoadMany<Texture2D>(@"Menu Selector\SelectorIdle{0}", 1, 3);
            MenuConstants.MenuSelectorEnterImages = SharedResources.ContentManager.LoadMany<Texture2D>(@"Menu Selector\SelectorEnter{0}", 1, 2);
            MenuConstants.MenuEnterImages = SharedResources.ContentManager.LoadMany<Texture2D>(@"Menu Selector\Enter{0}", 1, 7);

            CreateAnimations();
        }

        private void CreateAnimations()
        {
            CreateIntroAnimations();
            CreateMenuSelectorFrameAnimations();
            CreateEnterFrameAnimation();
        }

        private void CreateIntroAnimations()
        {
            // This is a teleport icon that comes in from the top.
            var menuSelectorIntroFrameAnimation = new Frame2DAnimation(MenuConstants.MenuSelectorIntroImages,
                                                                                 new Tuple<float, int>[]
                                                                                    {
                                                                                        new Tuple<float, int>(0.0f, 0),
                                                                                        new Tuple<float, int>(MenuConstants.IntroTeleportDuration, 1),
                                                                                        new Tuple<float, int>(MenuConstants.IntroTeleportDuration + 0.05f, 2),
                                                                                        new Tuple<float, int>(MenuConstants.IntroTeleportDuration + 0.07f, 3),
                                                                                        new Tuple<float, int>(MenuConstants.IntroTeleportDuration + 0.09f, 4),
                                                                                        new Tuple<float, int>(MenuConstants.IntroTeleportDuration + 0.11f, 5),
                                                                                        new Tuple<float, int>(MenuConstants.IntroTeleportDuration + 0.13f, 6),
                                                                                        new Tuple<float, int>(MenuConstants.IntroTeleportDuration + 0.15f, 7),
                                                                                    });
            menuSelectorIntroFrameAnimation.OnComplete = () =>
            {
                // Go back to the Blinking animation.
                UseSelectorEnterAnimation = false;
                this.menuSelectorFrameAnimation.TrueAnimation.Reset();
                this.menuSelectorIntroAnimation = null;
            };
            menuSelectorIntroFrameAnimation.LogName = "Selector Intro Frame Animation";
            // Teleporting in, translating down to the first menu option (the default selected one). The total duration should match the Frame Animation, which has no easing specified,
            // up to the point of the first image (the teleport image).
            // Note that we offset the Y value so that the translation ends where our first Idle image needs to appear. This is done by simply subtracting the difference of the two frames and
            // centering, where the Intro Image is taller than the Idle Image.
            var menuStart = GetMenuSelectorPosition(this.selectedMenuOption, menuSelectorIntroFrameAnimation);
            var menuSelectorIntroTranslationAnimation = new TranslationAnimation(menuStart.X, -100, menuStart.X, menuStart.Y - (MenuConstants.MenuSelectorIntroImages[0].Height - MenuConstants.MenuSelectorIdleImages[0].Height) / 2, MenuConstants.IntroTeleportDuration);
            menuSelectorIntroTranslationAnimation.LogName = "Selector Intro Translation Animation";
            menuSelectorIntroAnimation = new Frame2DTranslationAnimation(menuSelectorIntroFrameAnimation, menuSelectorIntroTranslationAnimation);

        }

        private void CreateMenuSelectorFrameAnimations()
        {
            // This is a standing icon that blinks
            this.menuSelectorFrameAnimation.FalseAnimation = new Frame2DAnimation(MenuConstants.MenuSelectorIdleImages,
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
            this.menuSelectorFrameAnimation.FalseAnimation.SetRepeat(0.1f);
            this.menuSelectorFrameAnimation.FalseAnimation.LogName = "Selector Idle Frame Animation";

            // This is a Shooting animation that the Selector does when Enter is clicked. After it completes, the selector reverts back to the Idle Animation until we transition.
            this.menuSelectorFrameAnimation.TrueAnimation = new Frame2DAnimation(MenuConstants.MenuSelectorEnterImages,
                                                                                 new Tuple<float, int>[]
                                                                                     {
                                                                                         new Tuple<float, int>(0.0f, 0),
                                                                                         new Tuple<float, int>(0.05f, 1),
                                                                                         new Tuple<float, int>(0.8f, 1),
                                                                                     });
            this.menuSelectorFrameAnimation.TrueAnimation.OnComplete = () =>
            {
                // Go back to the Blinking animation.
                UseSelectorEnterAnimation = false;
                this.menuSelectorFrameAnimation.TrueAnimation.Reset();
            };
            this.menuSelectorFrameAnimation.TrueAnimation.LogName = "Selector Enter Frame Animation";

            this.menuSelectorFrameAnimation.Condition = () => { return this.UseSelectorEnterAnimation && (menuSelectorUpDownTranslationAnimation == null || menuSelectorUpDownTranslationAnimation.IsComplete); };
        }

        private void CreateEnterFrameAnimation()
        {
            // This is a bullet animation that both animates in place, with a related translation to have it move over the chosen menu item. The translation
            // is created only when required, as it depends on the location.
            menuEnterAnimation.Frame = new Frame2DAnimation(MenuConstants.MenuEnterImages,
                                                                         new Tuple<float, int>[]
                                                                             {
                                                                                 new Tuple<float, int>(0.0f, 0),
                                                                                 new Tuple<float, int>(0.1f, 1),
                                                                                 new Tuple<float, int>(0.2f, 2),
                                                                                 new Tuple<float, int>(0.3f, 3),
                                                                                 new Tuple<float, int>(0.4f, 4),
                                                                                 new Tuple<float, int>(0.5f, 5),
                                                                                 new Tuple<float, int>(0.6f, 6),
                                                                             });
            menuEnterAnimation.Frame.SetRepeat(0.1f, 3);
            menuEnterAnimation.Frame.LogName = "Enter Frame Animation";
        }

        public override void Update(GameTime gameTime)
        {
            // Regardless of any input-blocking animations happening, this animation needs to be updated.
            this.menuSelectorFrameAnimation.Animation.Update(gameTime);

            if (this.menuSelectorIntroAnimation != null)
            {
                if (this.menuSelectorIntroAnimation.Translation != null && !this.menuSelectorIntroAnimation.Translation.IsComplete)
                {
                    // We're currently translating the intro menu selector.
                    this.menuSelectorIntroAnimation.Translation.Update(gameTime);
                }
                if (this.menuSelectorIntroAnimation.Frame != null)
                {
                    // The Intro translation is done, but the Intro Frames aren't.
                    this.menuSelectorIntroAnimation.Frame.Update(gameTime);
                }
            }
            else if (menuSelectorUpDownTranslationAnimation != null && !menuSelectorUpDownTranslationAnimation.IsComplete)
            {
                // We're currently translating the menu selector.
                menuSelectorUpDownTranslationAnimation.Update(gameTime);
            }
            else if (menuEnterAnimation.Frame != null && menuEnterAnimation.Translation != null)
            {
                // We're currently doing the Enter Animation.
                menuEnterAnimation.Update(gameTime);
            }
            else
            {
                Update_Keyboard(gameTime);
            }

            base.Update(gameTime);
        }

        private void Update_Keyboard(GameTime gameTime)
        {
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
                    case MenuConstants.MenuOption.Server:
                        DoEnter(gameTime, () =>
                        {
                            if (OnServer != null)
                                OnServer();
                        });
                        break;
                    case MenuConstants.MenuOption.Client:
                        DoEnter(gameTime, () =>
                                              {
                                                  // TODO: Prompt for connection information
                                                  if (OnClient != null)
                                                  {
                                                      var address = "desktop-win7";
                                                      var port = 11000;

                                                      OnClient(new SimpleServerAddress() { Address = address, Port = port });
                                                  }
                                              });
                        break;
                    case MenuConstants.MenuOption.Exit:
                        DoEnter(gameTime, TryRaiseOnExit);
                        break;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Escape))
            {
                TryRaiseOnExit();
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
            menuSelectorUpDownTranslationAnimation = new TranslationAnimation(menuStart.X, menuStart.Y, menuEnd.X, menuEnd.Y, 0.0f, MenuConstants.MenuTranslationTotalDuration)
                                          {
                                              OnComplete = () =>
                                              {
                                                  this.selectedMenuOption = newMenuItem;
                                                  this.menuSelectorUpDownTranslationAnimation = null;
                                              },
                                          };
            menuSelectorUpDownTranslationAnimation.LogName = "Selector Up Translation";

            menuSelectorUpDownTranslationAnimation.AddVerticalEasing(0.0f, 0.5f, 0.1f, 1.0f);
            menuSelectorUpDownTranslationAnimation.AddVerticalEasing(0.5f, 1.0f, 1.0f, 0.1f);
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
            menuSelectorUpDownTranslationAnimation = new TranslationAnimation(menuStart.X, menuStart.Y, menuEnd.X, menuEnd.Y, 0.0f, MenuConstants.MenuTranslationTotalDuration)
                                          {
                                              OnComplete = () =>
                                              {
                                                  this.selectedMenuOption = newMenuItem;
                                                  this.menuSelectorUpDownTranslationAnimation = null;
                                              }
                                          };
            menuSelectorUpDownTranslationAnimation.LogName = "Selector Down Translation";

            menuSelectorUpDownTranslationAnimation.AddVerticalEasing(0.0f, 0.5f, 0.1f, 1.0f);
            menuSelectorUpDownTranslationAnimation.AddVerticalEasing(0.5f, 1.0f, 1.0f, 0.1f);
        }

        private void DoEnter(GameTime gameTime, Action onComplete)
        {
            // No need to start the new Enter Frame Animation as the next Update round will do that. We also don't need to stop the Idle Frame Animation
            // because it's not noticeable enough if it were to have restarted.
            UseSelectorEnterAnimation = true;

            // When the translation is complete, we'll invoke the underlying action that was supposed to run as a result of the Enter key being pressed.
            // Note that the Frame Animation never ends, so there's no action to take when it completes.
            var menuSelectorEnterPositions = GetMenuSelectorEnterPositions(this.selectedMenuOption);
            this.menuEnterAnimation.Translation = new TranslationAnimation(menuSelectorEnterPositions[0], menuSelectorEnterPositions[1], 2.0f)
                                                     {
                                                         OnComplete = () =>
                                                                          {
                                                                              this.menuEnterAnimation.Translation = null;
                                                                              this.menuEnterAnimation.Frame.Stop(true);
                                                                              onComplete();
                                                                          }
                                                     };

            this.menuEnterAnimation.Translation.AddHorizontalEasing(0.0f, 0.03f, 0.0015f, 1.0f);
            this.menuEnterAnimation.Translation.AddHorizontalEasing(0.03f, 0.05f, 1.0f, 1.5f);
            this.menuEnterAnimation.Translation.AddHorizontalEasing(0.05f, 0.1f, 1.5f, 2.0f);
        }
        
        private void TryRaiseOnExit()
        {
            // TODO: Prompt to exit?
            if (OnExit != null)
                OnExit();
        }

        /// <summary> Top-Left of the Menu Text Entry. </summary>
        private Vector2 GetMenuTextPosition(MenuConstants.MenuOption menuOption)
        {
            return new Vector2(MenuConstants.MenuStartX, MenuConstants.MenuStartY + (MenuConstants.MenuItemFont.LineSpacing * (int)menuOption));
        }

        /// <summary>
        ///     Returns the top left of the specified Frame Animation to allow for the center of the image to be vertically centered with the text, but with some pixels between the right of the image and the left of the text.
        ///     If the Frame Animation specified is null, then the Idle or Enter frame will be used based on the UseIdleAnimation and UseEnteranimation flags.
        /// </summary>
        private Vector2 GetMenuSelectorPosition(MenuConstants.MenuOption menuOption, Frame2DAnimation menuSelectorFrameAnimation = null)
        {
            var frame = (menuSelectorFrameAnimation == null) ? this.menuSelectorFrameAnimation.Animation.CurrentFrame : menuSelectorFrameAnimation.CurrentFrame;
            var menuTextPosition = GetMenuTextPosition(menuOption);
            return new Vector2(menuTextPosition.X - frame.Width - 25, menuTextPosition.Y + (MenuConstants.MenuItemFont.LineSpacing / 2) - frame.Height / 2);
        }

        /// <summary> Returns the top left of the Enter Animation to allow for the center of the image to be vertically centered with the Selector's arm cannon, as well as the top left of where it should end. </summary>
        private Vector2[] GetMenuSelectorEnterPositions(MenuConstants.MenuOption menuOption)
        {
            var menuSelectorPosition = GetMenuSelectorPosition(menuOption);
            var menuSelectorEnterPositionStart = new Vector2(menuSelectorPosition.X + this.menuSelectorFrameAnimation.Animation.CurrentFrame.Width - 8, menuSelectorPosition.Y);
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

            if (this.menuSelectorIntroAnimation != null && this.menuSelectorIntroAnimation.Translation != null)
            {
                // Draw the intro selector
                spriteBatch.Draw(this.menuSelectorIntroAnimation.Frame.CurrentFrame, new Vector2(this.menuSelectorIntroAnimation.Translation.CurrentX, this.menuSelectorIntroAnimation.Translation.CurrentY), Color.White);
            }
            else
            {
                // Draw the menu selector, either as a stand-still animation (Idle/Enter animation) or an up/down translation (Idle animation).
                Vector2 menuSelectorPosition;
                if (menuSelectorUpDownTranslationAnimation == null || menuSelectorUpDownTranslationAnimation.IsComplete)
                    menuSelectorPosition = GetMenuSelectorPosition(this.selectedMenuOption);
                else
                    menuSelectorPosition = new Vector2(menuSelectorUpDownTranslationAnimation.CurrentX, menuSelectorUpDownTranslationAnimation.CurrentY);

                spriteBatch.Draw(this.menuSelectorFrameAnimation.Animation.CurrentFrame, menuSelectorPosition, Color.White);

                if (this.menuEnterAnimation.Frame != null && this.menuEnterAnimation.Translation != null && this.menuEnterAnimation.Translation.IsRunning)
                {
                    spriteBatch.Draw(this.menuEnterAnimation.Frame.CurrentFrame, this.menuEnterAnimation.Translation.Current, Color.White);
                }
            }
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
