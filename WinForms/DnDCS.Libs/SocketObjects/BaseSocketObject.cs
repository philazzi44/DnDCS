using System;

namespace DnDCS.Libs.SimpleObjects
{
    public class BaseSocketObject
    {
        public SocketConstants.SocketAction Action { get; private set; }
        protected byte ActionByte { get { return (byte)Action; } }

        public BaseSocketObject() : this(SocketConstants.SocketAction.Unknown)
        {
        }

        public BaseSocketObject(SocketConstants.SocketAction action)
        {
            Action = action;
        }

        public static BaseSocketObject BaseObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.Acknowledge:
                case SocketConstants.SocketAction.Ping:
                case SocketConstants.SocketAction.BlackoutOn:
                case SocketConstants.SocketAction.BlackoutOff:
                case SocketConstants.SocketAction.Exit:
                    return new BaseSocketObject(action);

                case SocketConstants.SocketAction.GridSize:
                    return GridSizeSocketObject.GridSizeObjectFromBytes(bytes);
                case SocketConstants.SocketAction.GridColor:
                    return ColorSocketObject.GridColorObjectFromBytes(bytes);
                case SocketConstants.SocketAction.Map:
                case SocketConstants.SocketAction.Fog:
                    return ImageSocketObject.ImageObjectFromBytes(bytes);
                case SocketConstants.SocketAction.FogUpdate:
                    return FogUpdateSocketObject.PointArrayObjectFromBytes(bytes);
                case SocketConstants.SocketAction.CenterMap:
                    return CenterMapSocketObject.CenterMapObjectFromBytes(bytes);

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        public virtual byte[] GetBytes()
        {
            return new byte[] { ActionByte };
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}'", Action);
        }
    }
}
