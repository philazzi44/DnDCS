using System.Configuration;

namespace DnDCS.XNA.Libs
{
    public static class XNAConfigValues
    {
        public static readonly string WinFormsApp;
        public static readonly bool ShowDebug;

        static XNAConfigValues()
        {
            WinFormsApp = ConfigurationManager.AppSettings["WinFormsApp"] ?? "DnDCS.Win.exe";
            ShowDebug = (ConfigurationManager.AppSettings["ShowDebug"] ?? "false") == "true";
        }
    }
}
