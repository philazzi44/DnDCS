namespace DnDCS.Server
{
    partial class GetImageUrlDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetImageUrlDialog));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.tboUrl = new System.Windows.Forms.TextBox();
            this.pbxPreview = new System.Windows.Forms.PictureBox();
            this.gbxPreview = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.lboHistory = new System.Windows.Forms.ListBox();
            this.gbxHistory = new System.Windows.Forms.GroupBox();
            this.gbxUrl = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).BeginInit();
            this.gbxPreview.SuspendLayout();
            this.gbxHistory.SuspendLayout();
            this.gbxUrl.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(330, 442);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(411, 442);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 9);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(252, 13);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "Enter the local or remote Url to load (must be a .png)";
            // 
            // tboUrl
            // 
            this.tboUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tboUrl.Location = new System.Drawing.Point(6, 19);
            this.tboUrl.Name = "tboUrl";
            this.tboUrl.Size = new System.Drawing.Size(432, 20);
            this.tboUrl.TabIndex = 0;
            this.tboUrl.TextChanged += new System.EventHandler(this.tboUrl_TextChanged);
            // 
            // pbxPreview
            // 
            this.pbxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxPreview.Location = new System.Drawing.Point(3, 16);
            this.pbxPreview.Name = "pbxPreview";
            this.pbxPreview.Size = new System.Drawing.Size(234, 161);
            this.pbxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxPreview.TabIndex = 4;
            this.pbxPreview.TabStop = false;
            // 
            // gbxPreview
            // 
            this.gbxPreview.Controls.Add(this.pbxPreview);
            this.gbxPreview.Location = new System.Drawing.Point(12, 256);
            this.gbxPreview.Name = "gbxPreview";
            this.gbxPreview.Size = new System.Drawing.Size(240, 180);
            this.gbxPreview.TabIndex = 3;
            this.gbxPreview.TabStop = false;
            this.gbxPreview.Text = "Preview";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(444, 17);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(25, 23);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnDelete.BackgroundImage")));
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(444, 19);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(24, 24);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // lboHistory
            // 
            this.lboHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lboHistory.FormattingEnabled = true;
            this.lboHistory.HorizontalScrollbar = true;
            this.lboHistory.Location = new System.Drawing.Point(6, 19);
            this.lboHistory.Name = "lboHistory";
            this.lboHistory.Size = new System.Drawing.Size(432, 134);
            this.lboHistory.TabIndex = 0;
            this.lboHistory.SelectedIndexChanged += new System.EventHandler(this.lboHistory_SelectedIndexChanged);
            // 
            // gbxHistory
            // 
            this.gbxHistory.Controls.Add(this.lboHistory);
            this.gbxHistory.Controls.Add(this.btnDelete);
            this.gbxHistory.Location = new System.Drawing.Point(12, 36);
            this.gbxHistory.Name = "gbxHistory";
            this.gbxHistory.Size = new System.Drawing.Size(474, 161);
            this.gbxHistory.TabIndex = 1;
            this.gbxHistory.TabStop = false;
            this.gbxHistory.Text = "History";
            // 
            // gbxUrl
            // 
            this.gbxUrl.Controls.Add(this.tboUrl);
            this.gbxUrl.Controls.Add(this.btnBrowse);
            this.gbxUrl.Location = new System.Drawing.Point(12, 203);
            this.gbxUrl.Name = "gbxUrl";
            this.gbxUrl.Size = new System.Drawing.Size(474, 47);
            this.gbxUrl.TabIndex = 2;
            this.gbxUrl.TabStop = false;
            this.gbxUrl.Text = "Image Url";
            // 
            // GetImageUrlDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(495, 470);
            this.Controls.Add(this.gbxUrl);
            this.Controls.Add(this.gbxHistory);
            this.Controls.Add(this.gbxPreview);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetImageUrlDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load Url";
            this.Load += new System.EventHandler(this.GetImageUrlDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).EndInit();
            this.gbxPreview.ResumeLayout(false);
            this.gbxHistory.ResumeLayout(false);
            this.gbxUrl.ResumeLayout(false);
            this.gbxUrl.PerformLayout();
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
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.ListBox lboHistory;
        private System.Windows.Forms.GroupBox gbxHistory;
        private System.Windows.Forms.GroupBox gbxUrl;
    }
}