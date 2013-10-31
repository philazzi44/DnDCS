using System;
using System.Collections.Generic;
using System.Linq;

namespace DnDCS.Libs.SimpleObjects
{
    public class FogOrRevealAllSocketObject : BaseSocketObject
    {
        public bool FogAll { get; set; }

        public FogOrRevealAllSocketObject(SocketConstants.SocketAction action, bool fogAll) :
            base(SocketConstants.SocketAction.FogOrRevealAll)
        {
            FogAll = fogAll;
        }
        
        public static FogOrRevealAllSocketObject FogOrRevealAllObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.FogOrRevealAll:
                    return new FogOrRevealAllSocketObject(action, bytes[1] == (byte)1);

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.Add(FogAll ? (byte)1 : (byte)0);
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Fog All: {1}", Action, FogAll);
        }
    }
}