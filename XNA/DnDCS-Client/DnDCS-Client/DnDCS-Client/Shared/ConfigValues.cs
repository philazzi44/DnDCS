using System.Configuration;

namespace DnDCS_Client.Shared
{
    public static class ConfigValues
    {
        public static readonly string WinFormsApp;

        static ConfigValues()
        {
            WinFormsApp = ConfigurationManager.AppSettings["WinFormsApp"] ?? "DnDCS.exe";
        }
    }
}
