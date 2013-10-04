using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DnDCS_Client.GameLogic
{
    public partial class Game
    {
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!this.gameState.IsConnected)
                return;

            debugText.Clear();

            // TODO: This stinks, because we need to check the condition every single update... Have to do this until I figure out how to post to the Game thread.
            if (gameState.UpdateTitle)
            {
                gameState.UpdateTitle = false;
                this.Window.Title = string.Format("DnDCS Client - Connected to {0}:{1}", gameState.Connection.Address, gameState.Connection.Port);
            }

            gameState.CurrentKeyboardState = Keyboard.GetState();
            gameState.CurrentMouseState = Mouse.GetState();

            Update_TryUseNewMap();
            Update_TryUseNewFog();

            if (gameState.CurrentMouseState.ScrollWheelValue != lastWheelValue)
            {
                Update_HandleScroll();
                Update_HandleZoom();
            }

            lastWheelValue = gameState.CurrentMouseState.ScrollWheelValue;

            debugText.Add("Zoom Factor: " + zoomFactor);
            debugText.Add("Vertical Scroll Position: " + verticalScrollPosition);
            debugText.Add("Horizontal Scroll Position: " + horizontalScrollPosition);
            if (map != null)
            {
                debugText.Add("Map Size: " + map.Width + "x" + map.Height);
                debugText.Add("Map Bounds: " + ActualMapWidth + "x" + ActualMapHeight);
                debugText.Add("Logical Map Bounds: " + LogicalMapWidth + "x" + LogicalMapHeight);
            }
            debugText.Add("Client Bounds: " + ActualClientWidth + "x" + ActualClientHeight);
            debugText.Add("Logical Client Bounds: " + LogicalClientWidth + "x" + LogicalClientHeight);
            base.Update(gameTime);
        }

        private void Update_TryUseNewMap()
        {
            if (this.newMap != null)
            {
                lock (newMapLock)
                {
                    if (this.map != null)
                        this.map.Dispose();
                    this.map = this.newMap;
                    this.newMap = null;
                }
            }
        }

        private void Update_TryUseNewFog()
        {
            if (this.newFog != null)
            {
                lock (newFogLock)
                {
                    if (this.fog != null)
                        this.fog.Dispose();
                    this.fog = this.newFog;
                    this.newFog = null;
                }
            }
        }

        private void Update_HandleScroll()
        {
            // TODO: Add support for scrolling off screen, so we don't know when the map actually ends. Cap it at Window.Width/Height offscreen though - no reason to know exactly where it ends.
            // Control forces a Zoom, so overrides all Scrolling.
            if (this.map != null && !gameState.CurrentKeyboardState.IsKeyDown(Keys.LeftControl))
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
                    verticalScrollPosition = Math.Max(0, verticalScrollPosition - (int)Math.Abs(map.Height * GameConstants.ScrollDeltaPercent));
                }
                else if (wheelDelta < 0)
                {
                    // Down
                    verticalScrollPosition = Math.Min(verticalScrollPosition + (int)Math.Abs(map.Height * GameConstants.ScrollDeltaPercent), LogicalMapHeight - ActualClientHeight);
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
                    horizontalScrollPosition = Math.Max(0, horizontalScrollPosition - (int)Math.Abs(map.Width * GameConstants.ScrollDeltaPercent));
                }
                else if (wheelDelta < 0)
                {
                    // Right
                    horizontalScrollPosition = Math.Min(horizontalScrollPosition + (int)Math.Abs(map.Width * GameConstants.ScrollDeltaPercent), LogicalMapWidth - ActualClientWidth);
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
                    zoomFactor = Math.Min((float)Math.Round(zoomFactor + GameConstants.ZoomFactorDelta, 1), GameConstants.ZoomMaximumFactor);
                }
                else if (wheelDelta < 0)
                {
                    // Out
                    zoomFactor = Math.Max((float)Math.Round(zoomFactor - GameConstants.ZoomFactorDelta, 1), GameConstants.ZoomMinimumFactor);
                }
                else
                {
                    return;
                }

                // After any zoom, we need to re-bound the Scroll Positions so we're not over-showing the map.
                horizontalScrollPosition = Math.Max(0, Math.Min(horizontalScrollPosition, LogicalMapWidth - ActualClientWidth));
                verticalScrollPosition = Math.Max(0, Math.Min(verticalScrollPosition, LogicalMapHeight - ActualClientHeight));
            }
        }
    }
}
