﻿namespace DnDCS.Server
{
    partial class ServerControl_Old
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerControl_Old));
            this.pbxMap = new System.Windows.Forms.PictureBox();
            this.spltServer = new System.Windows.Forms.SplitContainer();
            this.pnlMap = new System.Windows.Forms.Panel();
            this.flpControls = new System.Windows.Forms.FlowLayoutPanel();
            this.gbxCommands = new System.Windows.Forms.GroupBox();
            this.btnSyncFog = new System.Windows.Forms.Button();
            this.btnSelectTool = new System.Windows.Forms.Button();
            this.btnFogTool = new System.Windows.Forms.Button();
            this.btnRevealAll = new System.Windows.Forms.Button();
            this.btnFogAll = new System.Windows.Forms.Button();
            this.btnToggleBlackout = new System.Windows.Forms.Button();
            this.gbxGridSize = new System.Windows.Forms.GroupBox();
            this.chkShowGrid = new System.Windows.Forms.CheckBox();
            this.lblGridSize = new System.Windows.Forms.Label();
            this.nudGridSize = new System.Windows.Forms.NumericUpDown();
            this.gbxLog = new System.Windows.Forms.GroupBox();
            this.tboLog = new System.Windows.Forms.RichTextBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnLoadImage = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spltServer)).BeginInit();
            this.spltServer.Panel1.SuspendLayout();
            this.spltServer.Panel2.SuspendLayout();
            this.spltServer.SuspendLayout();
            this.pnlMap.SuspendLayout();
            this.flpControls.SuspendLayout();
            this.gbxCommands.SuspendLayout();
            this.gbxGridSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGridSize)).BeginInit();
            this.gbxLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbxMap
            // 
            this.pbxMap.Location = new System.Drawing.Point(0, 0);
            this.pbxMap.Name = "pbxMap";
            this.pbxMap.Size = new System.Drawing.Size(320, 240);
            this.pbxMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbxMap.TabIndex = 0;
            this.pbxMap.TabStop = false;
            // 
            // spltServer
            // 
            this.spltServer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spltServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltServer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.spltServer.Location = new System.Drawing.Point(0, 0);
            this.spltServer.Name = "spltServer";
            // 
            // spltServer.Panel1
            // 
            this.spltServer.Panel1.Controls.Add(this.pnlMap);
            this.spltServer.Panel1MinSize = 400;
            // 
            // spltServer.Panel2
            // 
            this.spltServer.Panel2.Controls.Add(this.flpControls);
            this.spltServer.Panel2MinSize = 125;
            this.spltServer.Size = new System.Drawing.Size(804, 489);
            this.spltServer.SplitterDistance = 675;
            this.spltServer.TabIndex = 1;
            // 
            // pnlMap
            // 
            this.pnlMap.AutoScroll = true;
            this.pnlMap.Controls.Add(this.pbxMap);
            this.pnlMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMap.Location = new System.Drawing.Point(0, 0);
            this.pnlMap.Name = "pnlMap";
            this.pnlMap.Size = new System.Drawing.Size(671, 485);
            this.pnlMap.TabIndex = 0;
            this.pnlMap.SizeChanged += new System.EventHandler(this.pnlMap_SizeChanged);
            // 
            // flpControls
            // 
            this.flpControls.Controls.Add(this.gbxCommands);
            this.flpControls.Controls.Add(this.gbxGridSize);
            this.flpControls.Controls.Add(this.gbxLog);
            this.flpControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpControls.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpControls.Location = new System.Drawing.Point(0, 0);
            this.flpControls.Name = "flpControls";
            this.flpControls.Size = new System.Drawing.Size(121, 485);
            this.flpControls.TabIndex = 0;
            this.flpControls.WrapContents = false;
            this.flpControls.SizeChanged += new System.EventHandler(this.flpControls_SizeChanged);
            // 
            // gbxCommands
            // 
            this.gbxCommands.Controls.Add(this.btnSyncFog);
            this.gbxCommands.Controls.Add(this.btnSelectTool);
            this.gbxCommands.Controls.Add(this.btnFogTool);
            this.gbxCommands.Controls.Add(this.btnLoadImage);
            this.gbxCommands.Controls.Add(this.btnRevealAll);
            this.gbxCommands.Controls.Add(this.btnFogAll);
            this.gbxCommands.Controls.Add(this.btnToggleBlackout);
            this.gbxCommands.Enabled = false;
            this.gbxCommands.Location = new System.Drawing.Point(3, 3);
            this.gbxCommands.Name = "gbxCommands";
            this.gbxCommands.Size = new System.Drawing.Size(115, 260);
            this.gbxCommands.TabIndex = 1;
            this.gbxCommands.TabStop = false;
            this.gbxCommands.Text = "Commands";
            // 
            // btnSyncFog
            // 
            this.btnSyncFog.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSyncFog.BackgroundImage")));
            this.btnSyncFog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSyncFog.Location = new System.Drawing.Point(61, 98);
            this.btnSyncFog.Name = "btnSyncFog";
            this.btnSyncFog.Size = new System.Drawing.Size(48, 48);
            this.btnSyncFog.TabIndex = 2;
            this.btnSyncFog.UseVisualStyleBackColor = true;
            this.btnSyncFog.Click += new System.EventHandler(this.btnSyncFog_Click);
            // 
            // btnSelectTool
            // 
            this.btnSelectTool.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSelectTool.BackgroundImage")));
            this.btnSelectTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSelectTool.Location = new System.Drawing.Point(6, 19);
            this.btnSelectTool.Name = "btnSelectTool";
            this.btnSelectTool.Size = new System.Drawing.Size(48, 48);
            this.btnSelectTool.TabIndex = 0;
            this.btnSelectTool.UseVisualStyleBackColor = true;
            this.btnSelectTool.Click += new System.EventHandler(this.btnSelectTool_Click);
            // 
            // btnFogTool
            // 
            this.btnFogTool.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFogTool.BackgroundImage")));
            this.btnFogTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFogTool.Location = new System.Drawing.Point(6, 98);
            this.btnFogTool.Name = "btnFogTool";
            this.btnFogTool.Size = new System.Drawing.Size(48, 48);
            this.btnFogTool.TabIndex = 1;
            this.btnFogTool.UseVisualStyleBackColor = true;
            this.btnFogTool.Click += new System.EventHandler(this.btnFogTool_Click);
            // 
            // btnRevealAll
            // 
            this.btnRevealAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRevealAll.BackgroundImage")));
            this.btnRevealAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRevealAll.Location = new System.Drawing.Point(60, 152);
            this.btnRevealAll.Name = "btnRevealAll";
            this.btnRevealAll.Size = new System.Drawing.Size(48, 48);
            this.btnRevealAll.TabIndex = 4;
            this.btnRevealAll.TabStop = false;
            this.btnRevealAll.UseVisualStyleBackColor = true;
            this.btnRevealAll.Click += new System.EventHandler(this.btnRevealAll_Click);
            // 
            // btnFogAll
            // 
            this.btnFogAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFogAll.BackgroundImage")));
            this.btnFogAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFogAll.Location = new System.Drawing.Point(6, 152);
            this.btnFogAll.Name = "btnFogAll";
            this.btnFogAll.Size = new System.Drawing.Size(48, 48);
            this.btnFogAll.TabIndex = 3;
            this.btnFogAll.TabStop = false;
            this.btnFogAll.UseVisualStyleBackColor = true;
            this.btnFogAll.Click += new System.EventHandler(this.btnFogAll_Click);
            // 
            // btnToggleBlackout
            // 
            this.btnToggleBlackout.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnToggleBlackout.BackgroundImage")));
            this.btnToggleBlackout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnToggleBlackout.Location = new System.Drawing.Point(61, 19);
            this.btnToggleBlackout.Name = "btnToggleBlackout";
            this.btnToggleBlackout.Size = new System.Drawing.Size(48, 48);
            this.btnToggleBlackout.TabIndex = 0;
            this.btnToggleBlackout.TabStop = false;
            this.btnToggleBlackout.UseVisualStyleBackColor = false;
            this.btnToggleBlackout.Click += new System.EventHandler(this.btnToggleBlackout_Click);
            // 
            // gbxGridSize
            // 
            this.gbxGridSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxGridSize.Controls.Add(this.chkShowGrid);
            this.gbxGridSize.Controls.Add(this.lblGridSize);
            this.gbxGridSize.Controls.Add(this.nudGridSize);
            this.gbxGridSize.Location = new System.Drawing.Point(3, 269);
            this.gbxGridSize.Name = "gbxGridSize";
            this.gbxGridSize.Size = new System.Drawing.Size(115, 47);
            this.gbxGridSize.TabIndex = 2;
            this.gbxGridSize.TabStop = false;
            this.gbxGridSize.Text = "Grid Size";
            // 
            // chkShowGrid
            // 
            this.chkShowGrid.AutoSize = true;
            this.chkShowGrid.Location = new System.Drawing.Point(10, 21);
            this.chkShowGrid.Name = "chkShowGrid";
            this.chkShowGrid.Size = new System.Drawing.Size(15, 14);
            this.chkShowGrid.TabIndex = 0;
            this.chkShowGrid.UseVisualStyleBackColor = true;
            this.chkShowGrid.CheckedChanged += new System.EventHandler(this.chkShowGrid_CheckedChanged);
            // 
            // lblGridSize
            // 
            this.lblGridSize.AutoSize = true;
            this.lblGridSize.Enabled = false;
            this.lblGridSize.Location = new System.Drawing.Point(31, 22);
            this.lblGridSize.Name = "lblGridSize";
            this.lblGridSize.Size = new System.Drawing.Size(30, 13);
            this.lblGridSize.TabIndex = 0;
            this.lblGridSize.Text = "Size:";
            // 
            // nudGridSize
            // 
            this.nudGridSize.Enabled = false;
            this.nudGridSize.Location = new System.Drawing.Point(67, 19);
            this.nudGridSize.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.nudGridSize.Name = "nudGridSize";
            this.nudGridSize.Size = new System.Drawing.Size(42, 20);
            this.nudGridSize.TabIndex = 0;
            this.nudGridSize.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.nudGridSize.ValueChanged += new System.EventHandler(this.nudGridSize_ValueChanged);
            this.nudGridSize.Leave += new System.EventHandler(this.nudGridSize_Leave);
            // 
            // gbxLog
            // 
            this.gbxLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxLog.Controls.Add(this.tboLog);
            this.gbxLog.Controls.Add(this.btnClearLog);
            this.gbxLog.Location = new System.Drawing.Point(3, 322);
            this.gbxLog.Name = "gbxLog";
            this.gbxLog.Size = new System.Drawing.Size(115, 160);
            this.gbxLog.TabIndex = 0;
            this.gbxLog.TabStop = false;
            this.gbxLog.Text = "Log";
            // 
            // tboLog
            // 
            this.tboLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tboLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tboLog.Location = new System.Drawing.Point(6, 51);
            this.tboLog.Name = "tboLog";
            this.tboLog.ReadOnly = true;
            this.tboLog.Size = new System.Drawing.Size(103, 103);
            this.tboLog.TabIndex = 1;
            this.tboLog.TabStop = false;
            this.tboLog.Text = "";
            this.tboLog.WordWrap = false;
            this.tboLog.TextChanged += new System.EventHandler(this.tboLog_TextChanged);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearLog.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnClearLog.BackgroundImage")));
            this.btnClearLog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClearLog.Enabled = false;
            this.btnClearLog.Location = new System.Drawing.Point(83, 19);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(26, 26);
            this.btnClearLog.TabIndex = 0;
            this.btnClearLog.TabStop = false;
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLoadImage.BackgroundImage")));
            this.btnLoadImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLoadImage.Location = new System.Drawing.Point(60, 206);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(48, 48);
            this.btnLoadImage.TabIndex = 4;
            this.btnLoadImage.TabStop = false;
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.btnLoadImage_Click);
            // 
            // ServerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.spltServer);
            this.Name = "ServerControl";
            this.Size = new System.Drawing.Size(804, 489);
            this.Load += new System.EventHandler(this.ServerControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).EndInit();
            this.spltServer.Panel1.ResumeLayout(false);
            this.spltServer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltServer)).EndInit();
            this.spltServer.ResumeLayout(false);
            this.pnlMap.ResumeLayout(false);
            this.pnlMap.PerformLayout();
            this.flpControls.ResumeLayout(false);
            this.gbxCommands.ResumeLayout(false);
            this.gbxGridSize.ResumeLayout(false);
            this.gbxGridSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGridSize)).EndInit();
            this.gbxLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbxMap;
        private System.Windows.Forms.SplitContainer spltServer;
        private System.Windows.Forms.GroupBox gbxCommands;
        private System.Windows.Forms.Button btnSelectTool;
        private System.Windows.Forms.Button btnFogTool;
        private System.Windows.Forms.Button btnToggleBlackout;
        private System.Windows.Forms.Panel pnlMap;
        private System.Windows.Forms.Button btnFogAll;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.RichTextBox tboLog;
        private System.Windows.Forms.GroupBox gbxLog;
        private System.Windows.Forms.Button btnSyncFog;
        private System.Windows.Forms.GroupBox gbxGridSize;
        private System.Windows.Forms.FlowLayoutPanel flpControls;
        private System.Windows.Forms.Label lblGridSize;
        private System.Windows.Forms.NumericUpDown nudGridSize;
        private System.Windows.Forms.CheckBox chkShowGrid;
        private System.Windows.Forms.Button btnRevealAll;
        private System.Windows.Forms.Button btnLoadImage;
    }
}
