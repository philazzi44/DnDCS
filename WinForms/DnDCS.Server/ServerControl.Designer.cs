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
            this.spltServer = new System.Windows.Forms.SplitContainer();
            this.ctlDnDMap = new DnDCS.WinFormsLibs.DnDServerPictureBox();
            this.ctlControlPanel = new DnDCS.WinFormsLibs.DnDServerControlPanel();
            ((System.ComponentModel.ISupportInitialize)(this.spltServer)).BeginInit();
            this.spltServer.Panel1.SuspendLayout();
            this.spltServer.Panel2.SuspendLayout();
            this.spltServer.SuspendLayout();
            this.SuspendLayout();
            // 
            // spltServer
            // 
            this.spltServer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spltServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltServer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.spltServer.IsSplitterFixed = true;
            this.spltServer.Location = new System.Drawing.Point(0, 0);
            this.spltServer.Name = "spltServer";
            // 
            // spltServer.Panel1
            // 
            this.spltServer.Panel1.Controls.Add(this.ctlDnDMap);
            this.spltServer.Panel1MinSize = 400;
            // 
            // spltServer.Panel2
            // 
            this.spltServer.Panel2.Controls.Add(this.ctlControlPanel);
            this.spltServer.Panel2MinSize = 127;
            this.spltServer.Size = new System.Drawing.Size(804, 489);
            this.spltServer.SplitterDistance = 670;
            this.spltServer.TabIndex = 1;
            // 
            // ctlDnDMap
            // 
            this.ctlDnDMap.AllowZoom = false;
            this.ctlDnDMap.BackColor = System.Drawing.Color.Black;
            this.ctlDnDMap.CurrentTool = DnDCS.WinFormsLibs.DnDMapConstants.Tool.SelectTool;
            this.ctlDnDMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlDnDMap.FogAlpha = ((byte)(255));
            this.ctlDnDMap.IsFlippedView = false;
            this.ctlDnDMap.IsRemovingFog = null;
            this.ctlDnDMap.Location = new System.Drawing.Point(0, 0);
            this.ctlDnDMap.Name = "ctlDnDMap";
            this.ctlDnDMap.Size = new System.Drawing.Size(666, 485);
            this.ctlDnDMap.TabIndex = 0;
            this.ctlDnDMap.UseFogAlphaEffect = false;
            // 
            // ctlControlPanel
            // 
            this.ctlControlPanel.Connection = null;
            this.ctlControlPanel.DnDMapControl = null;
            this.ctlControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlControlPanel.LoadImageMenuItem = null;
            this.ctlControlPanel.Location = new System.Drawing.Point(0, 0);
            this.ctlControlPanel.MinimumSize = new System.Drawing.Size(127, 0);
            this.ctlControlPanel.Name = "ctlControlPanel";
            this.ctlControlPanel.RealTimeFogUpdates = false;
            this.ctlControlPanel.RedoLastFogActionMenuItem = null;
            this.ctlControlPanel.ShowGridValues = true;
            this.ctlControlPanel.ShowLogValues = true;
            this.ctlControlPanel.Size = new System.Drawing.Size(127, 485);
            this.ctlControlPanel.TabIndex = 0;
            this.ctlControlPanel.UndoLastFogActionMenuItem = null;
            // 
            // ServerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.spltServer);
            this.Name = "ServerControl";
            this.Size = new System.Drawing.Size(804, 489);
            this.Load += new System.EventHandler(this.ServerControl_Load);
            this.spltServer.Panel1.ResumeLayout(false);
            this.spltServer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltServer)).EndInit();
            this.spltServer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spltServer;
        private WinFormsLibs.DnDServerPictureBox ctlDnDMap;
        private WinFormsLibs.DnDServerControlPanel ctlControlPanel;
    }
}
