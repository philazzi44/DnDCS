using System.Configuration;

namespace DnDCS.Libs
{
    public static class ConfigValues
    {
        public static readonly int DefaultServerNetSocketPort;
        public static readonly int DefaultServerWebSocketPort;
        public static readonly string DefaultServerName;
        public static readonly string DefaultServerIP1;
        public static readonly string DefaultServerIP2;
        public static readonly string DefaultServerIP3;
        public static readonly string DefaultServerIP4;
        public static readonly int PingInterval;
        public static readonly string ClientDataFile;
        public static readonly string ServerDataFile;
        public static readonly string ServerFogDataFolder;
        public static readonly int MinimumGridSize;
        public static readonly int MaximumGridSize;
        public static readonly float MaximumZoomFactor;
        public static readonly float MinimumZoomFactor;
        public static readonly bool LogPings;
        public static readonly int FogSaveInterval;

        static ConfigValues()
        {
            int defaultServerSocketPort;
            DefaultServerNetSocketPort = int.TryParse(ConfigurationManager.AppSettings["DefaultServerNetSocketPort"], out defaultServerSocketPort) ? defaultServerSocketPort : 11000;

            int defaultServerWebSocketPort;
            DefaultServerWebSocketPort = int.TryParse(ConfigurationManager.AppSettings["DefaultServerWebSocketPort"], out defaultServerWebSocketPort) ? defaultServerWebSocketPort : 11001;

            DefaultServerName = ConfigurationManager.AppSettings["DefaultServerName"] ?? "localhost";
            DefaultServerIP1 = ConfigurationManager.AppSettings["DefaultServerIP1"] ?? "127";
            DefaultServerIP2 = ConfigurationManager.AppSettings["DefaultServerIP2"] ?? "0";
            DefaultServerIP3 = ConfigurationManager.AppSettings["DefaultServerIP3"] ?? "0";
            DefaultServerIP4 = ConfigurationManager.AppSettings["DefaultServerIP4"] ?? "1";

            ClientDataFile = ConfigurationManager.AppSettings["ClientDataFile"] ?? "ClientData.xml";
            ServerDataFile = ConfigurationManager.AppSettings["ServerDataFile"] ?? "ServerData.xml";
            ServerFogDataFolder = ConfigurationManager.AppSettings["ServerFogDataFolder"] ?? "FogData";

            int pingInterval;
            PingInterval = int.TryParse(ConfigurationManager.AppSettings["PingInterval"], out pingInterval) ? pingInterval : 5000;

            int minimumGridSize;
            MinimumGridSize = int.TryParse(ConfigurationManager.AppSettings["MinimumGridSize"], out minimumGridSize) ? minimumGridSize : 10;
            int maximumGridSize;
            MaximumGridSize = int.TryParse(ConfigurationManager.AppSettings["MaximumGridSize"], out maximumGridSize) ? maximumGridSize : 256;
            
            float minimumGridZoomFactor;
            MinimumZoomFactor = float.TryParse(ConfigurationManager.AppSettings["MinimumZoomFactor"], out minimumGridZoomFactor) ? minimumGridZoomFactor : 0.2f;
            float maximumGridZoomFactor;
            MaximumZoomFactor = float.TryParse(ConfigurationManager.AppSettings["MaximumZoomFactor"], out maximumGridZoomFactor) ? maximumGridZoomFactor : 10.0f;

            bool logPings;
            LogPings = bool.TryParse(ConfigurationManager.AppSettings["LogPings"], out logPings) ? LogPings : false;

            int fogSaveInterval;
            FogSaveInterval = int.TryParse(ConfigurationManager.AppSettings["FogSaveInterval"], out fogSaveInterval) ? fogSaveInterval : 60000;
        }

    }
}
