using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace DnDCS.Libs.SocketObjects
{
    public class PointArraySocketObject : BaseSocketObject
    {
        public Point[] Points { get; set; }
        public bool IsClearing { get; set; }

        public PointArraySocketObject(SocketConstants.SocketAction action, Point[] points, bool isClearing)
            : base(action)
        {
            Points = points;
            IsClearing = isClearing;
        }

        public static PointArraySocketObject PointArrayObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.FogUpdate:
                    return new PointArraySocketObject(action, ConvertBytesToPointArray(bytes.Skip(2).ToArray()), bytes[1] == (byte)1);

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        private static byte[] ConvertPointsToBytes(Point[] points)
        {
            var pointBytes = new List<byte>(points.Length * 4);
            foreach (var point in points)
            {
                pointBytes.AddRange(BitConverter.GetBytes(point.X));
                pointBytes.AddRange(BitConverter.GetBytes(point.Y));
            }
            return pointBytes.ToArray();
        }

        private static Point[] ConvertBytesToPointArray(byte[] pointBytes)
        {
            var points = new List<Point>(pointBytes.Length / 4);
            for (int i = 0; i < pointBytes.Length; i += 8)
            {
                var x = BitConverter.ToInt32(pointBytes, i);
                var y = BitConverter.ToInt32(pointBytes, i + 4);
                points.Add(new Point(x, y));
            }
            return points.ToArray();
        }

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.Add(IsClearing ? (byte)1 : (byte)0);
            bytes.AddRange(ConvertPointsToBytes(this.Points));
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Number of Points: {1}, IsClearing: {2}", Action, Points.Length, IsClearing);
        }
    }
}