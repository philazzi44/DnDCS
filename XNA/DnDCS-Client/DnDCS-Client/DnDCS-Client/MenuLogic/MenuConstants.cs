using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace DnDCS_Client.MenuLogic
{
    public static class MenuConstants
    {
        public enum MenuOption
        {
            Connect,
            Exit,
        };

        public static readonly IDictionary<MenuOption, string> MenuOptions = new Dictionary<MenuOption, string>
        {
            { MenuOption.Connect, "Connect" },
            { MenuOption.Exit, "Exit" },
        };

        public static SpriteFont MenuItemFont { get; set; }
        public static Texture2D MenuSelectorImage { get; set; }
    }
}
