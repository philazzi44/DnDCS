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
            Server = 0,
            Client,
            Exit,
        };

        public static readonly IDictionary<MenuOption, string> MenuOptions = new Dictionary<MenuOption, string>
        {
            { MenuOption.Server, "Server" },
            { MenuOption.Client, "Client" },
            { MenuOption.Exit, "Exit" },
        };

        public static SpriteFont MenuItemFont { get; set; }

        public static Texture2D[] MenuSelectorIntroImages { get; set; }
        public static Texture2D[] MenuSelectorIdleImages { get; set; }
        public static Texture2D[] MenuSelectorEnterImages { get; set; }
        public static Texture2D[] MenuEnterImages { get; set; }

        /// <summary> Time, in seconds, for a menu translation to occur. </summary>
        public static readonly float MenuTranslationTotalDuration = 0.2f;

        /// <summary> When the Intro animation is occurring, this is the duration of the actual teleporting coming down from the sky. </summary>
        public const float IntroTeleportDuration = 0.5f;

        public static int MenuStartX { get { return SharedResources.GameWindow.ClientBounds.Width / 4; } }
        public static int MenuStartY { get { return SharedResources.GameWindow.ClientBounds.Height / 4; } }
    }
}
