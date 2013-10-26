using System;
using DnDCS.XNA.Libs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS.XNA.Client
{
    public partial class ClientComponent
    {
        private void Draw_Init()
        {
            GraphicsDevice.Clear(Color.Black);
            if (effect != null)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    pass.Apply();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Draw_Init();

            var spriteBatch = SharedResources.SpriteBatch;

            spriteBatch.Begin();
            try
            {
                if (gameState.IsServerNotFound)
                {
                    Draw_ServerNotFound();
                }
                else if (gameState.IsConnectionClosed)
                {
                    Draw_Exit();
                }
                else if (!this.gameState.IsConnected)
                {
                    if (gameState.IsConnecting)
                        Draw_Connecting();
                    else
                        Draw_NotConnected();
                }
                else if (gameState.IsConnectionClosed)
                {
                    Draw_Exit();
                }

                else if (gameState.IsBlackoutOn)
                {
                    Draw_Blackout(gameTime);
                }
                else if (gameState.Map == null)
                {
                    Draw_NoMap(gameTime);
                }
                else
                {
                    spriteBatch.Draw(gameState.Map, new Vector2(-gameState.HorizontalScrollPosition, -gameState.VerticalScrollPosition), null, Color.White, 0f, Vector2.Zero, gameState.ZoomFactor, SpriteEffects.None, 0);

                    if (gridSize.HasValue)
                    {
                        var gridSizeStep = (int)(gridSize.Value * gameState.ZoomFactor);
                        for (var x = -gameState.HorizontalScrollPosition; x < gameState.LogicalMapWidth; x += gridSizeStep)
                        {
                            spriteBatch.Draw(ClientConstants.GridTileImage, new Rectangle(x, 0, 1, Math.Min(gameState.LogicalMapHeight, gameState.ActualClientHeight + gameState.VerticalScrollPosition)), gridTileColor);
                        }
                        for (var y = -gameState.VerticalScrollPosition; y < gameState.LogicalMapHeight; y += gridSizeStep)
                        {
                            spriteBatch.Draw(ClientConstants.GridTileImage, new Rectangle(0, y, Math.Min(gameState.LogicalMapWidth, gameState.ActualClientWidth + gameState.HorizontalScrollPosition), 1), gridTileColor);
                        }
                    }

                    spriteBatch.Draw(gameState.Fog, new Vector2(-gameState.HorizontalScrollPosition, -gameState.VerticalScrollPosition), null, Color.White, 0f, Vector2.Zero, gameState.ZoomFactor, SpriteEffects.None, 0);
                }
            }
            finally
            {
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
        
        private void Draw_NotConnected()
        {
            DrawCenteredMessage("Not connected");
        }

        private void Draw_ServerNotFound()
        {
            if (gameState.Connection != null)
                DrawCenteredMessage(string.Format("Server at {0}:{1} could not be found", gameState.Connection.Address, gameState.Connection.Port));
        }

        private void Draw_Connecting()
        {
            if (gameState.Connection != null)
                DrawCenteredMessage(string.Format("Connecting to {0}:{1}...", gameState.Connection.Address, gameState.Connection.Port));
        }

        private void Draw_Exit()
        {
            DrawCenteredMessage("Server has closed the connection");
        }

        private void Draw_Blackout(GameTime gameTime)
        {
            var color = (gameTime.TotalGameTime.Seconds % 2 == 0) ? Color.White : Color.Wheat;
            SharedResources.SpriteBatch.Draw(ClientConstants.BlackoutImage, new Vector2(gameState.ActualClientWidth / 2 - ClientConstants.BlackoutImage.Width / 2, gameState.ActualClientHeight / 2 - ClientConstants.BlackoutImage.Height / 2), color);
        }

        private void Draw_NoMap(GameTime gameTime)
        {
            var color = (gameTime.TotalGameTime.Seconds % 2 == 0) ? Color.White : Color.Wheat;
            SharedResources.SpriteBatch.Draw(ClientConstants.NoMapImage, new Vector2(gameState.ActualClientWidth / 2 - ClientConstants.NoMapImage.Width / 2, gameState.ActualClientHeight / 2 - ClientConstants.NoMapImage.Height / 2), color);
        }

        private void DrawCenteredMessage(string msg)
        {
            var msgSize = ClientConstants.GenericMessageFont.MeasureString(msg);
            SharedResources.SpriteBatch.DrawString(ClientConstants.GenericMessageFont, msg, new Vector2((int)((gameState.ActualClientWidth / 2) - (msgSize.X / 2)), (int)((gameState.ActualClientHeight / 2) - (msgSize.Y / 2))), Color.Aqua);
        }
    }
}
