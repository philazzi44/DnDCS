using System;
using System.Collections.Generic;

namespace DnDCS.Libs.SimpleObjects
{
    public class ColorSocketObject : BaseSocketObject
    {
        public SimpleColor Value
        {
            get { return new SimpleColor(A, R, G, B); }
            set
            {
                A = value.A;
                R = value.R;
                G = value.G;
                B = value.B;
            }
        }
        public byte A { get; private set; }
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public ColorSocketObject(SocketConstants.SocketAction socketAction, byte a, byte r, byte g, byte b) :
            base(socketAction)
        {
            Value = new SimpleColor(a, r, g, b);
        }

        public ColorSocketObject(SocketConstants.SocketAction socketAction, SimpleColor color) :
            this(socketAction, color.A, color.R, color.G, color.B)
        {
        }

        public static ColorSocketObject GridColorObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.GridColor:
                    return new ColorSocketObject(action, bytes[1], bytes[2], bytes[3], bytes[4]);

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.Add(A);
            bytes.Add(R);
            bytes.Add(G);
            bytes.Add(B);
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Color: {1}", Action, Value);
        }
    }
}