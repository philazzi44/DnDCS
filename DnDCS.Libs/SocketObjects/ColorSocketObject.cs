using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnDCS.Libs.SocketObjects
{
    public class ColorSocketObject : BaseSocketObject
    {
        public System.Drawing.Color Value
        {
            get { return System.Drawing.Color.FromArgb(A, R, G, B); }
            set
            {
                A = value.A;
                R = value.R;
                G = value.G;
                B = value.B;
            }
        }
        public int A { get; private set; }
        public int R { get; private set; }
        public int G { get; private set; }
        public int B { get; private set; }

        public ColorSocketObject(SocketConstants.SocketAction socketAction, int a, int r, int g, int b) :
            this(socketAction, System.Drawing.Color.FromArgb(a, r, g, b))
        {
        }

        public ColorSocketObject(SocketConstants.SocketAction socketAction, System.Drawing.Color gridColor)
            : base(socketAction)
        {
            Value = gridColor;
        }

        public static ColorSocketObject GridColorObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.GridColor:
                    return new ColorSocketObject(action, BitConverter.ToInt32(bytes, 1), BitConverter.ToInt32(bytes, 2), BitConverter.ToInt32(bytes, 3), BitConverter.ToInt32(bytes, 4));

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.AddRange(BitConverter.GetBytes(A));
            bytes.AddRange(BitConverter.GetBytes(R));
            bytes.AddRange(BitConverter.GetBytes(G));
            bytes.AddRange(BitConverter.GetBytes(B));
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Color: {1}", Action, Value);
        }
    }
}