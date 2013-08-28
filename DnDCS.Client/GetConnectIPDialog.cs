using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DnDCS.Client
{
    public partial class GetConnectIPDialog : Form
    {
        public string Address
        {
            get { return (rdoIP.Checked) ? string.Join(".", tboIP1.Text, tboIP2.Text, tboIP3.Text, tboIP4.Text).Trim() : tboName.Text.Trim(); }
        }

        public GetConnectIPDialog()
        {
            InitializeComponent();
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
                tboName.Text = string.Empty;
                pnlName.Visible = false;
                pnlIP.Visible = true;
            }
            else
            {
                tboIP1.Text = tboIP2.Text = tboIP3.Text = tboIP4.Text = string.Empty;
                pnlName.Visible = true;
                pnlIP.Visible = false;
            }
        }

        private void tboName_TextChanged(object sender, EventArgs e)
        {
            btnPing.Enabled = btnConnect.Enabled = !string.IsNullOrWhiteSpace(tboName.Text);
        }

        private void OnIPChanged()
        {
            btnPing.Enabled = btnConnect.Enabled = !string.IsNullOrWhiteSpace(tboIP1.Text) &&
                                                   !string.IsNullOrWhiteSpace(tboIP2.Text) &&
                                                   !string.IsNullOrWhiteSpace(tboIP3.Text) &&
                                                   !string.IsNullOrWhiteSpace(tboIP4.Text);
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

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
