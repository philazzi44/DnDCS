using System.Configuration;

namespace DnDCS.XNA.Libs
{
    public static class ConfigValues
    {
        public static readonly string WinFormsApp;

        static ConfigValues()
        {
            WinFormsApp = ConfigurationManager.AppSettings["WinFormsApp"] ?? "DnDCS.Win.exe";
        }
    }
}
