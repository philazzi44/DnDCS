using System;
using System.Collections.Generic;
using System.Drawing;

namespace DnDCS.Win.Libs.Assets
{
    public static class AssetsLoader
    {
        private static readonly IDictionary<string, object> assets = new Dictionary<string, object>();

        public static Icon LauncherIcon
        {
            get { return GetResource("Assets/LauncherIcon.ico", Icon.ExtractAssociatedIcon); }
        }

        public static Icon ClientIcon
        {
            get { return GetResource("Assets/ClientIcon.ico", Icon.ExtractAssociatedIcon); }
        }

        public static Icon ServerIcon
        {
            get { return GetResource("Assets/ServerIcon.ico", Icon.ExtractAssociatedIcon); }
        }

        public static Image BlackoutImage
        {
            get { return GetResource("Assets/BlackoutImage.png", Image.FromFile); }
        }

        public static Image CenterMapOverlayIcon
        {
            get { return GetResource("Assets/CenterMapOverlayIcon.png", Image.FromFile); }
        }

        private static T GetResource<T>(string name, Func<string, T> fromNameConverter)
        {
            lock (assets)
            {
                if (assets.ContainsKey(name))
                    return (T)assets[name];
                var resource = fromNameConverter(name);
                assets.Add(name, resource);
                return resource;
            }
        }
    }
}
