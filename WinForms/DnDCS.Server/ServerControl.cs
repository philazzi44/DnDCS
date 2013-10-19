using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.PersistenceObjects;
using DnDCS.Libs.ServerEvents;
using DnDCS.Libs.SimpleObjects;
using DnDCS.WinFormsLibs;
using DnDCS.WinFormsLibs.Assets;

namespace DnDCS.Server
{
    public partial class ServerControl : UserControl, IDnDCSControl
    {
        // Parent Form Communication
        private MenuItem fullScreenMenuItem;
        public Action<bool> ToggleFullScreen { get; set; }
        private string initialParentFormText;

        // Settings
        private bool realTimeFogUpdates;
        private DnDMapConstants.Tool currentTool;
        private bool isBlackOutSet;

        // Cosmetic values
        private Color initialSelectToolColor;
        private Color initialFogRemoveToolColor;
        private Color initialFogAddToolColor;
        private Color initialBlackoutColor;

        private string mapUrl;

        // Menu References
        private MenuItem loadImage;
        private MenuItem undoLastFogAction;
        private MenuItem redoLastFogAction;

        // Server Connection
        private ServerSocketConnection connection;

        #region Init and Cleanup

        public ServerControl()
        {
            InitializeComponent();
        }
        
        private void ServerControl_Load(object sender, EventArgs e)
        {
            initialSelectToolColor = btnSelectTool.BackColor;
            initialFogRemoveToolColor = btnFogRemoveTool.BackColor;
            initialFogAddToolColor = btnFogAddTool.BackColor;
            initialBlackoutColor = btnToggleBlackout.BackColor;
            initialParentFormText = this.ParentForm.Text;

            btnSelectTool.Tag = DnDMapConstants.Tool.SelectTool;
            btnFogRemoveTool.Tag = DnDMapConstants.Tool.FogRemoveTool;
            btnFogAddTool.Tag = DnDMapConstants.Tool.FogAddTool;

            this.Disposed += new EventHandler(ServerControl_Disposed);

            this.ParentForm.Text = initialParentFormText + " (0 clients connected)";

            this.ctlDnDMap.AllowZoom = false;
            this.ctlDnDMap.PerformCenterMap += new Action<SimplePoint>(ctlDnDMap_PerformCenterMap);
            this.ctlDnDMap.OnOneFogUpdatesChanged += new Action<FogUpdate>(ctlDnDMap_OnOneFogUpdatesChanged);
            this.ctlDnDMap.OnManyFogUpdatesChanged += new Action(ctlDnDMap_OnManyFogUpdatesChanged);
            this.ctlDnDMap.FogAlpha = DnDMapConstants.DEFAULT_FOG_BRUSH_ALPHA;
            this.ctlDnDMap.TryToggleFullScreen += new Action<Keys>(ctlDnDMap_TryToggleFullScreen);
            this.ctlDnDMap.Init();

            var serverData = Persistence.LoadServerData();
            realTimeFogUpdates = serverData.RealTimeFogUpdates;
            btnSyncFog.Visible = !realTimeFogUpdates;
            gbxLog.Visible = serverData.ShowLog;
            gbxGridSize.Visible = serverData.ShowGridValues;
            chkShowGrid.Checked = serverData.ShowGrid;
            nudGridSize.Minimum = ConfigValues.MinimumGridSize;
            nudGridSize.Maximum = ConfigValues.MaximumGridSize;
            nudGridSize.Value = Math.Min(nudGridSize.Maximum, Math.Max(nudGridSize.Minimum, serverData.GridSize));
            if (serverData.IsGridColorSet)
                this.ctlDnDMap.GridPen = new Pen(Color.FromArgb(serverData.GridColorA, serverData.GridColorR, serverData.GridColorG, serverData.GridColorB));

            connection = new ServerSocketConnection(ConfigValues.DefaultServerPort);
            connection.OnClientConnected += connection_OnClientConnected;
            connection.OnClientCountChanged += new Action<int>(connection_OnClientCountChanged);
            connection.OnSocketEvent += new Action<ServerEvent>(connection_OnSocketEvent);
        }

        private void ctlDnDMap_TryToggleFullScreen(Keys keyCode)
        {
            if (keyCode == Keys.F11 || (keyCode == Keys.Escape && this.fullScreenMenuItem.Checked))
                this.fullScreenMenuItem.PerformClick();
        }

