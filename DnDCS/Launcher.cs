using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DnDCS.Client;
using DnDCS.Server;
using DnDCS.Libs;

namespace DnDCS
{
    public partial class Launcher : Form
    {
        public Launcher()
        {
            InitializeComponent();
        }
        
        private void Launcher_Load(object sender, EventArgs e)
        {
            this.Icon = DnDCS.Libs.Assets.AssetsLoader.LauncherIcon;
        }

        private void Launcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logger.LogInfo("Application closing.");
        }
        
        private void btnClient_Click(object sender, EventArgs e)
        {
            SetMode("Client", DnDCS.Libs.Assets.AssetsLoader.ClientIcon, new ClientControl());
        }

        private void btnServer_Click(object sender, EventArgs e)
        {
            SetMode("Server", DnDCS.Libs.Assets.AssetsLoader.ServerIcon, new ServerControl());
        }

        private void SetMode(string mode, Icon icon, Control control)
        {
            Logger.FileSuffix = mode;
            Logger.LogInfo(string.Format("Initializing {0} Mode", mode));

            this.Text = "DnDCS - " + mode;
            this.Icon = icon;
            var del = this.pnlInit;
            this.Controls.Remove(del);
            del.Dispose();

            if (control is IDnDCSControl)
                this.Menu = ((IDnDCSControl)control).GetMainMenu();

            control.Dock = DockStyle.Fill;
            this.Controls.Add(control);
            this.Invalidate();
        }
    }
}
