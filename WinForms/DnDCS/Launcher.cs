using System;
using System.Drawing;
using System.Windows.Forms;
using DnDCS.Client;
using DnDCS.Libs;
using DnDCS.Libs.SocketObjects;
using DnDCS.Server;
using DnDCS.WinFormsLibs;

namespace DnDCS
{
    public partial class Launcher : Form
    {
        // Tracks the initial values on the form when we decide to toggle Full Screen mode.
        private bool initialFormTopMost;
        private FormBorderStyle initialFormBorderStyle;
        private FormWindowState initialFormWindowState;
        private MainMenu _menu;

        private DnDPoint lastScrollPosition = DnDPoint.Empty;
        private IDnDCSControl control;

        public Launcher()
        {
            InitializeComponent();
        }
        
        private void Launcher_Load(object sender, EventArgs e)
        {
            this.Icon = DnDCS.WinFormsLibs.Assets.AssetsLoader.LauncherIcon;

            initialFormTopMost = this.TopMost;
            initialFormBorderStyle = this.FormBorderStyle;
            initialFormWindowState = this.WindowState;
        }

        private void Launcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logger.LogInfo("Application closing.");
        }
        
        private void btnClient_Click(object sender, EventArgs e)
        {
            SetMode("Client", DnDCS.WinFormsLibs.Assets.AssetsLoader.ClientIcon, new ClientControl());
        }

        private void btnServer_Click(object sender, EventArgs e)
        {
            SetMode("Server", DnDCS.WinFormsLibs.Assets.AssetsLoader.ServerIcon, new ServerControl());
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
        
        private void Launcher_Activated(object sender, EventArgs e)
        {
            if (this.control != null)
                this.control.ScrollPosition = lastScrollPosition;
        }

        private void Launcher_Deactivate(object sender, EventArgs e)
        {
            if (this.control != null)
                lastScrollPosition = this.control.ScrollPosition;
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
