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
            this.pbxMap.Paint += new System.Windows.Forms.PaintEventHandler(this.pbxMap_Paint);
            this.pbxMap.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseClick);
            this.pbxMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseDown);
            this.pbxMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseMove);
            this.pbxMap.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseUp);
            this.pbxMap.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.pbxMap_PreviewKeyDown);
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
