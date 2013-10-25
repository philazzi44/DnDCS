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
            this.flpTop = new System.Windows.Forms.FlowLayoutPanel();
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
            this.flpAllControls = new System.Windows.Forms.FlowLayoutPanel();
            this.ctlMiniMap = new DnDCS.WinFormsLibs.DnDMiniMap();
            this.flpTop.SuspendLayout();
            this.gbxCommands.SuspendLayout();
            this.gbxGridSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGridSize)).BeginInit();
            this.gbxLog.SuspendLayout();
            this.flpAllControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // flpTop
            // 
            this.flpTop.AutoScroll = true;
            this.flpTop.Controls.Add(this.gbxCommands);
            this.flpTop.Controls.Add(this.gbxGridSize);
            this.flpTop.Controls.Add(this.gbxLog);
            this.flpTop.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpTop.Location = new System.Drawing.Point(0, 3);
            this.flpTop.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.flpTop.Name = "flpTop";
            this.flpTop.Size = new System.Drawing.Size(125, 400);
            this.flpTop.TabIndex = 0;
            this.flpTop.WrapContents = false;
            this.flpTop.SizeChanged += new System.EventHandler(this.flpTop_SizeChanged);
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
            this.gbxCommands.Location = new System.Drawing.Point(1, 3);
            this.gbxCommands.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
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
            this.gbxGridSize.Location = new System.Drawing.Point(1, 242);
            this.gbxGridSize.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.gbxGridSize.Name = "gbxGridSize";
            this.gbxGridSize.Size = new System.Drawing.Size(101, 47);
            this.gbxGridSize.TabIndex = 2;
            this.gbxGridSize.TabStop = false;
            this.gbxGridSize.Text = "Grid Size";
            this.gbxGridSize.VisibleChanged += new System.EventHandler(this.gbxGridSize_VisibleChanged);
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
            this.gbxLog.Location = new System.Drawing.Point(1, 295);
            this.gbxLog.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.gbxLog.Name = "gbxLog";
            this.gbxLog.Size = new System.Drawing.Size(101, 160);
            this.gbxLog.TabIndex = 0;
            this.gbxLog.TabStop = false;
            this.gbxLog.Text = "Log";
            this.gbxLog.VisibleChanged += new System.EventHandler(this.gbxLog_VisibleChanged);
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
            // flpAllControls
            // 
            this.flpAllControls.Controls.Add(this.flpTop);
            this.flpAllControls.Controls.Add(this.ctlMiniMap);
            this.flpAllControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpAllControls.Location = new System.Drawing.Point(0, 0);
            this.flpAllControls.Margin = new System.Windows.Forms.Padding(0);
            this.flpAllControls.Name = "flpAllControls";
            this.flpAllControls.Size = new System.Drawing.Size(127, 534);
            this.flpAllControls.TabIndex = 0;
            // 
            // ctlMiniMap
            // 
            this.ctlMiniMap.DnDMapControl = null;
            this.ctlMiniMap.Location = new System.Drawing.Point(1, 409);
            this.ctlMiniMap.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.ctlMiniMap.MaximumSize = new System.Drawing.Size(125, 125);
            this.ctlMiniMap.MinimumSize = new System.Drawing.Size(125, 125);
            this.ctlMiniMap.Name = "ctlMiniMap";
            this.ctlMiniMap.Size = new System.Drawing.Size(125, 125);
            this.ctlMiniMap.TabIndex = 0;
            this.ctlMiniMap.TabStop = false;
            // 
            // DnDServerControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flpAllControls);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(127, 0);
            this.Name = "DnDServerControlPanel";
            this.Size = new System.Drawing.Size(127, 534);
            this.Load += new System.EventHandler(this.DnDServerControlPanel_Load);
            this.SizeChanged += new System.EventHandler(this.DnDServerControlPanel_SizeChanged);
            this.flpTop.ResumeLayout(false);
            this.gbxCommands.ResumeLayout(false);
            this.gbxGridSize.ResumeLayout(false);
            this.gbxGridSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGridSize)).EndInit();
            this.gbxLog.ResumeLayout(false);
            this.flpAllControls.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DnDMiniMap ctlMiniMap;
        private System.Windows.Forms.FlowLayoutPanel flpTop;
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
        private System.Windows.Forms.FlowLayoutPanel flpAllControls;
    }
}
