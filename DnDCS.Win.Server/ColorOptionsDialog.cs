using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DnDCS.Win.Server
{
    public partial class ColorOptionsDialog : Form
    {
        [Browsable(false)]
        public Color GridLineColor
        {
            get { return ctlGridLines.Value; }
            set { ctlGridLines.Value = value; }
        }
        
        public ColorOptionsDialog()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
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
