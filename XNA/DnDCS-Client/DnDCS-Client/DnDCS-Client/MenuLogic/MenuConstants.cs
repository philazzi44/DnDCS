using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS_Client.MenuLogic
{
    public static class MenuConstants
    {
        /// <summary> Position in this enum dictates the position shown on screen. </summary>
        public enum MenuOption
        {
            Connect = 0,
            Exit,
        };

        public static readonly IDictionary<MenuOption, string> MenuOptions = new Dictionary<MenuOption, string>
        {
            { MenuOption.Connect, "Connect" },
            { MenuOption.Exit, "Exit" },
        };

        public static SpriteFont MenuItemFont { get; set; }
        public static Texture2D[] MenuSelectorImages { get; set; }
        public static Texture2D[] MenuSelectorEnterImages { get; set; }
        public static Texture2D[] MenuEnterImages { get; set; }

        /// <summary> Time, in seconds, for a menu translation to occur. </summary>
        public static readonly float MenuTranslationTotalDuration = 0.2f;

    }
}
