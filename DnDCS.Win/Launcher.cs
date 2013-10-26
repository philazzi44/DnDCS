using System;
using System.Drawing;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using DnDCS.Win.Client;
using DnDCS.Win.Libs;
using DnDCS.Win.Server;

namespace DnDCS.Win
{
    public partial class Launcher : Form
    {
        // Tracks the initial values on the form when we decide to toggle Full Screen mode.
        private bool initialFormTopMost;
        private FormBorderStyle initialFormBorderStyle;
        private FormWindowState initialFormWindowState;
        private MainMenu _menu;

        private IDnDCSControl control;

        private Constants.RunMode? runMode;
        private SimpleServerAddress ClientStartupServerAddress;

        public Launcher()
        {
            InitializeComponent();
        }

        public static Launcher CreateClient(SimpleServerAddress clientStartupServerAddress)
        {
            return new Launcher()
            {
                runMode = Constants.RunMode.Client,
                ClientStartupServerAddress = clientStartupServerAddress,
            };
        }

        public static Launcher CreateServer()
        {
            return new Launcher()
            {
                runMode = Constants.RunMode.Server,
            };
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            this.Icon = DnDCS.Win.Libs.Assets.AssetsLoader.LauncherIcon;

            initialFormTopMost = this.TopMost;
            initialFormBorderStyle = this.FormBorderStyle;
            initialFormWindowState = this.WindowState;

            if (this.runMode.HasValue)
            {
                switch (this.runMode.Value)
                {
                    case Constants.RunMode.Client:
                        this.btnClient.PerformClick();
                        break;

                    case Constants.RunMode.Server:
                        this.btnServer.PerformClick();
                        break;

                    default:
                        break;
                }
            }
        }

        private void Launcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logger.LogInfo("Application closing.");
        }
        
        private void btnClient_Click(object sender, EventArgs e)
        {
            SetMode("Client", DnDCS.Win.Libs.Assets.AssetsLoader.ClientIcon, new ClientControl()
                {
                    StartupServerAddress = ClientStartupServerAddress,
                });
        }

        private void btnServer_Click(object sender, EventArgs e)
        {
            SetMode("Server", DnDCS.Win.Libs.Assets.AssetsLoader.ServerIcon, new ServerControl());
        }

        private void SetMode(string mode, Icon icon, IDnDCSControl control)
        {
            this.control = control;

            Logger.FileSuffix = mode;
            Logger.LogInfo(string.Format("Initializing {0} Mode", mode));

            this.Text = "DnDCS - " + mode;
            this.Icon = icon;
            var del = this.pnlInit;
            this.Controls.Remove(del);
            del.Dispose();

            if (control is IDnDCSControl)
            {
                var dndCSControl = (IDnDCSControl)control;
                this.Menu = this._menu = dndCSControl.GetMainMenu();
                dndCSControl.ToggleFullScreen = ToggleFullScreen;
            }

            ((Control)control).Dock = DockStyle.Fill;
            this.Controls.Add((Control)control);
        }

        private void ToggleFullScreen(bool goFullScreen)
        {
            if (goFullScreen)
            {
                // Must force Normal state before trying to Maximize again.
                if (this.WindowState == FormWindowState.Maximized)
                    this.WindowState = FormWindowState.Normal;

                this.TopMost = true;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.Menu = null;
            }
            else
            {
                this.TopMost = initialFormTopMost;
                this.FormBorderStyle = initialFormBorderStyle;
                this.WindowState = initialFormWindowState;
                this.Menu = _menu;
            }
        }
    }
}
