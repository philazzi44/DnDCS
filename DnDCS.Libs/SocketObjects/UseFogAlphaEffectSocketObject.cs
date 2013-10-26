using System;
using System.Collections.Generic;

namespace DnDCS.Libs.SimpleObjects
{
    class UseFogAlphaEffectSocketObject : BaseSocketObject
    {
        public bool UseFogAlphaEffect { get; private set; }

        public UseFogAlphaEffectSocketObject(bool useFogAlphaEffect)
            : base(SocketConstants.SocketAction.UseFogAlphaEffect)
        {
            UseFogAlphaEffect = useFogAlphaEffect;
        }

        public static UseFogAlphaEffectSocketObject UseFogAlphaEffectObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.UseFogAlphaEffect:
                    return new UseFogAlphaEffectSocketObject(BitConverter.ToBoolean(bytes, 1));

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.AddRange(BitConverter.GetBytes(UseFogAlphaEffect));
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Use Fog Alpha Effect: {1}", Action, UseFogAlphaEffect);
        }
    }
}
