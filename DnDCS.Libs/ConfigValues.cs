using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DnDCS.Libs
{
    public static class ConfigValues
    {
        public static readonly int DefaultServerPort;
        public static readonly string DefaultServerName;
        public static readonly string DefaultServerIP1;
        public static readonly string DefaultServerIP2;
        public static readonly string DefaultServerIP3;
        public static readonly string DefaultServerIP4;
        public static readonly int PingInterval;
        public static readonly string ClientDataFile;
        public static readonly string ServerDataFile;
        public static readonly int MinimumGridSize;
        public static readonly int MaximumGridSize;
        public static readonly float MaximumGridZoomFactor;
        public static readonly float MinimumGridZoomFactor;
        public static readonly bool LogPings;

        static ConfigValues()
        {
            int defaultServerPort;
            DefaultServerPort = int.TryParse(ConfigurationManager.AppSettings["DefaultServerPort"], out defaultServerPort) ? defaultServerPort : 11000;

            DefaultServerName = ConfigurationManager.AppSettings["DefaultServerName"] ?? "localhost";
            DefaultServerIP1 = ConfigurationManager.AppSettings["DefaultServerIP1"] ?? "127";
            DefaultServerIP2 = ConfigurationManager.AppSettings["DefaultServerIP2"] ?? "0";
            DefaultServerIP3 = ConfigurationManager.AppSettings["DefaultServerIP3"] ?? "0";
            DefaultServerIP4 = ConfigurationManager.AppSettings["DefaultServerIP4"] ?? "1";

            ClientDataFile = ConfigurationManager.AppSettings["ClientDataFile"] ?? "ClientData.xml";
            ServerDataFile = ConfigurationManager.AppSettings["ServerDataFile"] ?? "ServerData.xml";

            int pingInterval;
            PingInterval = int.TryParse(ConfigurationManager.AppSettings["PingInterval"], out pingInterval) ? pingInterval : 5000;

            int minimumGridSize;
            MinimumGridSize = int.TryParse(ConfigurationManager.AppSettings["MinimumGridSize"], out minimumGridSize) ? minimumGridSize : 10;
            int maximumGridSize;
            MaximumGridSize = int.TryParse(ConfigurationManager.AppSettings["MaximumGridSize"], out maximumGridSize) ? maximumGridSize : 256;
            
            float minimumGridZoomFactor;
            MinimumGridZoomFactor = float.TryParse(ConfigurationManager.AppSettings["MinimumGridZoomFactor"], out minimumGridZoomFactor) ? minimumGridZoomFactor : 0.2f;
            float maximumGridZoomFactor;
            MaximumGridZoomFactor = float.TryParse(ConfigurationManager.AppSettings["MaximumGridZoomFactor"], out maximumGridZoomFactor) ? maximumGridZoomFactor : 10.0f;

            bool logPings;
            LogPings = bool.TryParse(ConfigurationManager.AppSettings["LogPings"], out logPings) ? LogPings : false;
        }

    }
}
