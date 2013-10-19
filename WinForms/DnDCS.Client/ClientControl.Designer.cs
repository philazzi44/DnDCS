namespace DnDCS.Client
{
    partial class ClientControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ctlDnDMap = new DnDCS.WinFormsLibs.DnDSparklesPictureBox();
            this.SuspendLayout();
            // 
            // ctlDnDMap
            // 
            this.ctlDnDMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ctlDnDMap.BackColor = System.Drawing.Color.Black;
            this.ctlDnDMap.IsBlackoutOn = false;
            this.ctlDnDMap.Location = new System.Drawing.Point(0, 0);
            this.ctlDnDMap.Map = null;
            this.ctlDnDMap.Name = "ctlDnDMap";
            this.ctlDnDMap.Size = new System.Drawing.Size(634, 477);
            this.ctlDnDMap.TabIndex = 0;
            // 
            // ClientControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctlDnDMap);
            this.Name = "ClientControl";
            this.Size = new System.Drawing.Size(640, 480);
            this.Load += new System.EventHandler(this.ClientControl_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private WinFormsLibs.DnDSparklesPictureBox ctlDnDMap;



    }
}
