using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnDCS.Libs.SocketObjects
{
    public static class SocketConstants
    {
        public static readonly BaseSocketObject AcknowledgeSocketObject = new BaseSocketObject(SocketConstants.SocketAction.Acknowledge);
        public static readonly BaseSocketObject ExitSocketObject = new BaseSocketObject(SocketConstants.SocketAction.Exit);

        // TODO: Port must be configurable
        public static int Port = 11000;

        public enum SocketAction : byte
        {
            EndOfData = 0,
            Unknown = 1,
            Acknowledge = 44,
            Map,
            Fog,
            FogUpdate,
            BlackoutOn,
            BlackoutOff,
            Exit,
        }
    }
}
