using System.Drawing;
using System.IO;
using DnDCS.Libs.SimpleObjects;
using DnDCS.Libs;

namespace DnDCS.WinFormsLibs
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
            using (var ms = new MemoryStream(dataBytes))
            {
                return Image.FromStream(ms);
            }
        }

        public static Point ToPoint(this SimplePoint point)
        {
            return new Point(point.X, point.Y);
        }

        public static SimplePoint ToSimplePoint(this Point point)
        {
            return new SimplePoint(point.X, point.Y);
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
            connection.WriteMap(map.Width, map.Height, map.ToBytes());
        }

        public static void WriteFog(this ServerSocketConnection connection, Image fog)
        {
            connection.WriteFog(fog.Width, fog.Height, fog.ToBytes());
        }

    }
}
