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
            var pointBytes = new List<byte>();
            foreach (var point in points)
            {
                pointBytes.AddRange(BitConverter.GetBytes(point.X));
                pointBytes.AddRange(BitConverter.GetBytes(point.Y));
            }
            return pointBytes.ToArray();
        }

        private static Point[] ConvertBytesToPointArray(byte[] pointBytes)
        {
            var points = new List<Point>();
            for (int i = 0; i < pointBytes.Length; i+=4)
            {
                var x = BitConverter.ToInt32(pointBytes, i);
                var y = BitConverter.ToInt32(pointBytes, i);
                points.Add(new Point(x, y));
            }
            return points.ToArray();
        }

        public override List<byte> GetBytes()
        {
            var bytes = base.GetBytes();
            bytes.AddRange(ConvertPointsToBytes(this.Points));
            return bytes;
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Number of Points: {1}, IsClearing: .", Action, Points.Length, IsClearing);
        }
    }
}