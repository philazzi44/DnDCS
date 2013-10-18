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
        private const float ScrollWheelStepPercent = 0.05f;

        private const byte DEFAULT_FOG_BRUSH_ALPHA = 90;

        private static readonly TimeSpan MouseMoveInterval = TimeSpan.FromMilliseconds(25d);
        private DateTime lastMouseMoveTime = DateTime.MinValue;

        private string initialParentFormText;
        private bool realTimeFogUpdates;

        private Color initialSelectToolColor;
        private Color initialFogRemoveToolColor;
        private Color initialFogAddToolColor;
        private Color initialBlackoutColor;
        private Image loadedMap;
        // TODO: When zooming is ready, change this var to always hold the zoomed map.
        private Image assignedMap;
        private string mapUrl;
        private List<FogUpdate> allFogUpdates = new List<FogUpdate>();

        private Bitmap fog;
        private Bitmap newFog;
        private Button currentTool;
        private bool isBlackOutSet;

        private FogUpdate currentFogUpdate;
        private readonly LinkedList<FogUpdate> undoFogUpdates = new LinkedList<FogUpdate>();
        private readonly LinkedList<FogUpdate> redoFogUpdates = new LinkedList<FogUpdate>();

        private MenuItem loadImage;
        private MenuItem undoLastFogAction;
        private MenuItem redoLastFogAction;

        private bool? isRemovingFog;

        private float assignedZoomFactor = 1.0f;
        private float variableZoomFactor;

        private readonly Brush newFogClearBrush = Brushes.Red;

        private readonly SolidBrush fogClearBrush = new SolidBrush(Color.White);

        /// <summary> This is the brush that is used to draw on the Fog bitmap. </summary>
        private static readonly Brush FOG_BRUSH = Brushes.Black;
        private readonly Brush newFogBrush = Brushes.Gray;
        private readonly ImageAttributes fogAttributes = new ImageAttributes();

        private Pen gridPen;

        private ServerSocketConnection connection;

        public Action<bool> ToggleFullScreen { get; set; }

        /// <summary> When set, the Center Map Image will be shown at the location for a brief period of time. </summary>
        private DateTime? centerMapImageAnimationStartTime;
        private Point? lastCenterMapPoint;
        private Timer centerMapPointDrawTimer;
        /// <summary> The duration of the Center Map Image icon being shown. </summary>
        private readonly TimeSpan centerMapImageAnimationDuration = TimeSpan.FromSeconds(1);
        private Image centerMapImage;
        
        private System.Threading.Thread zoomFactorHandlerThread;
        private System.Threading.AutoResetEvent zoomFactorHandlerEvent = new System.Threading.AutoResetEvent(false);
        private bool isZoomFactorInProgress;
        private bool isZoomFactorRunning;
        private Font zoomFactorFont;
        private const string ZoomInstructionMessage = "Press Enter or Left Click to commit the zoom factor, and Escape or Right Click to cancel.";


        private Point scrollPosition = Point.Empty;
        private Point ScrollPosition
        {
            get { return scrollPosition; }
            set { SetScroll(value.X, value.Y); }
        }

        public ServerControl()
        {
            InitializeComponent();
        }
        
        private void ServerControl_Load(object sender, EventArgs e)
        {
            centerMapImage = AssetsLoader.CenterMapOverlayIcon;
            centerMapPointDrawTimer = new Timer();
            centerMapPointDrawTimer.Tick += new EventHandler(centerMapPointDrawTimer_Tick);
            this.centerMapPointDrawTimer.Interval = 1000;

            initialSelectToolColor = btnSelectTool.BackColor;
            initialFogRemoveToolColor = btnFogRemoveTool.BackColor;
            initialFogAddToolColor = btnFogAddTool.BackColor;
            initialBlackoutColor = btnToggleBlackout.BackColor;

            initialParentFormText = this.ParentForm.Text;
            this.ParentForm.Text = initialParentFormText + " (0 clients connected)";

            this.Disposed += new EventHandler(ServerControl_Disposed);

            // Do a deep wiring of Mouse Wheel to intercept it regardless of the control that is selected.
            var controls = new Queue<Control>();
            controls.Enqueue(this);
            while (controls.Count > 0)
            {
                var control = controls.Dequeue();
                foreach (var child in control.Controls.OfType<Control>())
                    controls.Enqueue(child);
                control.MouseWheel += new MouseEventHandler(pbxMap_MouseWheel);
            }

            connection = new ServerSocketConnection(ConfigValues.DefaultServerPort);
            connection.OnClientConnected += connection_OnClientConnected;
            connection.OnClientCountChanged += new Action<int>(connection_OnClientCountChanged);
            connection.OnSocketEvent += new Action<ServerEvent>(connection_OnSocketEvent);

            var serverData = Persistence.LoadServerData();
            realTimeFogUpdates = serverData.RealTimeFogUpdates;
            btnSyncFog.Visible = !realTimeFogUpdates;
            gbxLog.Visible = serverData.ShowLog;
            gbxGridSize.Visible = serverData.ShowGridValues;
            chkShowGrid.Checked = serverData.ShowGrid;
            nudGridSize.Minimum = ConfigValues.MinimumGridSize;
            nudGridSize.Maximum = ConfigValues.MaximumGridSize;
            nudGridSize.Value = Math.Min(nudGridSize.Maximum, Math.Max(nudGridSize.Minimum, serverData.GridSize));

            gridPen = (serverData.IsGridColorSet) ? new Pen(Color.FromArgb(serverData.GridColorA, serverData.GridColorR, serverData.GridColorG, serverData.GridColorB)) : new Pen(Color.Aqua);

            SetFogAttributesColorMatrix(DEFAULT_FOG_BRUSH_ALPHA);
        }

        private void connection_OnClientConnected()
        {
            if (connection.IsStopping)
                return;
            SendAll(true);
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

            if (loadedMap != null)
                connection.WriteMap(loadedMap);
            if (fog != null)
                connection.WriteFog(fog);
            connection.WriteGridSize(chkShowGrid.Checked, chkShowGrid.Checked ? (int)nudGridSize.Value : 0);
            connection.WriteGridColor(gridPen.Color.ToSocketColor());
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

        private void connection_OnSocketEvent(ServerEvent socketEvent)
        {
            AppendToUILog(socketEvent.ToString());
        }

        private void ServerControl_Disposed(object sender, EventArgs e)
        {
            if (loadedMap != null)
                loadedMap.Dispose();
            if (fog != null)
                fog.Dispose();
            if (connection != null)
                connection.Stop();
            if (gridPen != null)
                gridPen.Dispose();
        }
        
        public MainMenu GetMainMenu()
        {
            var serverData = Persistence.LoadServerData();

            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
                loadImage = new MenuItem("Load Image", OnLoadImage_Click, Shortcut.CtrlShiftO),
                new MenuItem("-"),
                //new MenuItem("Save State", OnSaveState_Click, Shortcut.CtrlS),
                //new MenuItem("Load State", OnLoadState_Click, Shortcut.CtrlO),
                //new MenuItem("-"),
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
                                this.UpdateFogImage(fogUpdates, false);
                                connection.WriteFog(fog);
                                this.pbxMap.Refresh();
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

        private void TryPurgeMapData(string imageUrl)
        {
            var purgeMapData = MessageBox.Show(this, "Would you like to purge the previously stored fog? Otherwise, when drawing on the map, you will overwrite it.", "Purge Map Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            if (purgeMapData)
            {
                Persistence.SaveServerFogData(imageUrl, null);
            }
        }

        private void OnSaveState_Click(object sender, EventArgs e)
        {
            // TODO: Commit the png and overlay information to a file
            throw new NotImplementedException();
        }

        private void OnLoadState_Click(object sender, EventArgs e)
        {
            // TODO: Load a png and overlay information to a file
            throw new NotImplementedException();
        }

        private void OnExit_Click(object sender, EventArgs e)
        {
            // TODO: Prompt for save and save if needed.
            connection.Stop();
            this.ParentForm.Close();
        }

        private void OnUndoLastFogAction_Click(object sender, EventArgs e)
        {
            if (undoFogUpdates.Any())
            {
                var lastFogUpdate = undoFogUpdates.Last();
                lastFogUpdate.IsClearing = !lastFogUpdate.IsClearing;
                UpdateFogImage(lastFogUpdate);
                redoFogUpdates.AddLast(lastFogUpdate);
                undoFogUpdates.RemoveLast();
                pbxMap.Refresh();

                undoLastFogAction.Enabled = undoFogUpdates.Any();
                redoLastFogAction.Enabled = true;

                if (realTimeFogUpdates)
                    connection.WriteFog(fog);
            }
        }
        
        private void OnRedoLastFogAction_Click(object sender, EventArgs e)
        {
            if (redoFogUpdates.Any())
            {
                var lastFogUpdate = redoFogUpdates.Last();
                lastFogUpdate.IsClearing = !lastFogUpdate.IsClearing;
                UpdateFogImage(lastFogUpdate);
                undoFogUpdates.AddLast(lastFogUpdate);
                redoFogUpdates.RemoveLast();
                pbxMap.Refresh();

                undoLastFogAction.Enabled = true;
                redoLastFogAction.Enabled = redoFogUpdates.Any();

                if (realTimeFogUpdates)
                    connection.WriteFog(fog);
            }
        }

        private void OnRealTimeFogUpdates_Click(object sender, EventArgs e)
        {
            ToggleTools(btnSelectTool);

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

                colorOptions.GridLineColor = gridPen.Color;
                if (colorOptions.ShowDialog(this) == DialogResult.OK)
                {
                    gridPen.Dispose();
                    var newColor = colorOptions.GridLineColor;
                    gridPen = new Pen(newColor);

                    serverData.GridColorA = newColor.A;
                    serverData.GridColorR = newColor.R;
                    serverData.GridColorG = newColor.G;
                    serverData.GridColorB = newColor.B;
                    serverData.IsGridColorSet = true;

                    Persistence.SaveServerData(serverData);

                    connection.WriteGridColor(colorOptions.GridLineColor.ToSocketColor());

                    pbxMap.Refresh();
                }
            }
        }
        
        private void flpControls_SizeChanged(object sender, EventArgs e)
        {
            this.gbxCommands.Width = flpControls.Width - gbxCommands.Margin.Right;
        }

        private void btnSelectTool_Click(object sender, EventArgs e)
        {
            if (currentTool != btnSelectTool)
                ToggleTools(btnSelectTool);
        }

        private void btnFogAddTool_Click(object sender, EventArgs e)
        {
            if (currentTool != btnFogAddTool)
                ToggleTools(btnFogAddTool);
        }
        
        private void btnFogRemoveTool_Click(object sender, EventArgs e)
        {
            if (currentTool != btnFogRemoveTool)
                ToggleTools(btnFogRemoveTool);
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
                var fogAllFogUpdate = new FogUpdate(revealAll);
                fogAllFogUpdate.Add(new SimplePoint(0, 0));
                fogAllFogUpdate.Add(new SimplePoint(fog.Width, 0));
                fogAllFogUpdate.Add(new SimplePoint(fog.Width, fog.Height));
                fogAllFogUpdate.Add(new SimplePoint(0, fog.Height));

                UpdateFogImage(fogAllFogUpdate);
                undoFogUpdates.Clear();
                redoFogUpdates.Clear();
                undoLastFogAction.Enabled = redoLastFogAction.Enabled = false;
                pbxMap.Refresh();

                if (realTimeFogUpdates)
                    connection.WriteFog(fog);
            }
        }

        private void btnSyncFog_Click(object sender, EventArgs e)
        {
            // TODO: More efficient to send the list of updates rather than the full fog map.
            connection.WriteFog(fog);
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

            pbxMap.Refresh();
        }

        private void nudGridSize_ValueChanged(object sender, EventArgs e)
        {
            connection.WriteGridSize(chkShowGrid.Checked, chkShowGrid.Checked ? (int)nudGridSize.Value : 0);

            pbxMap.Refresh();
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

        private void ToggleTools(Button enabledTool)
        {
            // Ignore any tool toggling if we're not even allowing commands yet.
            if (!gbxCommands.Enabled)
                return;

            // Unset the previous tool as needed.
            if (currentTool == btnSelectTool)
                UnsetSelectTool();
            if (currentTool == btnFogRemoveTool)
                UnsetFogClearTool();
            if (currentTool == btnFogAddTool)
                UnsetFogAddTool();

            // Change the enabledness & colors as needed.
            if (btnSelectTool == enabledTool)
            {
                btnSelectTool.Enabled = false;
                btnSelectTool.BackColor = Color.White;

                btnFogRemoveTool.Enabled = true;
                btnFogRemoveTool.BackColor = initialFogRemoveToolColor;
                btnFogAddTool.Enabled = true;
                btnFogAddTool.BackColor = initialFogAddToolColor;
            }
            else if (btnFogRemoveTool == enabledTool)
            {
                btnFogRemoveTool.Enabled = false;
                btnFogRemoveTool.BackColor = Color.White;

                btnSelectTool.Enabled = true;
                btnSelectTool.BackColor = initialSelectToolColor;
                btnFogAddTool.Enabled = true;
                btnFogAddTool.BackColor = initialFogAddToolColor;
            }
            else if (btnFogAddTool == enabledTool)
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

            // Set the new tool
            if (btnSelectTool == enabledTool)
                SetSelectTool();
            if (btnFogRemoveTool == enabledTool)
                SetFogRemoveTool();
            if (btnFogAddTool == enabledTool)
                SetFogAddTool();

            currentTool = enabledTool;
        }

        private void SetSelectTool()
        {
        }

        private void SetFogRemoveTool()
        {
            pbxMap.Cursor = Cursors.Cross;
            isRemovingFog = true;
        }

        private void SetFogAddTool()
        {
            pbxMap.Cursor = Cursors.Cross;
            isRemovingFog = false;
        }

        private void pbxMap_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                var isControl = Control.ModifierKeys.HasFlag(Keys.Control);
                var isShift = Control.ModifierKeys.HasFlag(Keys.Shift);

                if (isShift)
                {
                    ScrollLeftOrRight((e.Delta > 0));
                    ((HandledMouseEventArgs)e).Handled = true;
                }
                else
                {
                    ScrollUpOrDown((e.Delta > 0));
                    ((HandledMouseEventArgs)e).Handled = true;
                }
            }

            this.pbxMap.Invalidate();
        }

        private void pbxMap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.loadedMap == null)
                return;

            if (e.Button == MouseButtons.Left && e.Clicks >= 2)
            {
                // Get the coordinates in real map coordinates by unwinding the Scroll and Zoom factor.
                lastCenterMapPoint = e.Location.Translate((int)(this.scrollPosition.X * assignedZoomFactor), (int)(this.scrollPosition.Y * assignedZoomFactor));
                connection.WriteCenterMap(lastCenterMapPoint.Value.ToSimplePoint());
                this.centerMapImageAnimationStartTime = DateTime.Now;
                centerMapPointDrawTimer.Enabled = true;
                this.pbxMap.Refresh();
            }
        }

        private Point lastDragPosition;

        private void pbxMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.loadedMap == null)
                return;

            var doMouseDrag = false;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                doMouseDrag = (this.currentTool == this.btnSelectTool);
                
                if ((this.currentTool == this.btnFogAddTool || this.currentTool == this.btnFogRemoveTool) && isRemovingFog.HasValue)
                {
                    newFog = new Bitmap(fog.Width, fog.Height);

                    currentFogUpdate = new FogUpdate(this.isRemovingFog.Value);
                    currentFogUpdate.Add(e.Location.Translate(this.scrollPosition).ToSimplePoint());
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                doMouseDrag = (this.currentTool == this.btnSelectTool || ((this.currentTool == this.btnFogAddTool || this.currentTool == this.btnFogRemoveTool) && isRemovingFog.HasValue));
            }

            if (doMouseDrag)
            {
                lastDragPosition = e.Location;
            }
        }

        private void pbxMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.loadedMap == null)
                return;

            var doMouseDrag = false;

            if (e.Button == MouseButtons.Left)
            {
                doMouseDrag = (this.currentTool == this.btnSelectTool);

                if ((this.currentTool == this.btnFogAddTool || this.currentTool == this.btnFogRemoveTool) && isRemovingFog.HasValue)
                {
                    // We ignore events firing too fast so that we don't end up with several points that are simply too close to each other to matter.
                    if (DateTime.Now - lastMouseMoveTime < MouseMoveInterval)
                        return;
                    lastMouseMoveTime = DateTime.Now;

                    // Update the New Fog image with the newly added point, so it can be drawn on the screen in real time.
                    currentFogUpdate.Add(e.Location.Translate(this.scrollPosition).ToSimplePoint());

                    UpdateNewFogImage(currentFogUpdate);

                    pbxMap.Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                doMouseDrag = true;
            }

            if (doMouseDrag)
            {
                const int MoveThreshold = 3;

                var newDragPosition = e.Location;

                var diffY = Math.Abs(newDragPosition.Y - lastDragPosition.Y);
                if (diffY > MoveThreshold)
                {
                    if (newDragPosition.Y < lastDragPosition.Y)
                        ScrollUpOrDown(false, diffY);
                    else if (newDragPosition.Y > lastDragPosition.Y)
                        ScrollUpOrDown(true, diffY);
                }

                var diffX = Math.Abs(newDragPosition.X - lastDragPosition.X);
                if (diffX > MoveThreshold)
                {
                    if (newDragPosition.X < lastDragPosition.X)
                        ScrollLeftOrRight(false, diffX);
                    else if (newDragPosition.X > lastDragPosition.X)
                        ScrollLeftOrRight(true, diffX);
                }

                lastDragPosition = e.Location;
            }
        }
        
        private void pbxMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.loadedMap == null)
                return;

            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            if ((this.currentTool == this.btnFogAddTool || this.currentTool == this.btnFogRemoveTool) && isRemovingFog.HasValue)
            {
                var toBeDisposedFog = newFog;
                newFog = null;
                if (toBeDisposedFog != null)
                    toBeDisposedFog.Dispose();

                // Commit the last point onto the main Fog Image then clear out the 'New Fog' temporary image altogether. Note that if we don't have
                // at least 3 points, then we don't have a shape that can be used.

                currentFogUpdate.Add(e.Location.Translate(this.scrollPosition).ToSimplePoint());
                if (currentFogUpdate.Length >= 3)
                {
                    UpdateFogImage(currentFogUpdate);
                    undoFogUpdates.AddLast(currentFogUpdate);
                    undoLastFogAction.Enabled = true;
                    redoFogUpdates.Clear();
                    redoLastFogAction.Enabled = false;

                    pbxMap.Refresh();

                    if (realTimeFogUpdates)
                        connection.WriteFogUpdate(currentFogUpdate);

                    currentFogUpdate = null;
                }
            }
        }

        private void UnsetSelectTool()
        {
        }

        private void UnsetFogClearTool()
        {
            pbxMap.Cursor = Cursors.Arrow;
            this.isRemovingFog = null;
        }

        private void UnsetFogAddTool()
        {
            pbxMap.Cursor = Cursors.Arrow;
            this.isRemovingFog = null;
        }

        private void SetMapImage(string imageUrl, Image mapImage)
        {
            if (mapImage == null)
                return;
            
            var oldMap = loadedMap;

            loadedMap = mapImage;
            assignedMap = new Bitmap(loadedMap, (int)(loadedMap.Width * assignedZoomFactor), (int)(loadedMap.Height * assignedZoomFactor));

            CreateFogImage();

            pbxMap.Refresh();

            gbxCommands.Enabled = true;
            ToggleTools(btnSelectTool);

            // Re-send everything since we've just re-created the Map and Fog. This will also force a Blackout of the new image.
            SendAll(true);

            if (oldMap != null)
                oldMap.Dispose();

            mapUrl = imageUrl;
            allFogUpdates.Clear();
        }

        private void CreateFogImage()
        {
            var oldFog = fog;

            fog = new Bitmap(loadedMap.Width, loadedMap.Height);
            using (var g = Graphics.FromImage(fog))
            {
                g.FillRectangle(FOG_BRUSH, 0, 0, fog.Width, fog.Height);
            }

            if (oldFog != null)
                oldFog.Dispose();
        }

        private void SetFogAttributesColorMatrix(byte a = DEFAULT_FOG_BRUSH_ALPHA)
        {
            // All colors are alpha blended by the alpha specified
            float[][] fogMatrixItems = { new float[] {1, 0, 0, 0, 0},
                                         new float[] {0, 1, 0, 0, 0},
                                         new float[] {0, 0, 1, 0, 0},
                                         new float[] {0, 0, 0, ((float)a) / 255f, 0}, 
                                         new float[] {0, 0, 0, 0, 1}
                                    };
            fogAttributes.SetColorMatrix(new ColorMatrix(fogMatrixItems), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            fogAttributes.SetColorKey(fogClearBrush.Color, fogClearBrush.Color, ColorAdjustType.Bitmap);
        }

        private void UpdateNewFogImage(FogUpdate fogUpdate)
        {
            using (var g = Graphics.FromImage(newFog))
            {
                g.FillPolygon((fogUpdate.IsClearing) ? newFogClearBrush : newFogBrush, fogUpdate.Points.Select(p => p.ToPoint()).ToArray());
            }
        }

        private void UpdateFogImage(FogUpdate fogUpdate)
        {
            UpdateFogImage(new[] { fogUpdate }, true);
        }

        private void UpdateFogImage(FogUpdate[] fogUpdates, bool persistFogUpdatesToFile)
        {
            using (var g = Graphics.FromImage(fog))
            {
                foreach (var fogUpdate in fogUpdates)
                {
                    g.FillPolygon((fogUpdate.IsClearing) ? fogClearBrush : FOG_BRUSH, fogUpdate.Points.Select(p => p.ToPoint()).ToArray());
                }
            }

            allFogUpdates.AddRange(fogUpdates);

            if (persistFogUpdatesToFile)
            {
                Persistence.SaveServerFogData(this.mapUrl, new ServerFogData() { Data = allFogUpdates.ToFogData() });
            }
        }

        private void ScrollLeftOrRight(bool isLeft, int? distance = null)
        {
            // Scroll left/right
            int newValue;
            if (isLeft)
                newValue = this.scrollPosition.X - (distance ?? (int)(pbxMap.Width * ScrollWheelStepPercent));
            else
                newValue = this.scrollPosition.X + (distance ?? (int)(pbxMap.Width * ScrollWheelStepPercent));
            SetScroll(newValue, null);
        }

        private void ScrollUpOrDown(bool isUp, int? distance = null)
        {
            // Scroll up/down
            int newValue;
            if (isUp)
                newValue = this.scrollPosition.Y - (distance ?? (int)(pbxMap.Width * ScrollWheelStepPercent));
            else
                newValue = this.scrollPosition.Y + (distance ?? (int)(pbxMap.Width * ScrollWheelStepPercent));
            SetScroll(null, newValue);
        }

        private void SetScroll(int? desiredX, int? desiredY)
        {
            if (!desiredX.HasValue)
                desiredX = this.scrollPosition.X;
            if (!desiredY.HasValue)
                desiredY = this.scrollPosition.Y;

            // Do not allow negative scrolling in any way.
            if (desiredX.Value < 0)
                desiredX = 0;
            if (desiredY.Value < 0)
                desiredY = 0;

            // TODO: Validate that the scroll position isn't beyond the width/height of the assigned image (taking zoom into account).

            // If the map we are showing is smaller than the width/height, then no X/Y scrolling is allowed at all.
            // Otherwise, enforce that the value is at most the amount that would be needed to show the full map given the current size of the visible area.
            if (this.loadedMap.Width < this.Width)
                desiredX = 0;
            else
                desiredX = Math.Min(desiredX.Value, this.loadedMap.Width - this.Width);

            if (this.loadedMap.Height < this.Height)
                desiredY = 0;
            else
                desiredY = Math.Min(desiredY.Value, this.loadedMap.Height - this.Height);

            this.scrollPosition = new Point(desiredX.Value, desiredY.Value);
            pbxMap.Invalidate();
        }

        /// <summary> Repaint event occurs every time we request it, or when the user scrolls. </summary>
        /// <remarks> Only need to realistically draw what the user can see. </remarks>
        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            if (loadedMap == null || fog == null)
                return;

            var g = e.Graphics;

            // Force clipping to the visible area only. This clipping will be translated as needed in the subsequent calls, but ensures
            // that we never try to draw beyond the visible area.
            g.SetClip(new Rectangle(0, 0, this.Width, this.Height));

            PaintMap(g);
            PaintGrid(g);
            PaintFog(g);
            PaintCenterMapOverlayIcon(g);
            PaintZoomFactorText(g);
        }

        private void PaintMap(Graphics g)
        {
            if (this.loadedMap != null)
            {
                g.TranslateTransform(-this.scrollPosition.X, -this.scrollPosition.Y);
                g.ScaleTransform(assignedZoomFactor, assignedZoomFactor);
                {
                    // TODO: Scaling goes here
                    g.ScaleTransform(assignedZoomFactor, assignedZoomFactor);
                    g.DrawImage(this.loadedMap, new Rectangle(0, 0, this.loadedMap.Width, this.loadedMap.Height), 0, 0, this.assignedMap.Width, this.assignedMap.Height, GraphicsUnit.Pixel);
                }
                g.ResetTransform();
            }
        }

        private void PaintGrid(Graphics g)
        {
            // Because Paint events are sometimes scattered, we'll just draw the whole Grid rather than only part of it so there are no gaps.
            // Since our Grid Size is usually pretty big, this will never end up with more than maybe a hundred iterations.
            if (chkShowGrid.Checked)
            {
                g.TranslateTransform(-this.scrollPosition.X, -this.scrollPosition.Y);
                g.ScaleTransform(assignedZoomFactor, assignedZoomFactor);
                {
                    for (int x = 0; x < this.loadedMap.Width; x += (int)nudGridSize.Value)
                    {
                        g.DrawLine(gridPen, x, 0, x, this.loadedMap.Height);
                    }
                    for (int y = 0; y < this.loadedMap.Height; y += (int)nudGridSize.Value)
                    {
                        g.DrawLine(gridPen, 0, y, this.loadedMap.Width, y);
                    }
                }
                g.ResetTransform();
            }
        }

        private void PaintFog(Graphics g)
        {
            if (fog != null)
            {
                g.TranslateTransform(-this.scrollPosition.X, -this.scrollPosition.Y);
                g.ScaleTransform(assignedZoomFactor, assignedZoomFactor);
                {
                    g.DrawImage(fog, new Rectangle(0, 0, this.fog.Width, this.fog.Height), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
                    if (newFog != null)
                        g.DrawImage(newFog, new Rectangle(0, 0, this.newFog.Width, this.newFog.Height), 0, 0, newFog.Width, newFog.Height, GraphicsUnit.Pixel, fogAttributes);
                }
                g.ResetTransform();
            }
        }

        private void PaintCenterMapOverlayIcon(Graphics g)
        {
            if (centerMapImage != null && lastCenterMapPoint.HasValue)
            {
                // Draw it at the location, so that the bottom/center is at the centered point.
                g.TranslateTransform(-this.scrollPosition.X, -this.scrollPosition.Y);
                g.DrawImage(centerMapImage, lastCenterMapPoint.Value.Translate(-(centerMapImage.Width / 2), -centerMapImage.Height));
            }
        }

        private void centerMapPointDrawTimer_Tick(object sender, EventArgs e)
        {
            if (centerMapImage == null || !centerMapImageAnimationStartTime.HasValue || !lastCenterMapPoint.HasValue
                   || (centerMapImageAnimationStartTime.Value + centerMapImageAnimationDuration < DateTime.Now))
            {
                centerMapImageAnimationStartTime = null;
                lastCenterMapPoint = null;
                centerMapPointDrawTimer.Enabled = false;
            }
            this.pbxMap.Invalidate();
        }

        private void PaintZoomFactorText(Graphics g)
        {
            return;

            string[] zoomMsgs = null;
            if (isZoomFactorInProgress)
                zoomMsgs = new[] { string.Format("Zoom: {0}x", variableZoomFactor), ZoomInstructionMessage };
            else if (isZoomFactorRunning)
                zoomMsgs = new[] { string.Format("Zooming to {0}x...", variableZoomFactor) };
            if (zoomMsgs != null)
            {
                var font = this.zoomFactorFont ?? System.Drawing.SystemFonts.DefaultFont;
                for (var i = 0; i < zoomMsgs.Length; i++)
                {
                    // Draw each line one after the other, separating them by the height of the message, centered on the screen.
                    var msgSize = g.MeasureString(zoomMsgs[i], font);
                    var x = (this.pbxMap.Width / 2.0f) - (msgSize.Width / 2.0f);
                    var y = (this.pbxMap.Height / 2.0f) - (msgSize.Height / 2.0f) + msgSize.Height * i;
                    
                    g.DrawString(zoomMsgs[i], font, Brushes.Aqua, x, y);
                }
            }
        }
    }
}
