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
        private MenuItem fullScreenMenuItem;
        
        private static readonly TimeSpan MouseMoveInterval = TimeSpan.FromMilliseconds(25d);
        private DateTime lastMouseMoveTime = DateTime.MinValue;

        private string initialParentFormText;
        private bool realTimeFogUpdates;

        private Color initialSelectToolColor;
        private Color initialFogRemoveToolColor;
        private Color initialFogAddToolColor;
        private Color initialBlackoutColor;
        private string mapUrl;

        private Button currentTool;
        private bool isBlackOutSet;

        private MenuItem loadImage;
        private MenuItem undoLastFogAction;
        private MenuItem redoLastFogAction;

        private ServerSocketConnection connection;

        public Action<bool> ToggleFullScreen { get; set; }

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
            this.ParentForm.Text = initialParentFormText + " (0 clients connected)";

            this.Disposed += new EventHandler(ServerControl_Disposed);

            // Do a deep wiring of Mouse Wheel to intercept it regardless of the control that is selected.
            // TODO: If we can force-focus the Map in all cases, we'd be ok.
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

            this.ctlDnDMap.GridPen = (serverData.IsGridColorSet) ? new Pen(Color.FromArgb(serverData.GridColorA, serverData.GridColorR, serverData.GridColorG, serverData.GridColorB)) : new Pen(Color.Aqua);
            this.ctlDnDMap.AllowZoom = false;
            this.ctlDnDMap.PerformCenterMap += new Action<SimplePoint>(ctlDnDMap_PerformCenterMap);
            this.ctlDnDMap.OnFogUpdateAdded += new Action<FogUpdate>(ctlDnDMap_OnFogUpdateAdded);
            this.ctlDnDMap.Init();

            SetFogAttributesColorMatrix(Constants.DEFAULT_FOG_BRUSH_ALPHA);
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

            if (this.ctlDnDMap.LoadedMap != null)
                connection.WriteMap(this.ctlDnDMap.LoadedMap);
            if (this.ctlDnDMap.Fog != null)
                connection.WriteFog(this.ctlDnDMap.Fog);
            connection.WriteGridSize(chkShowGrid.Checked, chkShowGrid.Checked ? (int)nudGridSize.Value : 0);
            connection.WriteGridColor(this.ctlDnDMap.GridPen.Color.ToSocketColor());
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
            if (connection != null)
                connection.Stop();
        }

        private void OnFullScreen_Click(object sender, EventArgs e)
        {
            if (ToggleFullScreen == null)
                return;

            var menuItem = sender as MenuItem;

            var goFullScreen = (menuItem.Checked = !menuItem.Checked);
            ToggleFullScreen(goFullScreen);
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
                fullScreenMenuItem = new MenuItem("Full Screen", OnFullScreen_Click) { Checked = false },
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
                                connection.WriteFog(this.ctlDnDMap.Fog);
                                this.ctlDnDMap.RefreshMapPictureBox();
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
            this.ctlDnDMap.Cursor = Cursors.Cross;
            this.ctlDnDMap.IsRemovingFog = true;
        }

        private void SetFogAddTool()
        {
            this.ctlDnDMap.Cursor = Cursors.Cross;
            this.ctlDnDMap.IsRemovingFog = false;
        }

        private void ctlDnDMap_PerformCenterMap(SimplePoint centerMap)
        {
            connection.WriteCenterMap(centerMap);
        }
        
        void ctlDnDMap_OnFogUpdateAdded(FogUpdate fogUpdate)
        {
            undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
            redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

            if (realTimeFogUpdates)
                connection.WriteFogUpdate(fogUpdate);
        }

        [Obsolete]
        private void UnsetSelectTool()
        {
        }

        [Obsolete]
        private void UnsetFogClearTool()
        {
            pbxMap.Cursor = Cursors.Arrow;
            this.isRemovingFog = null;
        }

        [Obsolete]
        private void UnsetFogAddTool()
        {
            pbxMap.Cursor = Cursors.Arrow;
            this.isRemovingFog = null;
        }

        private void SetMapImage(string imageUrl, Image mapImage)
        {
            if (mapImage == null)
                return;
            
            var oldMap = LoadedMap;

            LoadedMap = mapImage;

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

            fog = new Bitmap(LoadedMap.Width, LoadedMap.Height);
            using (var g = Graphics.FromImage(fog))
            {
                g.FillRectangle(FOG_BRUSH, 0, 0, fog.Width, fog.Height);
            }

            if (oldFog != null)
                oldFog.Dispose();
        }

        private void SetFogAttributesColorMatrix(byte a = Constants.DEFAULT_FOG_BRUSH_ALPHA)
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
                newValue = this.scrollPosition.X - (distance ?? (int)(pbxMap.Width * Constants.ScrollWheelStepScrollPercent));
            else
                newValue = this.scrollPosition.X + (distance ?? (int)(pbxMap.Width * Constants.ScrollWheelStepScrollPercent));
            SetScroll(newValue, null);
        }

        private void ScrollUpOrDown(bool isUp, int? distance = null)
        {
            // Scroll up/down
            int newValue;
            if (isUp)
                newValue = this.scrollPosition.Y - (distance ?? (int)(pbxMap.Width * Constants.ScrollWheelStepScrollPercent));
            else
                newValue = this.scrollPosition.Y + (distance ?? (int)(pbxMap.Width * Constants.ScrollWheelStepScrollPercent));
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
            if (this.LoadedMap.Width < this.pbxMap.Width)
                desiredX = 0;
            else
                desiredX = Math.Min(desiredX.Value, this.LoadedMap.Width - this.pbxMap.Width);

            if (this.LoadedMap.Height < this.pbxMap.Height)
                desiredY = 0;
            else
                desiredY = Math.Min(desiredY.Value, this.LoadedMap.Height - this.pbxMap.Height);

            this.scrollPosition = new Point(desiredX.Value, desiredY.Value);
            pbxMap.Invalidate();
        }

        /// <summary> Repaint event occurs every time we request it, or when the user scrolls. </summary>
        /// <remarks> Only need to realistically draw what the user can see. </remarks>
        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            if (LoadedMap == null || fog == null)
                return;

            var g = e.Graphics;

            // Note that there's no reason to set clipping now because the Picture Box that we are drawing on is set to Fill and never grows beyond that.

            PaintMap(g);
            PaintGrid(g);
            PaintFog(g);
            PaintCenterMapOverlayIcon(g);
            PaintZoomFactorText(g);
        }

        private void PaintMap(Graphics g)
        {
            if (this.LoadedMap != null)
            {
                g.TranslateTransform(-this.scrollPosition.X, -this.scrollPosition.Y);
                g.ScaleTransform(assignedZoomFactor, assignedZoomFactor);
                {
                    // TODO: Scaling goes here
                    g.ScaleTransform(assignedZoomFactor, assignedZoomFactor);
                    g.DrawImage(this.LoadedMap, new Rectangle(0, 0, this.LoadedMap.Width, this.LoadedMap.Height), 0, 0, this.LoadedMap.Width, this.LoadedMap.Height, GraphicsUnit.Pixel);
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
                    for (int x = 0; x < this.LoadedMap.Width; x += (int)nudGridSize.Value)
                    {
                        g.DrawLine(gridPen, x, 0, x, this.LoadedMap.Height);
                    }
                    for (int y = 0; y < this.LoadedMap.Height; y += (int)nudGridSize.Value)
                    {
                        g.DrawLine(gridPen, 0, y, this.LoadedMap.Width, y);
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
            if (isZoomFactorInProgress)
            {
                var font = this.zoomFactorFont ?? System.Drawing.SystemFonts.DefaultFont;

                var zoomMsgs = new[] { string.Format("Zoom: {0}x", variableZoomFactor) }.Concat(ZoomInstructionMessages).ToArray();
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
