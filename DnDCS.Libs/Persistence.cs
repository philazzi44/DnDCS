using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DnDCS.Libs.PersistenceObjects;
using System.Xml.Serialization;
using System.IO;

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
            try
            {
                return LoadData<ClientData>(ConfigValues.ClientDataFile);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load Client Data.", e);
                return null;
            }
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
            try
            {
                return LoadData<ServerData>(ConfigValues.ServerDataFile);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load Server Data.", e);
                return null;
            }
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
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new StreamReader(fileName))
            {
                return serializer.Deserialize(stream) as T;
            }
        }
    }
}