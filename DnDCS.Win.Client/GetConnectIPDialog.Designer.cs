﻿namespace DnDCS.Win.Client
{
    partial class GetConnectIPDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetConnectIPDialog));
            this.tboIP1 = new System.Windows.Forms.TextBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnPing = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.tboIP2 = new System.Windows.Forms.TextBox();
            this.tboIP3 = new System.Windows.Forms.TextBox();
            this.tboIP4 = new System.Windows.Forms.TextBox();
            this.rdoIP = new System.Windows.Forms.RadioButton();
            this.rdoName = new System.Windows.Forms.RadioButton();
            this.tboName = new System.Windows.Forms.TextBox();
            this.pnlIP = new System.Windows.Forms.Panel();
            this.pnlName = new System.Windows.Forms.Panel();
            this.flpConnect = new System.Windows.Forms.FlowLayoutPanel();
            this.tboPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.lboHistory = new System.Windows.Forms.ListBox();
            this.gbxAddress = new System.Windows.Forms.GroupBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.gbxHistory = new System.Windows.Forms.GroupBox();
            this.pnlIP.SuspendLayout();
            this.pnlName.SuspendLayout();
            this.flpConnect.SuspendLayout();
            this.gbxAddress.SuspendLayout();
            this.gbxHistory.SuspendLayout();
            this.SuspendLayout();
            // 
            // tboIP1
            // 
            this.tboIP1.Location = new System.Drawing.Point(3, 4);
            this.tboIP1.Name = "tboIP1";
            this.tboIP1.Size = new System.Drawing.Size(30, 20);
            this.tboIP1.TabIndex = 0;
            this.tboIP1.Text = "127";
            this.tboIP1.TextChanged += new System.EventHandler(this.tboIP1_TextChanged);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 9);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(219, 13);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "Enter the Machine Name or IP to connect to.";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(217, 334);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnPing
            // 
            this.btnPing.Enabled = false;
            this.btnPing.Location = new System.Drawing.Point(12, 334);
            this.btnPing.Name = "btnPing";
            this.btnPing.Size = new System.Drawing.Size(49, 23);
            this.btnPing.TabIndex = 3;
            this.btnPing.Text = "Ping";
            this.btnPing.UseVisualStyleBackColor = true;
            this.btnPing.Visible = false;
            // 
            // btnConnect
            // 
            this.btnConnect.Enabled = false;
            this.btnConnect.Location = new System.Drawing.Point(63, 334);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tboIP2
            // 
            this.tboIP2.Location = new System.Drawing.Point(39, 4);
            this.tboIP2.Name = "tboIP2";
            this.tboIP2.Size = new System.Drawing.Size(30, 20);
            this.tboIP2.TabIndex = 1;
            this.tboIP2.TextChanged += new System.EventHandler(this.tboIP2_TextChanged);
            // 
            // tboIP3
            // 
            this.tboIP3.Location = new System.Drawing.Point(75, 4);
            this.tboIP3.Name = "tboIP3";
            this.tboIP3.Size = new System.Drawing.Size(30, 20);
            this.tboIP3.TabIndex = 2;
            this.tboIP3.TextChanged += new System.EventHandler(this.tboIP3_TextChanged);
            // 
            // tboIP4
            // 
            this.tboIP4.Location = new System.Drawing.Point(111, 4);
            this.tboIP4.Name = "tboIP4";
            this.tboIP4.Size = new System.Drawing.Size(30, 20);
            this.tboIP4.TabIndex = 3;
            this.tboIP4.TextChanged += new System.EventHandler(this.tboIP4_TextChanged);
            // 
            // rdoIP
            // 
            this.rdoIP.AutoSize = true;
            this.rdoIP.Location = new System.Drawing.Point(65, 16);
            this.rdoIP.Name = "rdoIP";
            this.rdoIP.Size = new System.Drawing.Size(35, 17);
            this.rdoIP.TabIndex = 1;
            this.rdoIP.Text = "IP";
            this.rdoIP.UseVisualStyleBackColor = true;
            this.rdoIP.CheckedChanged += new System.EventHandler(this.rdoIP_CheckedChanged);
            // 
            // rdoName
            // 
            this.rdoName.AutoSize = true;
            this.rdoName.Checked = true;
            this.rdoName.Location = new System.Drawing.Point(6, 16);
            this.rdoName.Name = "rdoName";
            this.rdoName.Size = new System.Drawing.Size(53, 17);
            this.rdoName.TabIndex = 0;
            this.rdoName.TabStop = true;
            this.rdoName.Text = "Name";
            this.rdoName.UseVisualStyleBackColor = true;
            this.rdoName.CheckedChanged += new System.EventHandler(this.rdoName_CheckedChanged);
            // 
            // tboName
            // 
            this.tboName.Location = new System.Drawing.Point(3, 3);
            this.tboName.Name = "tboName";
            this.tboName.Size = new System.Drawing.Size(138, 20);
            this.tboName.TabIndex = 0;
            this.tboName.TextChanged += new System.EventHandler(this.tboName_TextChanged);
            // 
            // pnlIP
            // 
            this.pnlIP.Controls.Add(this.tboIP1);
            this.pnlIP.Controls.Add(this.tboIP2);
            this.pnlIP.Controls.Add(this.tboIP3);
            this.pnlIP.Controls.Add(this.tboIP4);
            this.pnlIP.Location = new System.Drawing.Point(3, 36);
            this.pnlIP.Name = "pnlIP";
            this.pnlIP.Size = new System.Drawing.Size(148, 26);
            this.pnlIP.TabIndex = 1;
            this.pnlIP.Visible = false;
            // 
            // pnlName
            // 
            this.pnlName.Controls.Add(this.tboName);
            this.pnlName.Location = new System.Drawing.Point(3, 3);
            this.pnlName.Name = "pnlName";
            this.pnlName.Size = new System.Drawing.Size(148, 27);
            this.pnlName.TabIndex = 0;
            // 
            // flpConnect
            // 
            this.flpConnect.Controls.Add(this.pnlName);
            this.flpConnect.Controls.Add(this.pnlIP);
            this.flpConnect.Location = new System.Drawing.Point(6, 51);
            this.flpConnect.Name = "flpConnect";
            this.flpConnect.Size = new System.Drawing.Size(158, 67);
            this.flpConnect.TabIndex = 2;
            // 
            // tboPort
            // 
            this.tboPort.Location = new System.Drawing.Point(205, 57);
            this.tboPort.Name = "tboPort";
            this.tboPort.Size = new System.Drawing.Size(69, 20);
            this.tboPort.TabIndex = 3;
            this.tboPort.TextChanged += new System.EventHandler(this.tboPort_TextChanged);
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(170, 60);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 13);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "Port:";
            // 
            // lboHistory
            // 
            this.lboHistory.FormattingEnabled = true;
            this.lboHistory.Location = new System.Drawing.Point(6, 16);
            this.lboHistory.Name = "lboHistory";
            this.lboHistory.Size = new System.Drawing.Size(235, 134);
            this.lboHistory.TabIndex = 0;
            this.lboHistory.SelectedIndexChanged += new System.EventHandler(this.lboHistory_SelectedIndexChanged);
            // 
            // gbxAddress
            // 
            this.gbxAddress.Controls.Add(this.flpConnect);
            this.gbxAddress.Controls.Add(this.lblPort);
            this.gbxAddress.Controls.Add(this.rdoIP);
            this.gbxAddress.Controls.Add(this.rdoName);
            this.gbxAddress.Controls.Add(this.tboPort);
            this.gbxAddress.Location = new System.Drawing.Point(12, 197);
            this.gbxAddress.Name = "gbxAddress";
            this.gbxAddress.Size = new System.Drawing.Size(280, 124);
            this.gbxAddress.TabIndex = 2;
            this.gbxAddress.TabStop = false;
            this.gbxAddress.Text = "Address";
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnDelete.BackgroundImage")));
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(247, 16);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(24, 24);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // gbxHistory
            // 
            this.gbxHistory.Controls.Add(this.lboHistory);
            this.gbxHistory.Controls.Add(this.btnDelete);
            this.gbxHistory.Location = new System.Drawing.Point(12, 32);
            this.gbxHistory.Name = "gbxHistory";
            this.gbxHistory.Size = new System.Drawing.Size(277, 157);
            this.gbxHistory.TabIndex = 1;
            this.gbxHistory.TabStop = false;
            this.gbxHistory.Text = "History";
            // 
            // GetConnectIPDialog
            // 
            this.AcceptButton = this.btnConnect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(296, 360);
            this.Controls.Add(this.gbxHistory);
            this.Controls.Add(this.gbxAddress);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPing);
            this.Controls.Add(this.btnConnect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetConnectIPDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connect to IP";
            this.Load += new System.EventHandler(this.GetConnectIPDialog_Load);
            this.pnlIP.ResumeLayout(false);
            this.pnlIP.PerformLayout();
            this.pnlName.ResumeLayout(false);
            this.pnlName.PerformLayout();
            this.flpConnect.ResumeLayout(false);
            this.gbxAddress.ResumeLayout(false);
            this.gbxAddress.PerformLayout();
            this.gbxHistory.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tboIP1;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnPing;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox tboIP2;
        private System.Windows.Forms.TextBox tboIP3;
        private System.Windows.Forms.TextBox tboIP4;
        private System.Windows.Forms.RadioButton rdoIP;
        private System.Windows.Forms.RadioButton rdoName;
        private System.Windows.Forms.TextBox tboName;
        private System.Windows.Forms.Panel pnlIP;
        private System.Windows.Forms.Panel pnlName;
        private System.Windows.Forms.FlowLayoutPanel flpConnect;
        private System.Windows.Forms.TextBox tboPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ListBox lboHistory;
        private System.Windows.Forms.GroupBox gbxAddress;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.GroupBox gbxHistory;

    }
}