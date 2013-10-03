using System;
using System.Collections.Generic;
using System.Linq;

namespace DnDCS.Libs.SimpleObjects
{
    public class ImageSocketObject : BaseSocketObject
    {
        public byte[] ImageBytes { get; private set; }

        public ImageSocketObject(SocketConstants.SocketAction action, byte[] imageBytes)
            : base(action)
        {
            ImageBytes = imageBytes;
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

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.AddRange(ImageBytes);
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Image Bytes Length: {1}", Action, ImageBytes.Length);
        }
    }
}