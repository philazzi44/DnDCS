using System;
using System.IO;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS.XNA.Client.ClientLogic
{
    public partial class Client
    {
        private void connection_OnFogUpdateReceived(FogUpdate fogUpdate)
        {
            lock (fogUpdatesLock)
            {
                fogUpdates.Add(fogUpdate);
                gameState.ConsumeFogUpdates = true;
            }
        }

        private void connection_OnConnectionEstablished()
        {
            this.gameState.IsConnected = true;
            this.gameState.UpdateTitle = true;
        }

        private void connection_OnServerNotFound()
        {
            this.gameState.IsServerNotFound = true;
        }

        private void connection_OnMapReceived(SimpleImage mapImage)
        {
            try
            {
                using (var stream = new MemoryStream(mapImage.Bytes))
                {
                    gameState.Map = Texture2D.FromStream(GraphicsDevice, stream);
                }

                //lock (newFogLock)
                //{
                //    // Since we received a new map, we'll automatically black out everything with fog until the Server tells us otherwise.
                //    this.newFog = new Texture2D(GraphicsDevice, newMap.Width, newMap.Height);
                //    this.newFog.SetData<Color>(Enumerable.Repeat(Color.Black, newMap.Width * newMap.Height).ToArray());
                //}
            }
            catch (Exception e)
            {
                Logger.LogError("Map Received Failure", e);
            }
        }

        private void connection_OnFogReceived(SimpleImage fogSimpleImage)
        {
            try
            {
                using (var stream = new MemoryStream(fogSimpleImage.Bytes))
                {
                    var fogImage = System.Drawing.Image.FromStream(stream);
                    
                    stream.Position = 0;

                    var fogTexture = Texture2D.FromStream(GraphicsDevice, stream);
                    // TODO: The Bitmap uses White to simulate Transparency. This is stupid but acceptable for now.
                    ReplaceNonBlackWithTransparent(fogTexture);

                    this.gameState.FogImage = fogImage;
                    this.gameState.Fog = fogTexture;
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Fog received failure.", e);
            }
        }

        private void connection_OnGridColorReceived(SimpleColor gridColor)
        {
            gridTileColor = new Color(gridColor.R, gridColor.G, gridColor.B, gridColor.A);
        }

        private void connection_OnGridSizeReceived(bool showGrid, int gridSize)
        {
            this.gridSize = (showGrid) ? gridSize : new Nullable<int>();
        }

        private void connection_OnBlackoutReceived(bool isBlackoutOn)
        {
            this.gameState.IsBlackoutOn = isBlackoutOn;
        }

        private void connection_OnExitReceived()
        {
            gameState.IsConnectionClosed = true;
        }
    }
}
