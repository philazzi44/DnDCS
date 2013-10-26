using System;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using DnDCS.Libs;
using DnDCS_Client.Shared;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS_Client.ClientLogic
{
    public partial class Client
    {
        public event Action OnEscape;

        private int lastWheelValue;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            gameState.Update();

            if (gameState.CurrentKeyboardState.IsKeyDown(Keys.Escape) && OnEscape != null)
            {
                OnEscape();
                return;
            }

            if (!this.gameState.IsConnected)
                return;

            if (gameState.CreateEffect)
            {
                gameState.CreateEffect = false;

                if (this.effect != null)
                    this.effect.Dispose();
                
                var aspect = (float)this.gameState.ActualClientWidth / (float)this.gameState.ActualClientHeight;
                effect = new BasicEffect(GraphicsDevice)
                {
                    World = Matrix.Identity,
                    View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up),
                    Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 1, 100),
                    VertexColorEnabled = true
                };
            }

            // TODO: This stinks, because we need to check the condition every single update... Have to do this until I figure out how to post to the Game thread.
            if (gameState.UpdateTitle)
            {
                gameState.UpdateTitle = false;
                SharedResources.GameWindow.Title = string.Format("DnDCS Client - Connected to {0}:{1}", gameState.Connection.Address, gameState.Connection.Port);
            }

            // TODO: This is the biggest piece of garbage I've written for this entire thing. Currently disabled because I can't stand it.
            if (gameState.ConsumeFogUpdates)
            {
                gameState.ConsumeFogUpdates = false;
                FogUpdate[] newFogUpdates;
                lock (fogUpdatesLock)
                {
                    newFogUpdates = this.fogUpdates.ToArray();
                    this.fogUpdates.Clear();
                }

                using (var g = System.Drawing.Graphics.FromImage(gameState.FogImage))
                {
                    // Draw all Fog Updates into the Bitmap.
                    foreach (var newFogUpdate in newFogUpdates)
                    {
                        g.FillPolygon((newFogUpdate.IsClearing) ? System.Drawing.Brushes.White : System.Drawing.Brushes.Black, newFogUpdate.Points.Select(p => new System.Drawing.Point(p.X, p.Y)).ToArray());
                    }

                    // Push the Bitmap into a Texture2D instance
                    using (var ms = new System.IO.MemoryStream())
                    {
                        gameState.FogImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        var newFogTexture = Texture2D.FromStream(GraphicsDevice, ms);

                        // TODO: The Bitmap uses White to simulate Transparency. This is stupid but acceptable for now.
                        ReplaceNonBlackWithTransparent(newFogTexture);

                        // Finally push the fog into the next Game State.
                        gameState.Fog = newFogTexture;
                    }
                }
            }

            if (gameState.CurrentMouseState.ScrollWheelValue != lastWheelValue)
            {
                Update_HandleScroll();
                Update_HandleZoom();
            }

            lastWheelValue = gameState.CurrentMouseState.ScrollWheelValue;

            Debug.Add("Zoom Factor: " + gameState.ZoomFactor);
            Debug.Add("Vertical Scroll Position: " + gameState.VerticalScrollPosition);
            Debug.Add("Horizontal Scroll Position: " + gameState.HorizontalScrollPosition);
            if (gameState.Map != null)
            {
                Debug.Add("Map Size: " + gameState.Map.Width + "x" + gameState.Map.Height);
                Debug.Add("Map Bounds: " + gameState.ActualMapWidth + "x" + gameState.ActualMapHeight);
                Debug.Add("Logical Map Bounds: " + gameState.LogicalMapWidth + "x" + gameState.LogicalMapHeight);
            }
            Debug.Add("Client Bounds: " + gameState.ActualClientWidth + "x" + gameState.ActualClientHeight);
            Debug.Add("Logical Client Bounds: " + gameState.LogicalClientWidth + "x" + gameState.LogicalClientHeight);
            base.Update(gameTime);
        }

        /// <summary> Replaces all non-black colors with a Transparent color. This should only be used in the context of Fogs. </summary>
        private void ReplaceNonBlackWithTransparent(Texture2D texture)
        {
            // Replace the old fog with the newly merged fog texture
            Color[] colors = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(colors);
            for (var i = 0; i < colors.Length; i++)
            {
                if (!colors[i].Equals(Color.Black))
                    colors[i] = Color.Transparent;
            }
            texture.SetData<Color>(colors);
        }

        private void Update_HandleScroll()
        {
            // TODO: Add support for scrolling off screen, so we don't know when the map actually ends. Cap it at Window.Width/Height offscreen though - no reason to know exactly where it ends.
            // Control forces a Zoom, so overrides all Scrolling.
            if (this.gameState.Map != null && !gameState.CurrentKeyboardState.IsKeyDown(Keys.LeftControl))
            {
                Update_HandleVerticalScroll();
                Update_HandleHorizontalScroll();
            }
        }

        private void Update_HandleVerticalScroll()
        {
            var wheelDelta = (gameState.CurrentMouseState.ScrollWheelValue - lastWheelValue);
            if (!gameState.CurrentKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                if (wheelDelta > 0)
                {
                    // Up
                    gameState.VerticalScrollPosition = Math.Max(0, gameState.VerticalScrollPosition - (int)Math.Abs(gameState.ActualMapHeight * ClientConstants.ScrollDeltaPercent));
                }
                else if (wheelDelta < 0)
                {
                    // Down
                    gameState.VerticalScrollPosition = Math.Min(gameState.VerticalScrollPosition + (int)Math.Abs(gameState.ActualMapHeight * ClientConstants.ScrollDeltaPercent), gameState.LogicalMapHeight - gameState.ActualClientHeight);
                }
            }

        }

        private void Update_HandleHorizontalScroll()
        {
            var wheelDelta = (gameState.CurrentMouseState.ScrollWheelValue - lastWheelValue);
            if (gameState.CurrentKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                if (wheelDelta > 0)
                {
                    // Left
                    gameState.HorizontalScrollPosition = Math.Max(0, gameState.HorizontalScrollPosition - (int)Math.Abs(gameState.ActualMapWidth * ClientConstants.ScrollDeltaPercent));
                }
                else if (wheelDelta < 0)
                {
                    // Right
                    gameState.HorizontalScrollPosition = Math.Min(gameState.HorizontalScrollPosition + (int)Math.Abs(gameState.ActualMapWidth * ClientConstants.ScrollDeltaPercent), gameState.LogicalMapWidth - gameState.ActualClientWidth);
                }
            }

        }

        private void Update_HandleZoom()
        {
            if (gameState.CurrentKeyboardState.IsKeyDown(Keys.LeftControl))
            {
                var wheelDelta = (gameState.CurrentMouseState.ScrollWheelValue - lastWheelValue);

                if (wheelDelta > 0)
                {
                    // In
                    gameState.ZoomFactor = Math.Min((float)Math.Round(gameState.ZoomFactor + ClientConstants.ZoomFactorDelta, 1), ClientConstants.ZoomMaximumFactor);
                }
                else if (wheelDelta < 0)
                {
                    // Out
                    gameState.ZoomFactor = Math.Max((float)Math.Round(gameState.ZoomFactor - ClientConstants.ZoomFactorDelta, 1), ClientConstants.ZoomMinimumFactor);
                }
                else
                {
                    return;
                }

                // After any zoom, we need to re-bound the Scroll Positions so we're not over-showing the map.
                gameState.HorizontalScrollPosition = Math.Max(0, Math.Min(gameState.HorizontalScrollPosition, gameState.LogicalMapWidth - gameState.ActualClientWidth));
                gameState.VerticalScrollPosition = Math.Max(0, Math.Min(gameState.VerticalScrollPosition, gameState.LogicalMapHeight - gameState.ActualClientHeight));
            }
        }
    }
}
