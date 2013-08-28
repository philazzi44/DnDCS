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

        private void btnClient_Click(object sender, EventArgs e)
        {
            spltLauncher.Panel1Collapsed = true;
            var client = new ClientControl();

            InitializeControl(client);
            AddToPanel2(client);
        }

        private void btnServer_Click(object sender, EventArgs e)
        {
            spltLauncher.Panel1Collapsed = true;
            var server = new ServerControl();

            InitializeControl(server);
            AddToPanel2(server);
        }

        private void InitializeControl(IDnDCSControl control)
        {
            this.Menu = control.GetMainMenu();
        }

        private void AddToPanel2(Control control)
        {
            control.Dock = DockStyle.Fill;
            spltLauncher.Panel2.Controls.Add(control);
        }
    }
}
