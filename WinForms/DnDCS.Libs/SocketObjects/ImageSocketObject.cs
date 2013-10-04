using System;
using System.Collections.Generic;
using System.Linq;

namespace DnDCS.Libs.SimpleObjects
{
    public class ImageSocketObject : BaseSocketObject
    {
        public SimpleImage Image { get; private set; }
        public ImageSocketObject(SocketConstants.SocketAction action, SimpleImage image)
            : base(action)
        {
            Image = image;
        }

        public ImageSocketObject(SocketConstants.SocketAction action, int imageWidth, int imageHeight, byte[] imageBytes)
            : this(action, new SimpleImage(imageWidth, imageHeight, imageBytes))
        {
        }
        
        public static ImageSocketObject ImageObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.Map:
                case SocketConstants.SocketAction.Fog:
                    return new ImageSocketObject(action, BitConverter.ToInt32(bytes, 1), BitConverter.ToInt32(bytes, 5), bytes.Skip(9).ToArray());

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.AddRange(BitConverter.GetBytes(Image.Width));
            bytes.AddRange(BitConverter.GetBytes(Image.Height));
            bytes.AddRange(Image.Bytes);
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Width x Height: {1}x{2}, Image Bytes Length: {3}", Action, Image.Width, Image.Height, Image.Bytes.Length);
        }
    }
}