namespace DnDCS.Server
{
    partial class GetUrlDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.tboUrl = new System.Windows.Forms.TextBox();
            this.pbxPreview = new System.Windows.Forms.PictureBox();
            this.gbxPreview = new System.Windows.Forms.GroupBox();
            this.btnLoad = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).BeginInit();
            this.gbxPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(392, 366);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(473, 366);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 9);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(181, 13);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "Enter the Url to load (Must be a .png)";
            // 
            // tboUrl
            // 
            this.tboUrl.Location = new System.Drawing.Point(15, 37);
            this.tboUrl.Name = "tboUrl";
            this.tboUrl.Size = new System.Drawing.Size(488, 20);
            this.tboUrl.TabIndex = 0;
            this.tboUrl.TextChanged += new System.EventHandler(this.tboUrl_TextChanged);
            // 
            // pbxPreview
            // 
            this.pbxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxPreview.Location = new System.Drawing.Point(3, 16);
            this.pbxPreview.Name = "pbxPreview";
            this.pbxPreview.Size = new System.Drawing.Size(527, 269);
            this.pbxPreview.TabIndex = 4;
            this.pbxPreview.TabStop = false;
            // 
            // gbxPreview
            // 
            this.gbxPreview.Controls.Add(this.pbxPreview);
            this.gbxPreview.Location = new System.Drawing.Point(15, 72);
            this.gbxPreview.Name = "gbxPreview";
            this.gbxPreview.Size = new System.Drawing.Size(533, 288);
            this.gbxPreview.TabIndex = 0;
            this.gbxPreview.TabStop = false;
            this.gbxPreview.Text = "Preview";
            // 
            // btnLoad
            // 
            this.btnLoad.Enabled = false;
            this.btnLoad.Location = new System.Drawing.Point(509, 34);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(39, 23);
            this.btnLoad.TabIndex = 1;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // GetUrlDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(555, 401);
            this.Controls.Add(this.gbxPreview);
            this.Controls.Add(this.tboUrl);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetUrlDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load Url";
            ((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).EndInit();
            this.gbxPreview.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TextBox tboUrl;
        private System.Windows.Forms.PictureBox pbxPreview;
        private System.Windows.Forms.GroupBox gbxPreview;
        private System.Windows.Forms.Button btnLoad;
    }
}