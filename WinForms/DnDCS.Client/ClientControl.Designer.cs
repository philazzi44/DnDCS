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
            this.pbxMap = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).BeginInit();
            this.SuspendLayout();
            // 
            // pbxMap
            // 
            this.pbxMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxMap.Location = new System.Drawing.Point(0, 0);
            this.pbxMap.Name = "pbxMap";
            this.pbxMap.Size = new System.Drawing.Size(640, 480);
            this.pbxMap.TabIndex = 1;
            this.pbxMap.TabStop = false;
            this.pbxMap.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseClick);
            this.pbxMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseDown);
            this.pbxMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseMove);
            this.pbxMap.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseUp);
            // 
            // ClientControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbxMap);
            this.Name = "ClientControl";
            this.Size = new System.Drawing.Size(640, 480);
            this.Load += new System.EventHandler(this.ClientControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbxMap;
    }
}
