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
            this.btnSelectTool = new System.Windows.Forms.Button();
            this.btnFogTool = new System.Windows.Forms.Button();
            this.btnFogAll = new System.Windows.Forms.Button();
            this.btnToggleBlackout = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbxMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spltServer)).BeginInit();
            this.spltServer.Panel1.SuspendLayout();
            this.spltServer.Panel2.SuspendLayout();
            this.spltServer.SuspendLayout();
            this.pnlMap.SuspendLayout();
            this.gbxCommands.SuspendLayout();
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
            this.spltServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltServer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.spltServer.IsSplitterFixed = true;
            this.spltServer.Location = new System.Drawing.Point(0, 0);
            this.spltServer.Name = "spltServer";
            // 
            // spltServer.Panel1
            // 
            this.spltServer.Panel1.Controls.Add(this.pnlMap);
            // 
            // spltServer.Panel2
            // 
            this.spltServer.Panel2.Controls.Add(this.gbxCommands);
            this.spltServer.Size = new System.Drawing.Size(804, 490);
            this.spltServer.SplitterDistance = 699;
            this.spltServer.TabIndex = 1;
            // 
            // pnlMap
            // 
            this.pnlMap.AutoScroll = true;
            this.pnlMap.Controls.Add(this.pbxMap);
            this.pnlMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMap.Location = new System.Drawing.Point(0, 0);
            this.pnlMap.Name = "pnlMap";
            this.pnlMap.Size = new System.Drawing.Size(699, 490);
            this.pnlMap.TabIndex = 0;
            // 
            // gbxCommands
            // 
            this.gbxCommands.Controls.Add(this.btnSelectTool);
            this.gbxCommands.Controls.Add(this.btnFogTool);
            this.gbxCommands.Controls.Add(this.btnFogAll);
            this.gbxCommands.Controls.Add(this.btnToggleBlackout);
            this.gbxCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxCommands.Enabled = false;
            this.gbxCommands.Location = new System.Drawing.Point(0, 0);
            this.gbxCommands.Name = "gbxCommands";
            this.gbxCommands.Size = new System.Drawing.Size(101, 490);
            this.gbxCommands.TabIndex = 0;
            this.gbxCommands.TabStop = false;
            this.gbxCommands.Text = "Commands";
            // 
            // btnSelectTool
            // 
            this.btnSelectTool.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSelectTool.BackgroundImage")));
            this.btnSelectTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSelectTool.Location = new System.Drawing.Point(6, 19);
            this.btnSelectTool.Name = "btnSelectTool";
            this.btnSelectTool.Size = new System.Drawing.Size(32, 32);
            this.btnSelectTool.TabIndex = 0;
            this.btnSelectTool.UseVisualStyleBackColor = true;
            this.btnSelectTool.Click += new System.EventHandler(this.btnSelectTool_Click);
            // 
            // btnFogTool
            // 
            this.btnFogTool.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFogTool.BackgroundImage")));
            this.btnFogTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFogTool.Location = new System.Drawing.Point(6, 57);
            this.btnFogTool.Name = "btnFogTool";
            this.btnFogTool.Size = new System.Drawing.Size(32, 32);
            this.btnFogTool.TabIndex = 1;
            this.btnFogTool.UseVisualStyleBackColor = true;
            this.btnFogTool.Click += new System.EventHandler(this.btnFogTool_Click);
            // 
            // btnFogAll
            // 
            this.btnFogAll.Location = new System.Drawing.Point(6, 227);
            this.btnFogAll.Name = "btnFogAll";
            this.btnFogAll.Size = new System.Drawing.Size(89, 23);
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
            this.btnToggleBlackout.Location = new System.Drawing.Point(6, 125);
            this.btnToggleBlackout.Name = "btnToggleBlackout";
            this.btnToggleBlackout.Size = new System.Drawing.Size(48, 48);
            this.btnToggleBlackout.TabIndex = 0;
            this.btnToggleBlackout.TabStop = false;
            this.btnToggleBlackout.UseVisualStyleBackColor = false;
            this.btnToggleBlackout.Click += new System.EventHandler(this.btnToggleBlackout_Click);
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
    }
}
