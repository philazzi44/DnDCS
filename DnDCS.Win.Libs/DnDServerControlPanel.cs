using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using System.ComponentModel;

namespace DnDCS.Win.Libs
{
    public partial class DnDServerControlPanel : UserControl
    {
        // Menu References
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MenuItem LoadImageMenuItem { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MenuItem UndoLastFogActionMenuItem { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MenuItem RedoLastFogActionMenuItem { get; set; }

        // Settings
        private bool realTimeFogUpdates;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RealTimeFogUpdates
        {
            get { return this.realTimeFogUpdates; }
            set
            {
                // If we were previously using non-Real Time Fog Updates, then we should sync up before we flip the value.
                if (!this.realTimeFogUpdates)
                    this.btnSyncFog.PerformClick();

                this.realTimeFogUpdates = value;
                this.btnSyncFog.Visible = !value;
            }
        }
        private DnDMapConstants.Tool currentTool;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBlackOutSet { get; private set; }

        // Cosmetic values
        private readonly int initialCommandsRightBuffer;
        private Color initialSelectToolColor;
        private Color initialFogRemoveToolColor;
        private Color initialFogAddToolColor;
        private Color initialBlackoutColor;

        // Server Connection
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ServerSocketConnection Connection { get; set; }

        // Related DnD Server Map
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DnDServerPictureBox DnDMapControl { get; set; }

        // Control Exposure
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowGridValues { get { return this.gbxGridSize.Visible; } set { this.gbxGridSize.Visible = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowGrid { get { return this.chkShowGrid.Checked; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int GridSize { get { return (int)this.nudGridSize.Value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowLogValues { get { return this.gbxLog.Visible; } set { this.gbxLog.Visible = value; } }

        #region Init and Cleanup

        public DnDServerControlPanel()
        {
            InitializeComponent();

            initialCommandsRightBuffer = this.flpTop.Right - this.gbxCommands.Right;
        }

        private void DnDServerControlPanel_Load(object sender, EventArgs e)
        {
            initialSelectToolColor = btnSelectTool.BackColor;
            initialFogRemoveToolColor = btnFogRemoveTool.BackColor;
            initialFogAddToolColor = btnFogAddTool.BackColor;
            initialBlackoutColor = btnToggleBlackout.BackColor;

            btnSelectTool.Tag = DnDMapConstants.Tool.SelectTool;
            btnFogRemoveTool.Tag = DnDMapConstants.Tool.FogRemoveTool;
            btnFogAddTool.Tag = DnDMapConstants.Tool.FogAddTool;
        }

        public void Init()
        {
            var serverData = Persistence.LoadServerData();
            this.realTimeFogUpdates = serverData.RealTimeFogUpdates;
            btnSyncFog.Visible = !this.realTimeFogUpdates;

            gbxGridSize.Visible = serverData.ShowGridValues;
            chkShowGrid.Checked = serverData.ShowGrid;
            nudGridSize.Minimum = ConfigValues.MinimumGridSize;
            nudGridSize.Maximum = ConfigValues.MaximumGridSize;
            nudGridSize.Value = Math.Min(nudGridSize.Maximum, Math.Max(nudGridSize.Minimum, serverData.GridSize));

            gbxLog.Visible = serverData.ShowLog;

            // The only command that can be done at the start is Load Image.
            foreach (var c in gbxCommands.Controls.OfType<Control>().Concat(gbxGridSize.Controls.OfType<Control>()).Concat(gbxLog.Controls.OfType<Control>()))
                c.Enabled = false;
            this.btnLoadImage.Enabled = true;

            this.flpTop.Height = GetTopFlowPanelHeight();

            this.ctlMiniMap.DnDMapControl = this.DnDMapControl;
            this.ctlMiniMap.OnNewCenterMap += new Action<SimplePoint>(ctlMiniMap_OnNewCenterMap);
            this.ctlMiniMap.Init();
        }

        private int GetTopFlowPanelHeight()
        {
            var visibleControls = this.flpTop.Controls.OfType<Control>().Where(c => c.Visible).OrderBy(c => c.Top).ToArray();
            var totalHeight = visibleControls.Sum(c => c.Height + c.Margin.Top + c.Margin.Bottom);

            var diff = (totalHeight + this.ctlMiniMap.Height) - this.Height;
            if (diff > 0)
            {
                // Our height is trying to go above the full height of the control, so we'll subtract so the Mini Map always fits.
                totalHeight -= diff;
            }
            return totalHeight;
        }

        #endregion Init and Cleanup

        #region Control and Tool Events

        private void flpTop_SizeChanged(object sender, EventArgs e)
        {
            this.gbxCommands.Width = flpTop.Width - gbxCommands.Margin.Right - initialCommandsRightBuffer;
        }

        private void btnSelectTool_Click(object sender, EventArgs e)
        {
            if (currentTool != (DnDMapConstants.Tool)btnSelectTool.Tag)
                ToggleTools((DnDMapConstants.Tool)btnSelectTool.Tag);
        }

        private void btnFogAddTool_Click(object sender, EventArgs e)
        {
            if (currentTool != (DnDMapConstants.Tool)btnFogAddTool.Tag)
                ToggleTools((DnDMapConstants.Tool)btnFogAddTool.Tag);
        }

        private void btnFogRemoveTool_Click(object sender, EventArgs e)
        {
            if (currentTool != (DnDMapConstants.Tool)btnFogRemoveTool.Tag)
                ToggleTools((DnDMapConstants.Tool)btnFogRemoveTool.Tag);
        }

        private void btnToggleBlackout_Click(object sender, EventArgs e)
        {
            if (IsBlackOutSet)
            {
                // Send message to client to stop doing full blackouts, and obey the fog of war map being sent over
                IsBlackOutSet = false;
                btnToggleBlackout.BackColor = initialBlackoutColor;
            }
            else
            {
                // Send message to client to do a full blackout, ignoring any fog of war map that may exist
                IsBlackOutSet = true;
                btnToggleBlackout.BackColor = Color.Black;
            }

            // Map and Fog Updates would have been sent to the client in real-time but masked on their end, so we can simply inform them of the change.
            Connection.WriteBlackout(IsBlackOutSet);
        }

        private void btnFogAll_Click(object sender, EventArgs e)
        {
            FogOrRevealAll(false);
        }

        private void btnRevealAll_Click(object sender, EventArgs e)
        {
            FogOrRevealAll(true);
        }

        private void FogOrRevealAll(bool revealAll)
        {
            var message = (revealAll) ? "This will reveal the entire map. Are you sure? This cannot be undone." : "This will fog the entire map. Are you sure? This cannot be undone.";
            var title = (revealAll) ? "Reveal Entire Map?" : "Fog Entire Map?";
            if (MessageBox.Show(this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                var fogOrRevealFogUpdate = this.DnDMapControl.FogOrRevealAll(revealAll);

                UndoLastFogActionMenuItem.Enabled = this.DnDMapControl.AnyUndoFogUpdates;
                RedoLastFogActionMenuItem.Enabled = this.DnDMapControl.AnyRedoFogUpdates;

                if (RealTimeFogUpdates)
                    Connection.WriteFogUpdate(fogOrRevealFogUpdate);
            }
        }

        private void btnSyncFog_Click(object sender, EventArgs e)
        {
            Connection.WriteFog(this.DnDMapControl.Fog);
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            this.LoadImageMenuItem.PerformClick();
        }

        private void chkShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            lblGridSize.Enabled = nudGridSize.Enabled = chkShowGrid.Checked;

            var serverData = Persistence.LoadServerData();
            serverData.ShowGrid = this.chkShowGrid.Checked;
            Persistence.SaveServerData(serverData);

            if (Connection != null)
                Connection.WriteGridSize(chkShowGrid.Checked, chkShowGrid.Checked ? (int)nudGridSize.Value : 0);

            this.DnDMapControl.SetGridSize(chkShowGrid.Checked, (int)nudGridSize.Value);
        }

        private void nudGridSize_ValueChanged(object sender, EventArgs e)
        {
            if (Connection != null)
                Connection.WriteGridSize(chkShowGrid.Checked, chkShowGrid.Checked ? (int)nudGridSize.Value : 0);

            this.DnDMapControl.SetGridSize(chkShowGrid.Checked, (int)nudGridSize.Value);
        }

        private void nudGridSize_Leave(object sender, EventArgs e)
        {
            var serverData = Persistence.LoadServerData();
            serverData.GridSize = (chkShowGrid.Checked) ? (int)this.nudGridSize.Value : 0;
            Persistence.SaveServerData(serverData);
            this.DnDMapControl.SetGridSize(chkShowGrid.Checked, (int)nudGridSize.Value);
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            tboLog.Clear();
        }

        private void tboLog_TextChanged(object sender, EventArgs e)
        {
            this.btnClearLog.Enabled = (this.tboLog.TextLength > 0);
        }

        private void gbxGridSize_VisibleChanged(object sender, EventArgs e)
        {
            this.flpTop.Height = GetTopFlowPanelHeight();
        }

        private void gbxLog_VisibleChanged(object sender, EventArgs e)
        {
            this.flpTop.Height = GetTopFlowPanelHeight();
        }

        private void DnDServerControlPanel_SizeChanged(object sender, EventArgs e)
        {
            this.flpTop.Height = GetTopFlowPanelHeight();
        }

        #endregion Control and Tool Events

        #region Mini Map Events

        private void ctlMiniMap_OnNewCenterMap(SimplePoint centerMap)
        {
            this.DnDMapControl.SetCenterMap(centerMap);
        }
        
        #endregion Mini Map Events

        public void EnableControlPanel(DnDMapConstants.Tool initialTool = DnDMapConstants.Tool.SelectTool)
        {
            foreach (var c in gbxCommands.Controls.OfType<Control>().Concat(gbxGridSize.Controls.OfType<Control>()).Concat(gbxLog.Controls.OfType<Control>()))
                c.Enabled = true;

            ToggleTools(initialTool);
        }

        public void SetSelectTool()
        {
            this.ToggleTools(DnDMapConstants.Tool.SelectTool);
        }

        private void ToggleTools(DnDMapConstants.Tool newTool)
        {
            // Ignore any tool toggling if we're not even allowing commands yet.
            if (!gbxCommands.Enabled)
                return;

            this.DnDMapControl.CurrentTool = newTool;

            // Change the enabledness & colors as needed.
            if (newTool == DnDMapConstants.Tool.SelectTool)
            {
                btnSelectTool.Enabled = false;
                btnSelectTool.BackColor = DnDMapConstants.SelectedToolColor;

                btnFogRemoveTool.Enabled = true;
                btnFogRemoveTool.BackColor = initialFogRemoveToolColor;
                btnFogAddTool.Enabled = true;
                btnFogAddTool.BackColor = initialFogAddToolColor;
            }
            else if (newTool == DnDMapConstants.Tool.FogRemoveTool)
            {
                btnFogRemoveTool.Enabled = false;
                btnFogRemoveTool.BackColor = DnDMapConstants.SelectedToolColor;

                btnSelectTool.Enabled = true;
                btnSelectTool.BackColor = initialSelectToolColor;
                btnFogAddTool.Enabled = true;
                btnFogAddTool.BackColor = initialFogAddToolColor;
            }
            else if (newTool == DnDMapConstants.Tool.FogAddTool)
            {
                btnFogAddTool.Enabled = false;
                btnFogAddTool.BackColor = DnDMapConstants.SelectedToolColor;

                btnSelectTool.Enabled = true;
                btnSelectTool.BackColor = initialSelectToolColor;
                btnFogRemoveTool.Enabled = true;
                btnFogRemoveTool.BackColor = initialFogRemoveToolColor;
            }
            else
            {
                throw new NotImplementedException();
            }

            currentTool = newTool;
        }
        
        public void ToggleBlackout()
        {
            this.btnToggleBlackout.PerformClick();
        }

        public void AppendToUILog(string text)
        {
            try
            {
                if (this.tboLog.InvokeRequired)
                {
                    this.tboLog.BeginInvoke(new Action(() =>
                                                           {
                                                               AppendToUILog(text);
                                                           }));
                    return;
                }

                try
                {
                    if (gbxLog.Visible)
                    {
                        if (tboLog.TextLength == 0)
                            tboLog.Text = text;
                        else
                            tboLog.Text = tboLog.Text + "\r\n" + text;
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }
        }
    }
}
