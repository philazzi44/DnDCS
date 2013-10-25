namespace DnDCS.WinFormsLibs
{
    partial class DnDServerControlPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DnDServerControlPanel));
            this.spltControls = new System.Windows.Forms.SplitContainer();
            this.flpControls = new System.Windows.Forms.FlowLayoutPanel();
            this.gbxCommands = new System.Windows.Forms.GroupBox();
            this.btnFogAddTool = new System.Windows.Forms.Button();
            this.btnSyncFog = new System.Windows.Forms.Button();
            this.btnSelectTool = new System.Windows.Forms.Button();
            this.btnFogRemoveTool = new System.Windows.Forms.Button();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnRevealAll = new System.Windows.Forms.Button();
            this.btnFogAll = new System.Windows.Forms.Button();
            this.btnToggleBlackout = new System.Windows.Forms.Button();
            this.gbxGridSize = new System.Windows.Forms.GroupBox();
            this.lblGridSize = new System.Windows.Forms.Label();
            this.chkShowGrid = new System.Windows.Forms.CheckBox();
            this.nudGridSize = new System.Windows.Forms.NumericUpDown();
            this.gbxLog = new System.Windows.Forms.GroupBox();
            this.tboLog = new System.Windows.Forms.RichTextBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.ctlMiniMap = new DnDCS.WinFormsLibs.DnDMiniMap();
            ((System.ComponentModel.ISupportInitialize)(this.spltControls)).BeginInit();
            this.spltControls.Panel1.SuspendLayout();
            this.spltControls.Panel2.SuspendLayout();
            this.spltControls.SuspendLayout();
            this.flpControls.SuspendLayout();
            this.gbxCommands.SuspendLayout();
            this.gbxGridSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGridSize)).BeginInit();
            this.gbxLog.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // spltControls
            // 
            this.spltControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltControls.Location = new System.Drawing.Point(0, 0);
            this.spltControls.Name = "spltControls";
            this.spltControls.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spltControls.Panel1
            // 
            this.spltControls.Panel1.Controls.Add(this.flpControls);
            // 
            // spltControls.Panel2
            // 
            this.spltControls.Panel2.Controls.Add(this.pnlBottom);
            this.spltControls.Panel2MinSize = 125;
            this.spltControls.Size = new System.Drawing.Size(125, 500);
            this.spltControls.SplitterDistance = 371;
            this.spltControls.TabIndex = 0;
            // 
            // flpControls
            // 
            this.flpControls.AutoScroll = true;
            this.flpControls.Controls.Add(this.gbxCommands);
            this.flpControls.Controls.Add(this.gbxGridSize);
            this.flpControls.Controls.Add(this.gbxLog);
            this.flpControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpControls.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpControls.Location = new System.Drawing.Point(0, 0);
            this.flpControls.Name = "flpControls";
            this.flpControls.Size = new System.Drawing.Size(125, 371);
            this.flpControls.TabIndex = 1;
            this.flpControls.WrapContents = false;
            this.flpControls.SizeChanged += new System.EventHandler(this.flpControls_SizeChanged);
            // 
            // gbxCommands
            // 
            this.gbxCommands.Controls.Add(this.btnFogAddTool);
            this.gbxCommands.Controls.Add(this.btnSyncFog);
            this.gbxCommands.Controls.Add(this.btnSelectTool);
            this.gbxCommands.Controls.Add(this.btnFogRemoveTool);
            this.gbxCommands.Controls.Add(this.btnLoadImage);
            this.gbxCommands.Controls.Add(this.btnRevealAll);
            this.gbxCommands.Controls.Add(this.btnFogAll);
            this.gbxCommands.Controls.Add(this.btnToggleBlackout);
            this.gbxCommands.Enabled = false;
            this.gbxCommands.Location = new System.Drawing.Point(3, 3);
            this.gbxCommands.Name = "gbxCommands";
            this.gbxCommands.Size = new System.Drawing.Size(101, 233);
            this.gbxCommands.TabIndex = 1;
            this.gbxCommands.TabStop = false;
            this.gbxCommands.Text = "Commands";
            // 
            // btnFogAddTool
            // 
            this.btnFogAddTool.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFogAddTool.BackgroundImage")));
            this.btnFogAddTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFogAddTool.Location = new System.Drawing.Point(6, 133);
            this.btnFogAddTool.Name = "btnFogAddTool";
            this.btnFogAddTool.Size = new System.Drawing.Size(42, 42);
            this.btnFogAddTool.TabIndex = 5;
            this.btnFogAddTool.UseVisualStyleBackColor = true;
            this.btnFogAddTool.Click += new System.EventHandler(this.btnFogAddTool_Click);
            // 
            // btnSyncFog
            // 
            this.btnSyncFog.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSyncFog.BackgroundImage")));
            this.btnSyncFog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSyncFog.Location = new System.Drawing.Point(53, 187);
            this.btnSyncFog.Name = "btnSyncFog";
            this.btnSyncFog.Size = new System.Drawing.Size(42, 42);
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
            this.btnSelectTool.Size = new System.Drawing.Size(42, 42);
            this.btnSelectTool.TabIndex = 0;
            this.btnSelectTool.UseVisualStyleBackColor = true;
            this.btnSelectTool.Click += new System.EventHandler(this.btnSelectTool_Click);
            // 
            // btnFogRemoveTool
            // 
            this.btnFogRemoveTool.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFogRemoveTool.BackgroundImage")));
            this.btnFogRemoveTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFogRemoveTool.Location = new System.Drawing.Point(6, 79);
            this.btnFogRemoveTool.Name = "btnFogRemoveTool";
            this.btnFogRemoveTool.Size = new System.Drawing.Size(42, 42);
            this.btnFogRemoveTool.TabIndex = 1;
            this.btnFogRemoveTool.UseVisualStyleBackColor = true;
            this.btnFogRemoveTool.Click += new System.EventHandler(this.btnFogRemoveTool_Click);
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLoadImage.BackgroundImage")));
            this.btnLoadImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLoadImage.Location = new System.Drawing.Point(6, 187);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(42, 42);
            this.btnLoadImage.TabIndex = 4;
            this.btnLoadImage.TabStop = false;
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.btnLoadImage_Click);
            // 
            // btnRevealAll
            // 
            this.btnRevealAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRevealAll.BackgroundImage")));
            this.btnRevealAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRevealAll.Location = new System.Drawing.Point(53, 79);
            this.btnRevealAll.Name = "btnRevealAll";
            this.btnRevealAll.Size = new System.Drawing.Size(42, 42);
            this.btnRevealAll.TabIndex = 4;
            this.btnRevealAll.TabStop = false;
            this.btnRevealAll.UseVisualStyleBackColor = true;
            this.btnRevealAll.Click += new System.EventHandler(this.btnRevealAll_Click);
            // 
            // btnFogAll
            // 
            this.btnFogAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFogAll.BackgroundImage")));
            this.btnFogAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFogAll.Location = new System.Drawing.Point(53, 133);
            this.btnFogAll.Name = "btnFogAll";
            this.btnFogAll.Size = new System.Drawing.Size(42, 42);
            this.btnFogAll.TabIndex = 3;
            this.btnFogAll.TabStop = false;
            this.btnFogAll.UseVisualStyleBackColor = true;
            this.btnFogAll.Click += new System.EventHandler(this.btnFogAll_Click);
            // 
            // btnToggleBlackout
            // 
            this.btnToggleBlackout.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnToggleBlackout.BackgroundImage")));
            this.btnToggleBlackout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnToggleBlackout.Location = new System.Drawing.Point(53, 19);
            this.btnToggleBlackout.Name = "btnToggleBlackout";
            this.btnToggleBlackout.Size = new System.Drawing.Size(42, 42);
            this.btnToggleBlackout.TabIndex = 0;
            this.btnToggleBlackout.TabStop = false;
            this.btnToggleBlackout.UseVisualStyleBackColor = false;
            this.btnToggleBlackout.Click += new System.EventHandler(this.btnToggleBlackout_Click);
            // 
            // gbxGridSize
            // 
            this.gbxGridSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxGridSize.Controls.Add(this.lblGridSize);
            this.gbxGridSize.Controls.Add(this.chkShowGrid);
            this.gbxGridSize.Controls.Add(this.nudGridSize);
            this.gbxGridSize.Enabled = false;
            this.gbxGridSize.Location = new System.Drawing.Point(3, 242);
            this.gbxGridSize.Name = "gbxGridSize";
            this.gbxGridSize.Size = new System.Drawing.Size(101, 47);
            this.gbxGridSize.TabIndex = 2;
            this.gbxGridSize.TabStop = false;
            this.gbxGridSize.Text = "Grid Size";
            // 
            // lblGridSize
            // 
            this.lblGridSize.AutoSize = true;
            this.lblGridSize.Location = new System.Drawing.Point(27, 21);
            this.lblGridSize.Name = "lblGridSize";
            this.lblGridSize.Size = new System.Drawing.Size(21, 13);
            this.lblGridSize.TabIndex = 0;
            this.lblGridSize.Text = "px:";
            // 
            // chkShowGrid
            // 
            this.chkShowGrid.AutoSize = true;
            this.chkShowGrid.Location = new System.Drawing.Point(6, 21);
            this.chkShowGrid.Name = "chkShowGrid";
            this.chkShowGrid.Size = new System.Drawing.Size(15, 14);
            this.chkShowGrid.TabIndex = 0;
            this.chkShowGrid.UseVisualStyleBackColor = true;
            this.chkShowGrid.Click += new System.EventHandler(this.chkShowGrid_CheckedChanged);
            // 
            // nudGridSize
            // 
            this.nudGridSize.Location = new System.Drawing.Point(53, 19);
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
            this.nudGridSize.Click += new System.EventHandler(this.nudGridSize_ValueChanged);
            this.nudGridSize.Leave += new System.EventHandler(this.nudGridSize_Leave);
            // 
            // gbxLog
            // 
            this.gbxLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxLog.Controls.Add(this.tboLog);
            this.gbxLog.Controls.Add(this.btnClearLog);
            this.gbxLog.Enabled = false;
            this.gbxLog.Location = new System.Drawing.Point(3, 295);
            this.gbxLog.Name = "gbxLog";
            this.gbxLog.Size = new System.Drawing.Size(101, 160);
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
            this.tboLog.Location = new System.Drawing.Point(6, 42);
            this.tboLog.Name = "tboLog";
            this.tboLog.ReadOnly = true;
            this.tboLog.Size = new System.Drawing.Size(89, 112);
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
            this.btnClearLog.Location = new System.Drawing.Point(71, 10);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(26, 26);
            this.btnClearLog.TabIndex = 0;
            this.btnClearLog.TabStop = false;
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.ctlMiniMap);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottom.Location = new System.Drawing.Point(0, 0);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(125, 125);
            this.pnlBottom.TabIndex = 0;
            // 
            // ctlMiniMap
            // 
            this.ctlMiniMap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ctlMiniMap.DnDMapControl = null;
            this.ctlMiniMap.Location = new System.Drawing.Point(0, 0);
            this.ctlMiniMap.MaximumSize = new System.Drawing.Size(100, 100);
            this.ctlMiniMap.MinimumSize = new System.Drawing.Size(125, 125);
            this.ctlMiniMap.Name = "ctlMiniMap";
            this.ctlMiniMap.Size = new System.Drawing.Size(125, 125);
            this.ctlMiniMap.TabIndex = 0;
            // 
            // DnDServerControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.spltControls);
            this.MinimumSize = new System.Drawing.Size(125, 0);
            this.Name = "DnDServerControlPanel";
            this.Size = new System.Drawing.Size(125, 500);
            this.Load += new System.EventHandler(this.DnDServerControlPanel_Load);
            this.spltControls.Panel1.ResumeLayout(false);
            this.spltControls.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltControls)).EndInit();
            this.spltControls.ResumeLayout(false);
            this.flpControls.ResumeLayout(false);
            this.gbxCommands.ResumeLayout(false);
            this.gbxGridSize.ResumeLayout(false);
            this.gbxGridSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGridSize)).EndInit();
            this.gbxLog.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spltControls;
        private System.Windows.Forms.Panel pnlBottom;
        private DnDMiniMap ctlMiniMap;
        private System.Windows.Forms.FlowLayoutPanel flpControls;
        private System.Windows.Forms.GroupBox gbxCommands;
        private System.Windows.Forms.Button btnFogAddTool;
        private System.Windows.Forms.Button btnSyncFog;
        private System.Windows.Forms.Button btnSelectTool;
        private System.Windows.Forms.Button btnFogRemoveTool;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Button btnRevealAll;
        private System.Windows.Forms.Button btnFogAll;
        private System.Windows.Forms.Button btnToggleBlackout;
        private System.Windows.Forms.GroupBox gbxGridSize;
        private System.Windows.Forms.Label lblGridSize;
        private System.Windows.Forms.CheckBox chkShowGrid;
        private System.Windows.Forms.NumericUpDown nudGridSize;
        private System.Windows.Forms.GroupBox gbxLog;
        private System.Windows.Forms.RichTextBox tboLog;
        private System.Windows.Forms.Button btnClearLog;
    }
}
