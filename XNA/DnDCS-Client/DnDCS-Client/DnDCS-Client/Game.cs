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

namespace DnDCS_Client
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D map;
        private const float scrollDeltaPercent = 0.1f;

        private int lastScrollWheelValue;
        private int lastVerticalScrollWheelValue;
        private int lastHorizontalScrollWheelValue;
        private int verticalScrollPosition;
        private int horizontalScrollPosition;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            this.map = this.Content.Load<Texture2D>("fatty");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            this.map.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add support for scrolling off screen, so we don't know when the map actually ends. Cap it at Window.Width/Height offscreen though - no reason to know exactly where it ends.
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();
            if (mouse.ScrollWheelValue != lastScrollWheelValue)
            {
                var isWheelUp = (mouse.ScrollWheelValue - lastScrollWheelValue) > 0;

                if (keyboard.IsKeyDown(Keys.LeftShift))
                {
                    if (isWheelUp)
                    {
                        // Scroll left
                        horizontalScrollPosition = Math.Max(0,
                                                          horizontalScrollPosition
                                                          - (int)Math.Abs(map.Width * scrollDeltaPercent));
                    }
                    else
                    {
                        // Scroll right
                        horizontalScrollPosition =
                            Math.Min(horizontalScrollPosition + (int)Math.Abs(map.Width * scrollDeltaPercent),
                                     map.Width - this.Window.ClientBounds.Width);
                    }

                    // We only increment by the delta, since the Scroll Wheel Value is used for both horizontal and vertical scrolling.
                    lastHorizontalScrollWheelValue += (mouse.ScrollWheelValue - lastScrollWheelValue);
                }
                else
                {
                    if (isWheelUp)
                    {
                        // Scroll up
                        verticalScrollPosition = Math.Max(0,
                                                          verticalScrollPosition
                                                          - (int)Math.Abs(map.Height * scrollDeltaPercent));
                    }
                    else // if (mouse.ScrollWheelValue < lastScrollWheelValue)
                    {
                        // Scroll down
                        verticalScrollPosition =
                            Math.Min(verticalScrollPosition + (int)Math.Abs(map.Height * scrollDeltaPercent),
                                     map.Height - this.Window.ClientBounds.Height);
                    }
                    // We only increment by the delta, since the Scroll Wheel Value is used for both horizontal and vertical scrolling.
                    lastVerticalScrollWheelValue += (mouse.ScrollWheelValue - lastScrollWheelValue);
                }
                lastScrollWheelValue = mouse.ScrollWheelValue;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin();
            spriteBatch.Draw(map, new Vector2(-horizontalScrollPosition, -verticalScrollPosition), Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
