using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS.XNA.Libs.Shared
{
    public static class Debug
    {
        public static SpriteFont Font { get; set; }

        private static readonly IList<string> debugText = new List<string>();
        public static string FullDebugText { get { return string.Join("\n", debugText); } }

        public static void Clear()
        {
            if (XNAConfigValues.ShowDebug)
            {
                debugText.Clear();
            }
        }

        public static void Add(string msg)
        {
            if (XNAConfigValues.ShowDebug)
            {
                lock (debugText)
                {
                    debugText.Add(msg);
                }
            }
        }
    }
}
