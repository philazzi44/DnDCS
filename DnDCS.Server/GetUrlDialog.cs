using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace DnDCS.Server
{
    public partial class GetUrlDialog : Form
    {
        public GetUrlDialog()
        {
            InitializeComponent();
        }

        public Image LoadedImage
        {
            get { return pbxPreview.Image; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            btnLoad.Enabled = !string.IsNullOrWhiteSpace(pbxPreview.ImageLocation);
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(tboUrl.Text, @".*\.png"))
            {
                pbxPreview.ImageLocation = tboUrl.Text;
            }
        }

        private void tboUrl_TextChanged(object sender, EventArgs e)
        {
            btnLoad.Enabled = (Regex.IsMatch(tboUrl.Text, @".*\.png"));
        }
    }
}
