using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS.XNA.Client
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
