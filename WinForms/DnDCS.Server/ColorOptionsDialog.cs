using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DnDCS.Server
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
