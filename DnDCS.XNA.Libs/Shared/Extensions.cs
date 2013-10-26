using Microsoft.Xna.Framework.Content;

namespace DnDCS.XNA.Libs.Shared
{
    public static class Extensions
    {
        /// <summary> Loads many images, such as an animation. Start and End are inclusive. </summary>
        public static T[] LoadMany<T>(this ContentManager contentManager, string basePathFormat, int startValue, int endValue)
        {
            var loaded = new T[endValue - startValue + 1];
            for (int imageIndex = startValue, arrayIndex = 0; imageIndex <= endValue; imageIndex++, arrayIndex++)
            {
                loaded[arrayIndex] = contentManager.Load<T>(string.Format(basePathFormat, imageIndex));
            }
            return loaded;
        }
    }
}
