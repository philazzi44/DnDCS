using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.PersistenceObjects;
using System.Text.RegularExpressions;

namespace DnDCS.Client
{
    public partial class GetConnectIPDialog : Form
    {
        private bool unselectHistoryOnChange;

        public string Address
        {
            get { return (rdoIP.Checked) ? string.Join(".", tboIP1.Text, tboIP2.Text, tboIP3.Text, tboIP4.Text).Trim() : tboName.Text.Trim(); }
        }

        public int Port { get { return int.Parse(tboPort.Text); } }

        public GetConnectIPDialog()
        {
            InitializeComponent();
        }
        
        private void GetConnectIPDialog_Load(object sender, EventArgs e)
        {
            tboName.Text = ConfigValues.DefaultServerName;
            tboIP1.Text = ConfigValues.DefaultServerIP1;
            tboIP2.Text = ConfigValues.DefaultServerIP2;
            tboIP3.Text = ConfigValues.DefaultServerIP3;
            tboIP4.Text = ConfigValues.DefaultServerIP4;
            tboPort.Text = ConfigValues.DefaultServerPort.ToString();

            var clientData = Persistence.LoadClientData();
            if (clientData != null && clientData.ServerAddressHistory != null)
            {
                foreach (var serverAddress in clientData.ServerAddressHistory)
                    lboHistory.Items.Add(serverAddress);
            }

            Closed += new EventHandler(GetConnectIPDialog_Closed);
        }

        private void GetConnectIPDialog_Closed(object sender, EventArgs e)
        {
            var clientData = Persistence.LoadClientData() ?? new ClientData();
            clientData.ServerAddressHistory = lboHistory.Items.OfType<ServerAddress>().ToArray();
            Persistence.SaveClientData(clientData);
        }

        private void rdoName_CheckedChanged(object sender, EventArgs e)
        {
            ToggleFieldVisibility();
        }

        private void rdoIP_CheckedChanged(object sender, EventArgs e)
        {
            ToggleFieldVisibility();
        }

        private void ToggleFieldVisibility()
        {
            if (rdoIP.Checked)
            {
                pnlName.Visible = false;
                pnlIP.Visible = true;
            }
            else
            {
                pnlName.Visible = true;
                pnlIP.Visible = false;
            }
        }

        private void tboName_TextChanged(object sender, EventArgs e)
        {
            OnNameChanged();
        }

        private void tboIP1_TextChanged(object sender, EventArgs e)
        {
            OnIPChanged();
        }

        private void tboIP2_TextChanged(object sender, EventArgs e)
        {
            OnIPChanged();
        }

        private void tboIP3_TextChanged(object sender, EventArgs e)
        {
            OnIPChanged();
        }

        private void tboIP4_TextChanged(object sender, EventArgs e)
        {
            OnIPChanged();
        }

        private void tboPort_TextChanged(object sender, EventArgs e)
        {
            OnPortChanged();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            lboHistory.Items.Add(new ServerAddress()
            {
                Address = Address,
                Port = this.Port
            });

            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void lboHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lboHistory.SelectedIndex >= 0)
            {
                btnDelete.Enabled = true;
                var serverAddress = (ServerAddress)lboHistory.SelectedItem;
                if (Utils.IsIPAddress(serverAddress.Address))
                {
                    rdoIP.Checked = true;
                    var ipSplit = serverAddress.Address.Split('.');
                    tboIP1.Text = ipSplit[0];
                    tboIP2.Text = ipSplit[1];
                    tboIP3.Text = ipSplit[2];
                    tboIP4.Text = ipSplit[3];
                }
                else
                {
                    rdoName.Checked = true;
                    tboName.Text = serverAddress.Address;
                }
                tboPort.Text = serverAddress.Port.ToString();
                unselectHistoryOnChange = true;
            }
            else
            {
                btnDelete.Enabled = false;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lboHistory.SelectedIndex < 0)
                return;
            lboHistory.Items.RemoveAt(lboHistory.SelectedIndex);
            unselectHistoryOnChange = false;
        }

        private void OnNameChanged()
        {
            if (unselectHistoryOnChange)
            {
                lboHistory.SelectedIndex = -1;
                unselectHistoryOnChange = false;
            }

            int port;
            btnPing.Enabled = btnConnect.Enabled = !string.IsNullOrWhiteSpace(tboName.Text) && int.TryParse(tboPort.Text, out port);
        }

        private void OnIPChanged()
        {
            if (unselectHistoryOnChange)
            {
                lboHistory.SelectedIndex = -1;
                unselectHistoryOnChange = false;
            }

            int port;
            btnPing.Enabled = btnConnect.Enabled = !string.IsNullOrWhiteSpace(tboIP1.Text) &&
                                                   !string.IsNullOrWhiteSpace(tboIP2.Text) &&
                                                   !string.IsNullOrWhiteSpace(tboIP3.Text) &&
                                                   !string.IsNullOrWhiteSpace(tboIP4.Text) &&
                                                   int.TryParse(tboPort.Text, out port);
        }

        private void OnPortChanged()
        {
            if (rdoName.Checked)
                OnNameChanged();
            else
                OnIPChanged();
        }
    }
}
