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
            this.pnlMap = new System.Windows.Forms.Panel();
            this.pbxMap = new System.Windows.Forms.PictureBox();
            this.pnlMap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMap
            // 
            this.pnlMap.Controls.Add(this.pbxMap);
            this.pnlMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMap.Location = new System.Drawing.Point(0, 0);
            this.pnlMap.Name = "pnlMap";
            this.pnlMap.Size = new System.Drawing.Size(659, 384);
            this.pnlMap.TabIndex = 0;
            // 
            // pbxMap
            // 
            this.pbxMap.Location = new System.Drawing.Point(0, 0);
            this.pbxMap.Name = "pbxMap";
            this.pbxMap.Size = new System.Drawing.Size(320, 240);
            this.pbxMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbxMap.TabIndex = 1;
            this.pbxMap.TabStop = false;
            // 
            // ClientControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMap);
            this.Name = "ClientControl";
            this.Size = new System.Drawing.Size(659, 384);
            this.pnlMap.ResumeLayout(false);
            this.pnlMap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMap;
        private System.Windows.Forms.PictureBox pbxMap;
    }
}
