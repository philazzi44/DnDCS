using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DnDCS.Libs;

namespace DnDCS.Client
{
    public partial class ClientControl : UserControl, IDnDCSControl
    {
        public ClientControl()
        {
            InitializeComponent();
        }

        public MainMenu GetMainMenu()
        {
            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
                new MenuItem("Connect", OnConnect_Click),
                new MenuItem("Exit", OnExit_Click),
            });
            menu.MenuItems.Add(fileMenu);
            return menu;
        }

        private void OnConnect_Click(object sender, EventArgs e)
        {
            // Prompt for IP address
            using (var getConnectIP = new GetConnectIPDialog())
            {
                if (getConnectIP.ShowDialog() == DialogResult.OK)
                {
                    Connect(getConnectIP.Address);
                }
            }
        }

        private void OnExit_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }

        private void Connect(string address)
        {
        }
    }
}
