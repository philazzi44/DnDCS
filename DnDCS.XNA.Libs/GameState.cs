using System;
using Microsoft.Xna.Framework.Input;

namespace DnDCS.XNA.Libs
{
    public abstract class GameState : IDisposable
    {
        public KeyboardState CurrentKeyboardState { get; set; }
        public MouseState CurrentMouseState { get; set; }

        public GameState()
        {
        }

        public virtual void Update()
        {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
        }

        public virtual void Dispose()
        {
        }
    }
}