        private void ServerControl_Disposed(object sender, EventArgs e)
        {
            if (connection != null)
                connection.Stop();
        }

        #endregion Init and Cleanup

        #region Connection Logic and Callbacks

        private void connection_OnClientConnected()
        {
            if (connection.IsStopping)
                return;
            SendAll(true);
        }

        private void connection_OnClientCountChanged(int count)
        {
            if (connection.IsStopping)
                return;
            this.BeginInvoke(new Action(() =>
            {
                this.ParentForm.Text = initialParentFormText + string.Format(" ({0} client{1} connected)", count, (count == 1) ? string.Empty : "s");
            }));
        }

        private void connection_OnSocketEvent(ServerEvent socketEvent)
        {
            AppendToUILog(socketEvent.ToString());
        }

        private void SendAll(bool sendBlackout)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { SendAll(sendBlackout); }));
                return;
            }

            if (!this.isBlackOutSet)
                this.btnToggleBlackout.PerformClick();
            else
                connection.WriteBlackout(true);

            if (this.ctlDnDMap.LoadedMap != null)
                connection.WriteMap(this.ctlDnDMap.LoadedMap);
            if (this.ctlDnDMap.Fog != null)
                connection.WriteFog(this.ctlDnDMap.Fog);
            connection.WriteGridSize(chkShowGrid.Checked, chkShowGrid.Checked ? (int)nudGridSize.Value : 0);
            connection.WriteGridColor(this.ctlDnDMap.GridPen.Color.ToSocketColor());
        }

        #endregion Connection Logic and Callbacks

        #region Menu and Menu Callbacks

        public MainMenu GetMainMenu()
        {
            var serverData = Persistence.LoadServerData();

            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
                loadImage = new MenuItem("Load Image", OnLoadImage_Click, Shortcut.CtrlShiftO),
                new MenuItem("-"),
                fullScreenMenuItem = new MenuItem("Full Screen", OnFullScreen_Click) { Checked = false },
                new MenuItem("-"),
                new MenuItem("Exit", OnExit_Click),
            });

            var optionsMenu = new MenuItem("Options");
            optionsMenu.MenuItems.AddRange(new MenuItem[] 
            {
                undoLastFogAction = new MenuItem("Undo Last Fog Action", OnUndoLastFogAction_Click, Shortcut.CtrlZ) { Enabled = false },
                redoLastFogAction = new MenuItem("Redo Last Fog Action", OnRedoLastFogAction_Click, Shortcut.CtrlY) { Enabled = false },
                new MenuItem("-"),
                new MenuItem("Real-time Fog Updates", OnRealTimeFogUpdates_Click) { Checked = serverData.RealTimeFogUpdates },
                new MenuItem("-"),
                new MenuItem("Show Grid Values", OnShowGridValues_Click) { Checked = serverData.ShowGridValues },
                new MenuItem("Show Log", OnShowLog_Click) { Checked = serverData.ShowLog },
                new MenuItem("-"),
                new MenuItem("Set Color Options", OnSetColorOptions_Click),
            });

            menu.MenuItems.Add(fileMenu);
            menu.MenuItems.Add(optionsMenu);
            return menu;
        }

        private void OnLoadImage_Click(object sender, EventArgs e)
        {
            using (var loadImage = new GetImageUrlDialog())
            {
                var result = loadImage.ShowDialog(this);
                if (result == DialogResult.OK && loadImage.LoadedImageUrl != this.mapUrl)
                {
                    var log = string.Format("Loaded image url '{0}'.", loadImage.LoadedImageUrl);
                    Logger.LogInfo(log);
                    AppendToUILog(log);
                    SetMapImage(loadImage.LoadedImageUrl, loadImage.LoadedImage);

                    var hasFogData = Persistence.PeekServerFogData(loadImage.LoadedImageUrl);
                    if (hasFogData)
                    {
                        var useMapData = MessageBox.Show(this, "Map has been loaded before. Would you like to reload the revealed fog?", "Load Fog Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                        if (useMapData)
                        {
                            var fogData = Persistence.LoadServerFogData(loadImage.LoadedImageUrl);
                            if (fogData == null)
                            {
                                TryPurgeMapData(loadImage.LoadedImageUrl);
                            }
                            else
                            {
                                var fogUpdates = fogData.Data.ToFogUpdate();
                                this.ctlDnDMap.SetFogUpdates(fogUpdates);
                                connection.WriteFog(this.ctlDnDMap.Fog);
                            }
                        }
                        else
                        {
                            TryPurgeMapData(loadImage.LoadedImageUrl);
                        }
                    }
                }
            }
        }

        private void OnFullScreen_Click(object sender, EventArgs e)
        {
            if (ToggleFullScreen == null)
                return;

            var menuItem = sender as MenuItem;

            var goFullScreen = (menuItem.Checked = !menuItem.Checked);
            ToggleFullScreen(goFullScreen);
        }

        private void OnExit_Click(object sender, EventArgs e)
        {
            // TODO: Prompt for save and save if needed.
            connection.Stop();
            this.ParentForm.Close();
        }

        private void OnUndoLastFogAction_Click(object sender, EventArgs e)
        {
            this.ctlDnDMap.TryUndoLastFogAction();
            undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
            redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

            if (realTimeFogUpdates)
                connection.WriteFog(this.ctlDnDMap.Fog);
        }

        private void OnRedoLastFogAction_Click(object sender, EventArgs e)
        {
            this.ctlDnDMap.TryRedoLastFogAction();
            undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
            redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

            if (realTimeFogUpdates)
                connection.WriteFog(this.ctlDnDMap.Fog);
        }

        private void OnRealTimeFogUpdates_Click(object sender, EventArgs e)
        {
            ToggleTools(DnDMapConstants.Tool.SelectTool);

            var menuItem = sender as MenuItem;
            realTimeFogUpdates = menuItem.Checked = !menuItem.Checked;

            var serverData = Persistence.LoadServerData();
            serverData.RealTimeFogUpdates = realTimeFogUpdates;
            Persistence.SaveServerData(serverData);

            btnSyncFog.Visible = !realTimeFogUpdates;
        }

        private void OnShowGridValues_Click(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            this.gbxGridSize.Visible = menuItem.Checked = !menuItem.Checked;

            var serverData = Persistence.LoadServerData();
            serverData.ShowGridValues = this.gbxGridSize.Visible;
            Persistence.SaveServerData(serverData);
        }

        private void OnShowLog_Click(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            this.gbxLog.Visible = menuItem.Checked = !menuItem.Checked;

            var serverData = Persistence.LoadServerData();
            serverData.ShowLog = this.gbxLog.Visible;
            Persistence.SaveServerData(serverData);
        }

        private void OnSetColorOptions_Click(object sender, EventArgs e)
        {
            using (var colorOptions = new ColorOptionsDialog())
            {
                var serverData = Persistence.LoadServerData();

                colorOptions.GridLineColor = this.ctlDnDMap.GridPen.Color;
                if (colorOptions.ShowDialog(this) == DialogResult.OK)
                {
                    var newColor = colorOptions.GridLineColor;
                    this.ctlDnDMap.GridPen = new Pen(newColor);

                    serverData.GridColorA = newColor.A;
                    serverData.GridColorR = newColor.R;
                    serverData.GridColorG = newColor.G;
                    serverData.GridColorB = newColor.B;
                    serverData.IsGridColorSet = true;

                    Persistence.SaveServerData(serverData);

                    connection.WriteGridColor(colorOptions.GridLineColor.ToSocketColor());

                    this.ctlDnDMap.RefreshMapPictureBox();
                }
            }
        }

        #endregion Menu and Menu Callbacks

        #region Control and Tool Events
        
        private void flpControls_SizeChanged(object sender, EventArgs e)
        {
            this.gbxCommands.Width = flpControls.Width - gbxCommands.Margin.Right;
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
            if (isBlackOutSet)
            {
                // Send message to client to stop doing full blackouts, and obey the fog of war map being sent over
                isBlackOutSet = false;
                btnToggleBlackout.BackColor = initialBlackoutColor;
            }
            else
            {
                // Send message to client to do a full blackout, ignoring any fog of war map that may exist
                isBlackOutSet = true;
                btnToggleBlackout.BackColor = Color.Black;
            }

            // Map and Fog Updates would have been sent to the client in real-time but masked on their end, so we can simply inform them of the change.
            connection.WriteBlackout(isBlackOutSet);
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
                this.ctlDnDMap.FogOrRevealAll(revealAll);

                undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
                redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

                if (realTimeFogUpdates)
                    connection.WriteFog(this.ctlDnDMap.Fog);
            }
        }

        private void btnSyncFog_Click(object sender, EventArgs e)
        {
            connection.WriteFog(this.ctlDnDMap.Fog);
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            this.loadImage.PerformClick();
        }

        private void chkShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            lblGridSize.Enabled = nudGridSize.Enabled = chkShowGrid.Checked;

            var serverData = Persistence.LoadServerData();
            serverData.ShowGrid = this.chkShowGrid.Checked;
            Persistence.SaveServerData(serverData);

            connection.WriteGridSize(chkShowGrid.Checked, chkShowGrid.Checked ? (int)nudGridSize.Value : 0);

            this.ctlDnDMap.RefreshMapPictureBox();
        }

        private void nudGridSize_ValueChanged(object sender, EventArgs e)
        {
            connection.WriteGridSize(chkShowGrid.Checked, chkShowGrid.Checked ? (int)nudGridSize.Value : 0);

            this.ctlDnDMap.RefreshMapPictureBox();
        }

        private void nudGridSize_Leave(object sender, EventArgs e)
        {
            var serverData = Persistence.LoadServerData();
            serverData.GridSize = (chkShowGrid.Checked) ? (int)this.nudGridSize.Value : 0;
            Persistence.SaveServerData(serverData);
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            tboLog.Clear();
        }
        
        private void tboLog_TextChanged(object sender, EventArgs e)
        {
            this.btnClearLog.Enabled = (this.tboLog.TextLength > 0);
        }

        #endregion Control and Tool Events

        #region DnD Map Events

        private void ctlDnDMap_PerformCenterMap(SimplePoint centerMap)
        {
            connection.WriteCenterMap(centerMap);
        }

        private void ctlDnDMap_OnOneFogUpdatesChanged(FogUpdate fogUpdate)
        {
            undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
            redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

            if (realTimeFogUpdates)
                connection.WriteFogUpdate(fogUpdate);
        }
        
        private void ctlDnDMap_OnManyFogUpdatesChanged()
        {
            undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
            redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

            connection.WriteFog(this.ctlDnDMap.Fog);
        }

        #endregion DnD Map Events

        private void AppendToUILog(string text)
        {
            try
            {
                tboLog.BeginInvoke(new Action(() =>
                {
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
                }));
            }
            catch
            {
            }
        }

        private void TryPurgeMapData(string imageUrl)
        {
            var purgeMapData = MessageBox.Show(this, "Would you like to purge the previously stored fog? Otherwise, when drawing on the map, you will overwrite it.", "Purge Map Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            if (purgeMapData)
            {
                Persistence.SaveServerFogData(imageUrl, null);
            }
        }

        private void ToggleTools(DnDMapConstants.Tool newTool)
        {
            // Ignore any tool toggling if we're not even allowing commands yet.
            if (!gbxCommands.Enabled)
                return;

            this.ctlDnDMap.CurrentTool = newTool;

            // Change the enabledness & colors as needed.
            if (newTool == DnDMapConstants.Tool.SelectTool)
            {
                btnSelectTool.Enabled = false;
                btnSelectTool.BackColor = Color.White;

                btnFogRemoveTool.Enabled = true;
                btnFogRemoveTool.BackColor = initialFogRemoveToolColor;
                btnFogAddTool.Enabled = true;
                btnFogAddTool.BackColor = initialFogAddToolColor;
            }
            else if (newTool == DnDMapConstants.Tool.FogRemoveTool)
            {
                btnFogRemoveTool.Enabled = false;
                btnFogRemoveTool.BackColor = Color.White;

                btnSelectTool.Enabled = true;
                btnSelectTool.BackColor = initialSelectToolColor;
                btnFogAddTool.Enabled = true;
                btnFogAddTool.BackColor = initialFogAddToolColor;
            }
            else if (newTool == DnDMapConstants.Tool.FogAddTool)
            {
                btnFogAddTool.Enabled = false;
                btnFogAddTool.BackColor = Color.White;

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

        private void SetMapImage(string imageUrl, Image mapImage)
        {
            if (mapImage == null)
                return;

            mapUrl = imageUrl;
            this.ctlDnDMap.SetMapAsync(mapImage);
            
            gbxCommands.Enabled = true;
            ToggleTools(DnDMapConstants.Tool.SelectTool);

            // Re-send everything since we've just re-created the Map and Fog. This will also force a Blackout of the new image.
            SendAll(true);
        }
    }
}
