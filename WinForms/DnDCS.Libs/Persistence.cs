using System;
using System.IO;
using System.Xml.Serialization;
using DnDCS.Libs.SimpleObjects;
using DnDCS.Libs.PersistenceObjects;

namespace DnDCS.Libs
{
    public static class Persistence
    {
        public static bool SaveClientData(ClientData clientData)
        {
            try
            {
                SaveData(ConfigValues.ClientDataFile, clientData);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to save Client Data.", e);
                return false;
            }
        }

        public static ClientData LoadClientData()
        {
            ClientData clientData = null;
            try
            {
                clientData = LoadData<ClientData>(ConfigValues.ClientDataFile);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load Client Data.", e);
            }
            return clientData ?? new ClientData()
            {
                ServerAddressHistory = new SimpleServerAddress[0],
            };
        }

        public static bool SaveServerData(ServerData serverData)
        {
            try
            {
                SaveData(ConfigValues.ServerDataFile, serverData);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to save Server Data.", e);
                return false;
            }
        }

        public static ServerData LoadServerData()
        {
            ServerData serverData = null;
            try
            {
                serverData = LoadData<ServerData>(ConfigValues.ServerDataFile);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load Server Data.", e);
            }
            return serverData ?? new ServerData()
            {
                RealTimeFogUpdates = true,
                ShowLog = true,
            };
        }

        private static void SaveData(string fileName, object data)
        {
            var serializer = new XmlSerializer(data.GetType());
            using (var stream = new StreamWriter(fileName))
            {
                serializer.Serialize(stream, data);
            }
        }

        private static T LoadData<T>(string fileName) where T : class
        {
            if (File.Exists(fileName))
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var stream = new StreamReader(fileName))
                {
                    return serializer.Deserialize(stream) as T;
                }
            }
            return null;
        }
    }
}