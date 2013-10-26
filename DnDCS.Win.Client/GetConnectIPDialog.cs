using System;
using System.Linq;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS.Win.Client
{
    public partial class GetConnectIPDialog : Form
    {
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
                if (lboHistory.Items.Count > 0)
                    lboHistory.SelectedIndex = 0;
            }

            Closed += new EventHandler(GetConnectIPDialog_Closed);
        }

        private void GetConnectIPDialog_Closed(object sender, EventArgs e)
        {
            var clientData = Persistence.LoadClientData();
            clientData.ServerAddressHistory = lboHistory.Items.OfType<SimpleServerAddress>().ToArray();
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
            lboHistory.SelectedIndex = -1;
            OnNameChanged();
        }

        private void tboIP1_TextChanged(object sender, EventArgs e)
        {
            lboHistory.SelectedIndex = -1;
            OnIPChanged();
        }

        private void tboIP2_TextChanged(object sender, EventArgs e)
        {
            lboHistory.SelectedIndex = -1;
            OnIPChanged();
        }

        private void tboIP3_TextChanged(object sender, EventArgs e)
        {
            lboHistory.SelectedIndex = -1;
            OnIPChanged();
        }

        private void tboIP4_TextChanged(object sender, EventArgs e)
        {
            lboHistory.SelectedIndex = -1;
            OnIPChanged();
        }

        private void tboPort_TextChanged(object sender, EventArgs e)
        {
            lboHistory.SelectedIndex = -1;
            OnPortChanged();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var newAddress = new SimpleServerAddress()
            {
                Address = Address,
                Port = this.Port
            };
            if (!lboHistory.Items.Contains(newAddress))
                lboHistory.Items.Insert(0, newAddress);

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
                var serverAddress = (SimpleServerAddress)lboHistory.SelectedItem;
                if (Utils.IsIPAddress(serverAddress.Address))
                {
                    rdoIP.Checked = true;
                    var ipSplit = serverAddress.Address.Split('.');

                    tboIP1.TextChanged -= new EventHandler(tboIP1_TextChanged);
                    tboIP1.Text = ipSplit[0];
                    tboIP1.TextChanged += new EventHandler(tboIP1_TextChanged);

                    tboIP2.TextChanged -= new EventHandler(tboIP2_TextChanged);
                    tboIP2.Text = ipSplit[1];
                    tboIP2.TextChanged += new EventHandler(tboIP2_TextChanged);

                    tboIP3.TextChanged -= new EventHandler(tboIP3_TextChanged);
                    tboIP3.Text = ipSplit[2];
                    tboIP3.TextChanged += new EventHandler(tboIP3_TextChanged);

                    tboIP4.TextChanged -= new EventHandler(tboIP4_TextChanged);
                    tboIP4.Text = ipSplit[3];
                    tboIP4.TextChanged += new EventHandler(tboIP4_TextChanged);
                }
                else
                {
                    rdoName.Checked = true;
                    tboName.TextChanged -= new EventHandler(tboName_TextChanged);
                    tboName.Text = serverAddress.Address;
                    tboName.TextChanged += new EventHandler(tboName_TextChanged);
                }
                tboPort.TextChanged -= new EventHandler(tboPort_TextChanged);
                tboPort.Text = serverAddress.Port.ToString();
                tboPort.TextChanged += new EventHandler(tboPort_TextChanged);
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
        }

        private void OnNameChanged()
        {
            int port;
            btnPing.Enabled = btnConnect.Enabled = !string.IsNullOrWhiteSpace(tboName.Text) && int.TryParse(tboPort.Text, out port);
        }

        private void OnIPChanged()
        {
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
