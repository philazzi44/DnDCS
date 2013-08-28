using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace DnDCS.Libs.SocketObjects
{
    public class ImageSocketObject : BaseSocketObject
    {
        private Image image;
        public Image Image
        {
            get
            {
                if (image == null)
                    image = ConvertBytesToImage(ImageBytes);
                return image;
            }
        }

        public byte[] ImageBytes { get; private set; }

        private ImageSocketObject(SocketConstants.SocketAction action, byte[] imageBytes)
            : base(action)
        {
            ImageBytes = imageBytes;
        }

        private ImageSocketObject(SocketConstants.SocketAction action, Image image)
            : base(action)
        {
            ImageBytes = ConvertImageToBytes(image);
            image = (Image)image.Clone();
        }

        public static ImageSocketObject ImageObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.Map:
                case SocketConstants.SocketAction.Fog:
                    return new ImageSocketObject(action, bytes.Skip(1).ToArray());

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        public static ImageSocketObject CreateMap(Image map)
        {
            var socketObject = new ImageSocketObject(SocketConstants.SocketAction.Map, map);
            return socketObject;
        }

        public static ImageSocketObject CreateFog(Image fog)
        {
            var socketObject = new ImageSocketObject(SocketConstants.SocketAction.Fog, fog);
            return socketObject;
        }

        private static byte[] ConvertImageToBytes(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private static Image ConvertBytesToImage(byte[] dataBytes)
        {
            using (var ms = new MemoryStream(dataBytes))
            {
                return Image.FromStream(ms);
            }
        }

        public override List<byte> GetBytes()
        {
            var bytes = base.GetBytes();
            bytes.AddRange(ImageBytes);
            return bytes;
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Bytes Length: {1}.", Action, ImageBytes.Length);
        }
    }
}