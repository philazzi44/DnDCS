using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DnDCS.XNA.Libs
{
    public static class AssetsLoader
    {
        private static readonly IDictionary<string, object> assets = new Dictionary<string, object>();

        public static SpriteFont GenericMessageFont
        {
            get { return GetResource<SpriteFont>("GenericMessage"); }
        }

        public static Texture2D GridTileImage
        {
            get
            {
                return GetResource<Texture2D>("GridTileImage", (name) =>
                {
                    var t = new Texture2D(SharedResources.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                    t.SetData<Color>(new[] { Color.White });
                    return t;
                });
            }
        }

        public static Texture2D BlackoutImage
        {
            get { return GetResource<Texture2D>("BlackoutImage"); }
        }

        public static Texture2D NoMapImage
        {
            get { return GetResource<Texture2D>("NoMapImage"); }
        }

        private static T GetResource<T>(string name)
        {
            return GetResource(name, SharedResources.ContentManager.Load<T>);
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
