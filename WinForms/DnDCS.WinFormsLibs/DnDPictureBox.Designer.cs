using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DnDCS.WinFormsLibs
{
    public partial class DnDPictureBox
    {
        protected PictureBox pbxMap;

        private void InitializeComponent()
        {
            this.pbxMap = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).BeginInit();
            this.SuspendLayout();
            // 
            // pbxMap
            // 
            this.pbxMap.BackColor = System.Drawing.Color.Black;
            this.pbxMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxMap.Location = new System.Drawing.Point(0, 0);
            this.pbxMap.Name = "pbxMap";
            this.pbxMap.Size = new System.Drawing.Size(150, 150);
            this.pbxMap.TabIndex = 0;
            this.pbxMap.TabStop = false;
            // 
            // DnDPictureBox
            // 
            this.Controls.Add(this.pbxMap);
            this.Name = "DnDPictureBox";
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
