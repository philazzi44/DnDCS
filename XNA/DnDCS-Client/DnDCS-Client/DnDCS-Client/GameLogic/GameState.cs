using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using DnDCS.Libs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DnDCS_Client.GameLogic
{
    public class GameState : IDisposable
    {
        public GameWindow Window { get; private set; }

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
        private Texture2D newMap;
        private Texture2D map;
        public Texture2D Map
        {
            get { return this.map; }
            set
            {
                lock (newMapLock)
                {
                    if (this.newMap != null)
                        this.newMap.Dispose();
                    this.newMap = value;
                }
            }
        }

       public int ActualMapWidth { get { return this.map.Width; } }
       public int ActualMapHeight { get { return this.map.Height; } }
       public int LogicalMapWidth { get { return (int)(ActualMapWidth * this.ZoomFactor); } }
       public int LogicalMapHeight { get { return (int)(ActualMapHeight * this.ZoomFactor); } }

       // Fog is flipped during an Update cycle only.
       private readonly object newFogLock = new object();
       private Texture2D newFog;
       private Texture2D fog;
       public Texture2D Fog
       {
           get { return this.fog; }
           set
           {
               lock (newFogLock)
               {
                   if (this.newFog != null)
                       this.newFog.Dispose();
                   this.newFog = value;
               }
           }
       }

       public int ActualClientWidth { get { return this.Window.ClientBounds.Width; } }
       public int ActualClientHeight { get { return this.Window.ClientBounds.Height; } }
       public int LogicalClientWidth { get { return (int)(ActualClientWidth * this.ZoomFactor); } }
       public int LogicalClientHeight { get { return (int)(ActualClientHeight * this.ZoomFactor); } }

        public GameState(GameWindow window)
        {
            this.Window = window;
            DebugText = new List<string>();
            ZoomFactor = 1.0f;
        }

        public void Update()
        {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            this.DebugText.Clear();

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

        public void Dispose()
        {
            if (this.map != null)
                this.map.Dispose();
            if (this.newMap != null)
                this.newMap.Dispose();
            if (this.fog != null)
                this.fog.Dispose();
            if (this.newFog != null)
                this.newFog.Dispose();
            if (this.Connection != null)
                this.Connection.Stop();
        }
    }
}
