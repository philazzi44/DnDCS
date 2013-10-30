using System;
using System.IO;
using System.Xml.Serialization;
using DnDCS.Libs.PersistenceObjects;
using DnDCS.Libs.SimpleObjects;

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

        private static string GetFogDataFileName(string imageUrl)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                imageUrl = imageUrl.Replace(c, '-');
            return Path.ChangeExtension(Path.Combine(ConfigValues.ServerFogDataFolder, imageUrl), "png");
        }

        public static bool PeekServerFogData(string imageUrl)
        {
            return File.Exists(GetFogDataFileName(imageUrl));
        }

        public static byte[] LoadServerFogData(string imageUrl)
        {
            var fileName = GetFogDataFileName(imageUrl);
            if (File.Exists(fileName))
            {
                using (var sourceStream = new FileStream(fileName, FileMode.Open))
                {
                    using(var memoryStream = new MemoryStream())
                    {
                      sourceStream.CopyTo(memoryStream);
                      return memoryStream.ToArray();
                    }
                }
            }

            return null;
        }

        public static void SaveServerFogData(string imageUrl, byte[] fogData)
        {
            try
            {
                var fileName = GetFogDataFileName(imageUrl);
                if (fogData == null)
                {
                    File.Delete(fileName);
                }
                else
                {
                    // Ensure the full directory structure exists for the path.
                    var dirName = Path.GetDirectoryName(fileName);
                    if (!string.IsNullOrWhiteSpace(dirName))
                        Directory.CreateDirectory(dirName);

                    using (var stream = new FileStream(fileName, FileMode.Create))
                    {
                        stream.Write(fogData, 0, fogData.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to save Fog Data.", e);
            }
        }

        private static void SaveData(string fileName, object data)
        {
            try
            {
                // Ensure the full directory structure exists for the path.
                var dirName = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrWhiteSpace(dirName))
                    Directory.CreateDirectory(dirName);

                var serializer = new XmlSerializer(data.GetType());
                using (var stream = new StreamWriter(fileName))
                {
                    serializer.Serialize(stream, data);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format("Failed to save data to {0}.", fileName), e);
            }
        }

        private static T LoadData<T>(string fileName) where T : class
        {
            try
            {
                if (File.Exists(fileName))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    using (var stream = new StreamReader(fileName))
                    {
                        return serializer.Deserialize(stream) as T;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format("Failed to load data from {0}.", fileName), e);
            }
            return null;
        }
    }
}