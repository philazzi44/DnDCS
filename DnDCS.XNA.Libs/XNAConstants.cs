using Microsoft.Xna.Framework.Graphics;

namespace DnDCS.XNA.Libs
{
    public static class XNAConstants
    {
        public const float ScrollDeltaPercent = 0.1f;

        public const float ZoomFactorDelta = 0.1f;
        public const float ZoomMinimumFactor = 0.2f;
        public const float ZoomMaximumFactor = 5.0f;

        public static SpriteFont GenericMessageFont { get; set; }
        public static Texture2D GridTileImage { get; set; }
        public static Texture2D BlackoutImage { get; set; }
        public static Texture2D NoMapImage { get; set; }
    }
}
