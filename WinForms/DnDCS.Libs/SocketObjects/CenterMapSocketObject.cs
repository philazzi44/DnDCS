using System;
using System.Collections.Generic;

namespace DnDCS.Libs.SimpleObjects
{
    public class CenterMapSocketObject : BaseSocketObject
    {
        public SimplePoint CenterMap { get; private set; }

        public CenterMapSocketObject(SimplePoint centerMap)
            : base(SocketConstants.SocketAction.CenterMap)
        {
            CenterMap = centerMap;
        }

        public static CenterMapSocketObject CenterMapObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.CenterMap:
                    return new CenterMapSocketObject(new SimplePoint(BitConverter.ToInt32(bytes, 1), BitConverter.ToInt32(bytes, 5)));

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.AddRange(BitConverter.GetBytes(CenterMap.X));
            bytes.AddRange(BitConverter.GetBytes(CenterMap.Y));
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', CenterMapX: {1}", Action, CenterMap);
        }
    }
}
