
namespace DnDCS.Libs.SimpleObjects
{
    public static class SocketConstants
    {
        public static readonly BaseSocketObject AcknowledgeSocketObject = new BaseSocketObject(SocketConstants.SocketAction.Acknowledge);
        public static readonly BaseSocketObject PingSocketObject = new BaseSocketObject(SocketConstants.SocketAction.Ping);
        public static readonly BaseSocketObject ExitSocketObject = new BaseSocketObject(SocketConstants.SocketAction.Exit);

        // Note: Any updates to this enum should also be added to the Web Client's javascript enumeration.
        public enum SocketAction : byte
        {
            Unknown = 0,
            Acknowledge = 1,
            Ping = 2,
            Map = 3,
            CenterMap = 4,
            Fog = 5,
            FogUpdate = 6,
            FogOrRevealAll = 7,
            UseFogAlphaEffect = 8,
            GridSize = 9,
            GridColor = 10,
            BlackoutOn = 11,
            BlackoutOff = 12,
            Exit = 13,
        }
    }
}
