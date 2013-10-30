using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DnDCS.XNA.Server
{
    public partial class ServerComponent
    {
        public event Action OnEscape;
        
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
        }
    }
}
