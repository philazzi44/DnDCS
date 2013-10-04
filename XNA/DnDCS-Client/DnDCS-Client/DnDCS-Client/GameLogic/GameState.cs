using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using DnDCS.Libs;

namespace DnDCS_Client.GameLogic
{
    public class GameState
    {
        public bool UpdateTitle { get; set; }
        public bool IsServerNotFound { get; set; }
        public bool IsConnecting { get; set; }
        public bool IsConnected { get; set; }
        public bool IsConnectionClosed { get; set; }
        public bool IsBlackoutOn { get; set; }

        public KeyboardState CurrentKeyboardState { get; set; }
        public MouseState CurrentMouseState { get; set; }

        public ClientSocketConnection Connection { get; set; }

        public void UpdateInputState()
        {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
        }
    }
}
