using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DnDCS.Server
{
    public partial class ColorOptionsControl : UserControl
    {
        public string Title
        {
            get { return gbxColor.Text; }
            set { gbxColor.Text = value; }
        }

        private Color _value;
        [Browsable(false)]
        public Color Value
        {
            get { return _value; }
            set
            {
                _value = value;
                btnColor.BackColor = _value;
                tbAlpha.Value = value.A;
                lblA.Text = "A: " + value.A.ToString();
                lblR.Text = "R: " + value.R.ToString();
                lblG.Text = "G: " + value.G.ToString();
                lblB.Text = "B: " + value.B.ToString();
            }
        }

        public ColorOptionsControl()
        {
            InitializeComponent();
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            using (var colorPicker = new ColorDialog())
            {
                colorPicker.Color = Color.FromArgb(255, Value);
                if (colorPicker.ShowDialog(this) == DialogResult.OK)
                {
                    Value = Color.FromArgb(tbAlpha.Value, colorPicker.Color);
                }
            }
        }

        private void tbAlpha_Scroll(object sender, EventArgs e)
        {
            Value = Color.FromArgb(tbAlpha.Value, btnColor.BackColor);
        }
    }
}
