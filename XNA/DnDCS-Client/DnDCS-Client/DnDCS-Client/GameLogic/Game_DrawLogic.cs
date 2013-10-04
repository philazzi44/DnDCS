
using System.Collections.Generic;
using System.Linq;
using DnDCS.Libs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS_Client.GameLogic
{
    public partial class Game
    {
        private string FullDebugText { get { return string.Join("\n", this.debugText); } }

        private void Draw_Init()
        {
            GraphicsDevice.Clear(Color.Black);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                pass.Apply();
        }

        protected override void Draw(GameTime gameTime)
        {
            Draw2(gameTime);
            return;

            Draw_Init();

            if (fogUpdates.Count > 0)
            {
                FogUpdate[] newFogUpdates;
                lock (fogUpdatesLock)
                {
                    newFogUpdates = fogUpdates.ToArray();
                    //fogUpdates.Clear();
                }
                //var pp = GraphicsDevice.PresentationParameters;
                //var renderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, true, GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
                //GraphicsDevice.SetRenderTarget(renderTarget);
                foreach (var newFogUpdate in newFogUpdates)
                {
                    var points = newFogUpdate.Points;

                    // [0] is [0]
                    // [1] is [1]
                    // [2] is [2]
                    // [3] is [0]
                    // [4] is [3]
                    // [5] is [4]
                    // [6] is [0]
                    // ...
                    var vertices = new List<VertexPositionColor>();
                    for (var i = 1; i < points.Length - 1; i++)
                    {
                        // TODO: Normalize
                        vertices.Add(new VertexPositionColor(new Vector3(points[0].X / 1458f, points[0].Y / 1089f, 0), Color.White));
                        vertices.Add(new VertexPositionColor(new Vector3(points[i].X / 1458f, points[i].Y / 1089f, 0), Color.White));
                        vertices.Add(new VertexPositionColor(new Vector3(points[i + 1].X / 1458f, points[i + 1].Y / 1089f, 0), Color.White));
                    }

                    RasterizerState rs = new RasterizerState();
                    rs.CullMode = CullMode.None;
                    GraphicsDevice.RasterizerState = rs;

                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3, VertexPositionColor.VertexDeclaration);

                    //VertexBuffer vb = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
                    //vb.SetData<VertexPositionColor>(vertices.ToArray());
                    //GraphicsDevice.SetVertexBuffer(vb);
                    //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Count / 3);

                }
                base.Draw(gameTime);
                return;
                //GraphicsDevice.SetRenderTarget(null);
                //lock (newFogLock)
                //{
                //    if (newFog != null)
                //        newFog.Dispose();
                //    newFog = renderTarget;
                //}
            }

            {
                var vertices = new VertexPositionColor[3];

                //vertices[0].Position = new Vector3(0f, 0f, 0f);
                //vertices[0].Color = Color.White;
                //vertices[1].Position = new Vector3(1, 1f, 0f);
                //vertices[1].Color = Color.White;
                //vertices[2].Position = new Vector3(0, 1f, 0f);
                //vertices[2].Color = Color.White;

                vertices[0].Position = new Vector3(0.3642578f, 0.0546875f, 0f);
                vertices[0].Color = Color.White;
                vertices[1].Position = new Vector3(0.3642578f, 0.2546875f, 0f);
                vertices[1].Color = Color.White;
                vertices[2].Position = new Vector3(0.4818359f, 0.5625f, 0f);
                vertices[2].Color = Color.White;

                RasterizerState rs = new RasterizerState();
                rs.CullMode = CullMode.None;
                GraphicsDevice.RasterizerState = rs;

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 1, VertexPositionColor.VertexDeclaration);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected void Draw2(GameTime gameTime)
        {
            // If we have any Fog Updates, we need to now render them onto the Fog texture.
            // TODO: This doesn't work right. See the official Draw method for work in progress on this.
            if (false && fogUpdates.Count > 0)
            {
                FogUpdate[] newFogUpdates;
                lock (fogUpdatesLock)
                {
                    newFogUpdates = fogUpdates.ToArray();
                    //fogUpdates.Clear();
                }
                //var pp = GraphicsDevice.PresentationParameters;
                //var renderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, true, GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
                //GraphicsDevice.SetRenderTarget(renderTarget);
                GraphicsDevice.Clear(Color.Black);
                foreach (var newFogUpdate in newFogUpdates)
                {
                    var points = newFogUpdate.Points;
                    var firstPoint = points[0];

                    var vertices = new List<VertexPositionColor>()
                    {
                        new VertexPositionColor(new Vector3(firstPoint.X, firstPoint.Y, 1), Color.Red),
                    };


                    // [0] is [0]
                    // [1] is [1]
                    // [2] is [2]
                    // [3] is [0]
                    // [4] is [3]
                    // [5] is [4]
                    // [6] is [0]
                    // [7] is [5]
                    // [8] is [6]
                    // ...
                    for (var i = 1; i < points.Length - 1; i++)
                    {
                        vertices.Add(new VertexPositionColor(new Vector3(points[0].X, points[0].Y, 1), Color.White));
                        vertices.Add(new VertexPositionColor(new Vector3(points[i].X, points[i].Y, 1), Color.White));
                        vertices.Add(new VertexPositionColor(new Vector3(points[i + 1].X, points[i + 1].Y, 1), Color.White));
                    }
                    var basicEffect = new BasicEffect(graphics.GraphicsDevice);
                    basicEffect.VertexColorEnabled = true;
                    basicEffect.LightingEnabled = false;
                    basicEffect.Projection = Matrix.CreateOrthographicOffCenter
                       (0, graphics.GraphicsDevice.Viewport.Width,     // left, right
                        graphics.GraphicsDevice.Viewport.Height, 0,    // bottom, top
                        0, 1);                                         // near, far plane

                    RasterizerState rs = new RasterizerState();
                    rs.CullMode = CullMode.None;
                    GraphicsDevice.RasterizerState = rs;
                    basicEffect.CurrentTechnique.Passes[0].Apply();

                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3, VertexPositionColor.VertexDeclaration);

                    //VertexBuffer vb = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
                    //vb.SetData<VertexPositionColor>(vertices.ToArray());
                    //GraphicsDevice.SetVertexBuffer(vb);
                    //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Count / 3);

                    base.Draw(gameTime);
                    return;

                }
                //GraphicsDevice.SetRenderTarget(null);
                //lock (newFogLock)
                //{
                //    if (newFog != null)
                //        newFog.Dispose();
                //    newFog = renderTarget;
                //}
            }

            GraphicsDevice.Clear(Color.Black);

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
                else if (map == null)
                {
                    Draw_NoMap(gameTime);
                }
                else
                {
                    spriteBatch.Draw(map, new Vector2(-horizontalScrollPosition, -verticalScrollPosition), null, Color.White, 0f, Vector2.Zero, zoomFactor, SpriteEffects.None, 0);

                    if (gridSize.HasValue)
                    {
                        // TODO: We can change the math to only draw what's visible, if necessary.
                        var gridSizeStep = (int)(gridSize.Value * zoomFactor);
                        for (var x = -horizontalScrollPosition; x < ActualMapWidth; x += gridSizeStep)
                        {
                            spriteBatch.Draw(GameConstants.GridTileImage, new Rectangle(x, 0, 1, ActualClientHeight + verticalScrollPosition), gridTileColor);
                        }
                        for (var y = -verticalScrollPosition; y < ActualMapHeight + verticalScrollPosition; y += gridSizeStep)
                        {
                            spriteBatch.Draw(GameConstants.GridTileImage, new Rectangle(0, y, ActualClientWidth + horizontalScrollPosition, 1), gridTileColor);
                        }
                    }

                    // TODO: Need to apply alpha masking somehow.
                    spriteBatch.Draw(fog, new Vector2(-horizontalScrollPosition, -verticalScrollPosition), null, Color.White, 0f, Vector2.Zero, zoomFactor, SpriteEffects.None, 0);
                }

                spriteBatch.DrawString(GameConstants.DebugFont, FullDebugText, Vector2.Zero, Color.Aqua);
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
            spriteBatch.Draw(GameConstants.BlackoutImage, new Vector2(this.ActualClientWidth / 2 - GameConstants.BlackoutImage.Width / 2, this.ActualClientHeight / 2 - GameConstants.BlackoutImage.Height / 2), color);
        }

        private void Draw_NoMap(GameTime gameTime)
        {
            var color = (gameTime.TotalGameTime.Seconds % 2 == 0) ? Color.White : Color.Wheat;
            spriteBatch.Draw(GameConstants.NoMapImage, new Vector2(this.ActualClientWidth / 2 - GameConstants.NoMapImage.Width / 2, this.ActualClientHeight / 2 - GameConstants.NoMapImage.Height / 2), color);
        }

        private void DrawCenteredMessage(string msg)
        {
            var msgSize = GameConstants.GenericMessageFont.MeasureString(msg);
            spriteBatch.DrawString(GameConstants.GenericMessageFont, msg, new Vector2((int)((this.ActualClientWidth / 2) - (msgSize.X / 2)), (int)((this.ActualClientHeight / 2) - (msgSize.Y / 2))), Color.Aqua);
        }
    }
}
