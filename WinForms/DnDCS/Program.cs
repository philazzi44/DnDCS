using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Launcher launcher = null;
            if (args != null && args.Length > 0)
            {
                var modeString = args[0];
                int mode;
                if (int.TryParse(modeString, out mode))
                {
                    switch ((Constants.RunMode)mode)
                    {
                        case Constants.RunMode.Client:
                            if (args.Length > 2)
                            {
                                var address = args[1];
                                var portString = args[2];
                                int port;
                                if (Utils.IsIPAddress(address) && int.TryParse(portString, out port))
                                {
                                    launcher = Launcher.CreateClient(new SimpleServerAddress()
                                    {
                                        Address = address,
                                        Port = port,
                                    });
                                }
                            }
                            break;
                        case Constants.RunMode.Server:
                            launcher = Launcher.CreateServer();
                            break;
                        default:
                            break;
                    }
                }
            }

            if (launcher == null)
                launcher = new Launcher();

            Application.Run(launcher);
        }
    }
}
