using System;
using System.Collections.Generic;
using System.Linq;

namespace DnDCS.Libs.SimpleObjects
{
    public class FogUpdateSocketObject : BaseSocketObject
    {
        public FogUpdate FogUpdateInstance { get; set; }

        public FogUpdateSocketObject(SocketConstants.SocketAction action, FogUpdate fogUpdate) :
            base(SocketConstants.SocketAction.FogUpdate)
        {
            FogUpdateInstance = new FogUpdate(fogUpdate.Points, fogUpdate.IsClearing);
        }
        
        public static FogUpdateSocketObject PointArrayObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.FogUpdate:
                    return new FogUpdateSocketObject(action, new FogUpdate(ConvertBytesToSocketPointArray(bytes.Skip(2).ToArray()), bytes[1] == (byte)1));

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        private static byte[] ConvertSocketPointsToBytes(SimplePoint[] points)
        {
            var pointBytes = new List<byte>(points.Length * 4);
            foreach (var point in points)
            {
                pointBytes.AddRange(BitConverter.GetBytes(point.X));
                pointBytes.AddRange(BitConverter.GetBytes(point.Y));
            }
            return pointBytes.ToArray();
        }

        private static SimplePoint[] ConvertBytesToSocketPointArray(byte[] pointBytes)
        {
            var points = new List<SimplePoint>(pointBytes.Length / 4);
            for (int i = 0; i < pointBytes.Length; i += 8)
            {
                var x = BitConverter.ToInt32(pointBytes, i);
                var y = BitConverter.ToInt32(pointBytes, i + 4);
                points.Add(new SimplePoint(x, y));
            }
            return points.ToArray();
        }

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.Add(FogUpdateInstance.IsClearing ? (byte)1 : (byte)0);
            bytes.AddRange(ConvertSocketPointsToBytes(FogUpdateInstance.Points));
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Number of Points: {1}, IsClearing: {2}", Action, FogUpdateInstance.Length, FogUpdateInstance.IsClearing);
        }
    }
}