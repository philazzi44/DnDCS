using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DnDCS.Libs;
using DnDCS.Libs.PersistenceObjects;
using DnDCS.Libs.SimpleObjects;
using System;

namespace DnDCS.Win.Libs
{
    public static class FormsUtils
    {
        public static byte[] ToBytes(this Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public static Image ToImage(this byte[] dataBytes)
        {
            try
            {
                using (var ms = new MemoryStream(dataBytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to convert byte[] to Image. Bytes length: " + dataBytes.Length, e);
                return null;
            }
        }

        public static Point ToPoint(this SimplePoint point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point Translate(this Point point, int x, int y, float factor = 1.0f)
        {
            return new Point((int)((point.X + x) * factor), (int)((point.Y + y)* factor));
        }

        public static Point Translate(this Point point, Point translate, float factor = 1.0f)
        {
            return point.Translate(translate.X, translate.Y, factor);
        }

        public static SimplePoint ToSimplePoint(this Point point)
        {
            return new SimplePoint(point.X, point.Y);
        }

        public static SimplePoint Translate(this SimplePoint point, int x, int y)
        {
            return new SimplePoint(point.X + x, point.Y + y);
        }

        public static Color ToColor(this SimpleColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static SimpleColor ToSocketColor(this Color color)
        {
            return new SimpleColor(color.A, color.R, color.G, color.B);
        }
        
        public static void WriteMap(this ServerSocketConnection connection, Image map)
        {
            if (connection.ClientsCount == 0)
                return;

            connection.WriteMap(map.Width, map.Height, map.ToBytes());
        }

        public static void WriteFog(this ServerSocketConnection connection, Image fog)
        {
            if (connection.ClientsCount == 0)
                return;

            connection.WriteFog(fog.Width, fog.Height, fog.ToBytes());
        }

        public static TransformedGraphics TranslateAndZoom(this Graphics g, Point scroll, Size fullSize, float zoom, bool isFlippedView)
        {
            return new TransformedGraphics(g, scroll, fullSize, zoom, isFlippedView);
        }
    }
}
