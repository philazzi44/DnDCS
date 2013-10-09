using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace DnDCS_Client
{
    public static class SharedResources
    {
        public static Game Game { get; set; }
        public static GameWindow GameWindow { get; set; }
        public static GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public static GraphicsDevice GraphicsDevice { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static ContentManager ContentManager { get; set; }

    }
}
