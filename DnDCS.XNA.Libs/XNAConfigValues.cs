using System.Configuration;

namespace DnDCS.XNA.Libs
{
    public static class XNAConfigValues
    {
        public static readonly string WinFormsApp;

        static XNAConfigValues()
        {
            WinFormsApp = ConfigurationManager.AppSettings["WinFormsApp"] ?? "DnDCS.Win.exe";
        }
    }
}
