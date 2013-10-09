using System;
using System.Collections.Generic;
using DnDCS.Libs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DnDCS_Client.ClientLogic
{
    public class ClientState : IDisposable
    {
        public List<string> DebugText { get; set; }
        public string FullDebugText { get { return string.Join("\n", this.DebugText); } }

        // TODO: Should be prompted.
        //private string address = "pazzi.parse3.local";
        public string Address = "desktop-win7";
        // TODO: Should be prompted.
        public int Port = 11000;

        public bool UpdateTitle { get; set; }
        public bool IsServerNotFound { get; set; }
        public bool IsConnecting { get; set; }
        public bool IsConnected { get; set; }
        public bool IsConnectionClosed { get; set; }
        public bool IsBlackoutOn { get; set; }
        public float ZoomFactor { get; set; }

        public int VerticalScrollPosition { get; set; }
        public int HorizontalScrollPosition { get; set; }

        public KeyboardState CurrentKeyboardState { get; set; }
        public MouseState CurrentMouseState { get; set; }

        public ClientSocketConnection Connection { get; set; }

        // Map is flipped during an Update cycle only.
        private readonly object newMapLock = new object();
        private Texture2D nextFrameMap;
        private Texture2D map;
        public Texture2D Map
        {
            get { return this.map; }
            set
            {
                lock (newMapLock)
                {
                    if (this.nextFrameMap != null)
                        this.nextFrameMap.Dispose();
                    this.nextFrameMap = value;
                }
            }
        }

        public int ActualMapWidth { get { return this.map.Width; } }
        public int ActualMapHeight { get { return this.map.Height; } }
        public int LogicalMapWidth { get { return (int)(ActualMapWidth * this.ZoomFactor); } }
        public int LogicalMapHeight { get { return (int)(ActualMapHeight * this.ZoomFactor); } }

        // Fog is flipped during an Update cycle only.
        private readonly object newFogLock = new object();
        public System.Drawing.Image FogImage { get; set; }
        private Texture2D nextFrameFog;
        private Texture2D fog;
        public Texture2D Fog
        {
            get { return this.fog; }
            set
            {
                lock (newFogLock)
                {
                    if (this.nextFrameFog != null)
                        this.nextFrameFog.Dispose();
                    this.nextFrameFog = value;
                }
            }
        }

        public int ActualClientWidth { get { return SharedResources.GameWindow.ClientBounds.Width; } }
        public int ActualClientHeight { get { return SharedResources.GameWindow.ClientBounds.Height; } }
        public int LogicalClientWidth { get { return (int)(ActualClientWidth * this.ZoomFactor); } }
        public int LogicalClientHeight { get { return (int)(ActualClientHeight * this.ZoomFactor); } }

        /// <summary> If true, a new Basic Effect will be created in the next cycle to ensure what we are showing matches expectations. Defaults to true. </summary>
        public bool CreateEffect { get; set; }

        public bool ConsumeFogUpdates { get; set; }

        public ClientState()
        {
            DebugText = new List<string>();
            ZoomFactor = 1.0f;

            this.CreateEffect = true;
        }

        public void Update()
        {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            this.DebugText.Clear();

            if (this.nextFrameMap != null)
            {
                lock (newMapLock)
                {
                    if (this.map != null)
                        this.map.Dispose();
                    this.map = this.nextFrameMap;
                    this.nextFrameMap = null;
                }
            }

            if (this.nextFrameFog != null)
            {
                lock (newFogLock)
                {
                    if (this.fog != null)
                        this.fog.Dispose();
                    this.fog = this.nextFrameFog;
                    this.nextFrameFog = null;
                }
            }
        }

        public void Dispose()
        {
            if (this.map != null)
                this.map.Dispose();
            if (this.nextFrameMap != null)
                this.nextFrameMap.Dispose();
            if (this.fog != null)
                this.fog.Dispose();
            if (this.FogImage != null)
                this.FogImage.Dispose();
            if (this.nextFrameFog != null)
                this.nextFrameFog.Dispose();
            if (this.Connection != null)
                this.Connection.Stop();
        }
    }
}