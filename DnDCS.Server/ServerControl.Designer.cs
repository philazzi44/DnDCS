namespace DnDCS.Server
{
    partial class ServerControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerControl));
            this.pbxMap = new System.Windows.Forms.PictureBox();
            this.spltServer = new System.Windows.Forms.SplitContainer();
            this.pnlMap = new System.Windows.Forms.Panel();
            this.gbxCommands = new System.Windows.Forms.GroupBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnSelectTool = new System.Windows.Forms.Button();
            this.tboLog = new System.Windows.Forms.RichTextBox();
            this.btnFogTool = new System.Windows.Forms.Button();
            this.btnFogAll = new System.Windows.Forms.Button();
            this.btnToggleBlackout = new System.Windows.Forms.Button();
            this.gbxLog = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spltServer)).BeginInit();
            this.spltServer.Panel1.SuspendLayout();
            this.spltServer.Panel2.SuspendLayout();
            this.spltServer.SuspendLayout();
            this.pnlMap.SuspendLayout();
            this.gbxCommands.SuspendLayout();
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
            this.spltServer.Panel2.Controls.Add(this.gbxLog);
            this.spltServer.Panel2.Controls.Add(this.gbxCommands);
            this.spltServer.Panel2MinSize = 125;
            this.spltServer.Size = new System.Drawing.Size(804, 490);
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
            this.pnlMap.Size = new System.Drawing.Size(671, 486);
            this.pnlMap.TabIndex = 0;
            // 
            // gbxCommands
            // 
            this.gbxCommands.Controls.Add(this.btnSelectTool);
            this.gbxCommands.Controls.Add(this.btnFogTool);
            this.gbxCommands.Controls.Add(this.btnFogAll);
            this.gbxCommands.Controls.Add(this.btnToggleBlackout);
            this.gbxCommands.Enabled = false;
            this.gbxCommands.Location = new System.Drawing.Point(3, 3);
            this.gbxCommands.Name = "gbxCommands";
            this.gbxCommands.Size = new System.Drawing.Size(115, 208);
            this.gbxCommands.TabIndex = 0;
            this.gbxCommands.TabStop = false;
            this.gbxCommands.Text = "Commands";
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
            // btnSelectTool
            // 
            this.btnSelectTool.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSelectTool.BackgroundImage")));
            this.btnSelectTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSelectTool.Location = new System.Drawing.Point(6, 98);
            this.btnSelectTool.Name = "btnSelectTool";
            this.btnSelectTool.Size = new System.Drawing.Size(48, 48);
            this.btnSelectTool.TabIndex = 0;
            this.btnSelectTool.UseVisualStyleBackColor = true;
            this.btnSelectTool.Click += new System.EventHandler(this.btnSelectTool_Click);
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
            this.tboLog.Size = new System.Drawing.Size(103, 209);
            this.tboLog.TabIndex = 0;
            this.tboLog.TabStop = false;
            this.tboLog.Text = "";
            this.tboLog.WordWrap = false;
            this.tboLog.TextChanged += new System.EventHandler(this.tboLog_TextChanged);
            // 
            // btnFogTool
            // 
            this.btnFogTool.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFogTool.BackgroundImage")));
            this.btnFogTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFogTool.Location = new System.Drawing.Point(60, 98);
            this.btnFogTool.Name = "btnFogTool";
            this.btnFogTool.Size = new System.Drawing.Size(48, 48);
            this.btnFogTool.TabIndex = 1;
            this.btnFogTool.UseVisualStyleBackColor = true;
            this.btnFogTool.Click += new System.EventHandler(this.btnFogTool_Click);
            // 
            // btnFogAll
            // 
            this.btnFogAll.Location = new System.Drawing.Point(6, 152);
            this.btnFogAll.Name = "btnFogAll";
            this.btnFogAll.Size = new System.Drawing.Size(48, 48);
            this.btnFogAll.TabIndex = 0;
            this.btnFogAll.TabStop = false;
            this.btnFogAll.Text = "Fog All";
            this.btnFogAll.UseVisualStyleBackColor = true;
            this.btnFogAll.Click += new System.EventHandler(this.btnFogAll_Click);
            // 
            // btnToggleBlackout
            // 
            this.btnToggleBlackout.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnToggleBlackout.BackgroundImage")));
            this.btnToggleBlackout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnToggleBlackout.Location = new System.Drawing.Point(65, 19);
            this.btnToggleBlackout.Name = "btnToggleBlackout";
            this.btnToggleBlackout.Size = new System.Drawing.Size(48, 48);
            this.btnToggleBlackout.TabIndex = 0;
            this.btnToggleBlackout.TabStop = false;
            this.btnToggleBlackout.UseVisualStyleBackColor = false;
            this.btnToggleBlackout.Click += new System.EventHandler(this.btnToggleBlackout_Click);
            // 
            // gbxLog
            // 
            this.gbxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxLog.Controls.Add(this.tboLog);
            this.gbxLog.Controls.Add(this.btnClearLog);
            this.gbxLog.Location = new System.Drawing.Point(3, 217);
            this.gbxLog.Name = "gbxLog";
            this.gbxLog.Size = new System.Drawing.Size(115, 266);
            this.gbxLog.TabIndex = 0;
            this.gbxLog.TabStop = false;
            this.gbxLog.Text = "Log";
            // 
            // ServerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.spltServer);
            this.Name = "ServerControl";
            this.Size = new System.Drawing.Size(804, 490);
            this.Load += new System.EventHandler(this.ServerControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).EndInit();
            this.spltServer.Panel1.ResumeLayout(false);
            this.spltServer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltServer)).EndInit();
            this.spltServer.ResumeLayout(false);
            this.pnlMap.ResumeLayout(false);
            this.pnlMap.PerformLayout();
            this.gbxCommands.ResumeLayout(false);
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
    }
}
