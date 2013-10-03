
namespace DnDCS.Libs.SimpleObjects
{
    public static class SocketConstants
    {
        public static readonly BaseSocketObject AcknowledgeSocketObject = new BaseSocketObject(SocketConstants.SocketAction.Acknowledge);
        public static readonly BaseSocketObject PingSocketObject = new BaseSocketObject(SocketConstants.SocketAction.Ping);
        public static readonly BaseSocketObject ExitSocketObject = new BaseSocketObject(SocketConstants.SocketAction.Exit);

        public enum SocketAction : byte
        {
            Unknown = 0,
            Acknowledge = 44,
            Ping,
            Map,
            Fog,
            FogUpdate,
            GridSize,
            GridColor,
            BlackoutOn,
            BlackoutOff,
            Exit,
        }
    }
}
