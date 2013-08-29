using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DnDCS.Libs.Assets
{
    public static class AssetsLoader
    {
        private static readonly IDictionary<string, object> assets = new Dictionary<string, object>();

        public static Icon LauncherIcon
        {
            get
            {
                const string name = "Assets/LauncherIcon.ico";
                if (assets.ContainsKey(name))
                    return (Icon)assets[name];
                var icon = Icon.ExtractAssociatedIcon(name);
                assets.Add(name, icon);
                return icon;
            }
        }

        public static Icon ClientIcon
        {
            get
            {
                const string name = "Assets/ClientIcon.ico";
                if (assets.ContainsKey(name))
                    return (Icon)assets[name];
                var icon = Icon.ExtractAssociatedIcon(name);
                assets.Add(name, icon);
                return icon;
            }
        }

        public static Icon ServerIcon
        {
            get
            {
                const string name = "Assets/ServerIcon.ico";
                if (assets.ContainsKey(name))
                    return (Icon)assets[name];
                var icon = Icon.ExtractAssociatedIcon(name);
                assets.Add(name, icon);
                return icon;
            }
        }

        public static Image BlackoutImage
        {
            get
            {
                const string name = "Assets/BlackoutImage.ico";
                if (assets.ContainsKey(name))
                    return (Image)assets[name];
                var image = Image.FromFile(name);
                assets.Add(name, image);
                return image;
            }
        }
    }
}
