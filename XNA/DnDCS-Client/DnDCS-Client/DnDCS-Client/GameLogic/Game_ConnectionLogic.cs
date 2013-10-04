using System;
using System.IO;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS_Client.GameLogic
{
    public partial class Game
    {
        private void connection_OnFogUpdateReceived(FogUpdate fogUpdate)
        {
            lock (fogUpdatesLock)
            {
                fogUpdates.Add(fogUpdate);
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
                lock (newMapLock)
                {
                    if (this.newMap != null)
                    {
                        this.newMap.Dispose();
                        this.newMap = null;
                    }

                    using (var stream = new MemoryStream(mapImage.Bytes))
                    {
                        this.newMap = Texture2D.FromStream(GraphicsDevice, stream);
                    }

                    //lock (newFogLock)
                    //{
                    //    // Since we received a new map, we'll automatically black out everything with fog until the Server tells us otherwise.
                    //    this.newFog = new Texture2D(GraphicsDevice, newMap.Width, newMap.Height);
                    //    this.newFog.SetData<Color>(Enumerable.Repeat(Color.Black, newMap.Width * newMap.Height).ToArray());
                    //}
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Map Received Failure", e);
                if (this.newMap != null)
                    this.newMap.Dispose();
                this.newMap = null;

            }
        }

        private void connection_OnFogReceived(SimpleImage fogImage)
        {
            try
            {
                lock (newFogLock)
                {
                    if (this.newFog != null)
                    {
                        this.newFog.Dispose();
                        this.newFog = null;
                    }

                    using (var stream = new MemoryStream(fogImage.Bytes))
                    {
                        this.newFog = Texture2D.FromStream(GraphicsDevice, stream);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Fog received failure.", e);
                if (this.newFog != null)
                    this.newFog.Dispose();
                this.newFog = null;
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
