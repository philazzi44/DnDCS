using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DnDCS.Libs;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using DnDCS.Libs.SocketObjects;
using DnDCS.Libs.ServerEvents;

namespace DnDCS.Server
{
    public partial class ServerControl : UserControl, IDnDCSControl
    {
        private const byte DEFAULT_FOG_BRUSH_ALPHA = 90;

        private static readonly TimeSpan MouseMoveInterval = TimeSpan.FromMilliseconds(50d);
        private DateTime lastMouseMoveTime = DateTime.MinValue;

        private string initialParentFormText;
        private bool realTimeFogUpdates;

        private Color initialSelectToolColor;
        private Color initialFogToolColor;
        private Color initialBlackoutColor;
        private Image map;

        private Bitmap fog;
        private Bitmap newFog;
        private Button lastTool;
        private bool isBlackOutSet;

        private FogUpdate currentFogUpdate;
        private readonly LinkedList<FogUpdate> undoFogUpdates = new LinkedList<FogUpdate>();
        private readonly LinkedList<FogUpdate> redoFogUpdates = new LinkedList<FogUpdate>();

        private MenuItem undoLastFogAction;
        private MenuItem redoLastFogAction;

        private readonly Brush newFogClearBrush = Brushes.Red;

        private readonly SolidBrush fogClearBrush = new SolidBrush(Color.White);

        private Color _fogBrushColor { get; set; }
        private Color FogBrushColor
        {
            get { return _fogBrushColor; }
            set
            {
                _fogBrushColor = value;
                SetFogAttributesColorMatrix(_fogBrushColor.A);
            }
        }

        private static readonly Brush FOG_BRUSH = Brushes.Black;
        private readonly Brush newFogBrush = Brushes.Gray;
        private readonly ImageAttributes fogAttributes = new ImageAttributes();

        private Pen gridPen;

        private ServerSocketConnection connection;

        public ServerControl()
        {
            InitializeComponent();
        }
        
        private void ServerControl_Load(object sender, EventArgs e)
        {
            initialSelectToolColor = btnSelectTool.BackColor;
            initialFogToolColor = btnFogTool.BackColor;
            initialBlackoutColor = btnToggleBlackout.BackColor;

            initialParentFormText = this.ParentForm.Text;
            this.ParentForm.Text = initialParentFormText + " (0 clients connected)";

            pbxMap.Paint += new PaintEventHandler(pbxMap_Paint);
            this.Disposed += new EventHandler(ServerControl_Disposed);

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

            FogBrushColor = Color.FromArgb(DEFAULT_FOG_BRUSH_ALPHA, Color.Black);
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

            connection.WriteMap(map);
            connection.WriteFog(fog);
            connection.WriteGridSize(chkShowGrid.Checked, chkShowGrid.Checked ? (int)nudGridSize.Value : 0);
            connection.WriteGridColor(gridPen.Color);
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
            if (map != null)
                map.Dispose();
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
                new MenuItem("Load Image", OnLoadImage_Click, Shortcut.CtrlShiftO),
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
                if (result == DialogResult.OK)
                {
                    var log = string.Format("Loaded image url '{0}'.", loadImage.LoadedImageUrl);
                    Logger.LogInfo(log);
                    AppendToUILog(log);
                    SetMapImage(loadImage.LoadedImage);
                }
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
                    gridPen = new Pen(colorOptions.GridLineColor);
                    serverData.GridColor = colorOptions.GridLineColor;

                    Persistence.SaveServerData(serverData);

                    connection.WriteGridColor(colorOptions.GridLineColor);

                    pbxMap.Refresh();
                }
            }
        }
        
        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            DrawOnGraphics(e.Graphics);
        }
        
        private void flpControls_SizeChanged(object sender, EventArgs e)
        {
            this.gbxCommands.Width = flpControls.Width - gbxCommands.Margin.Right;
        }

        private void btnSelectTool_Click(object sender, EventArgs e)
        {
            if (lastTool != btnSelectTool)
                ToggleTools(btnSelectTool);
        }

        private void btnFogTool_Click(object sender, EventArgs e)
        {
            if (lastTool != btnFogTool)
                ToggleTools(btnFogTool);
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
            if (MessageBox.Show(this, "This will fog the entire map. Are you sure? This cannot be undone.", "Fog Entire Map?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                var fogAllFogUpdate = new FogUpdate(false);
                fogAllFogUpdate.Add(new Point(0, 0));
                fogAllFogUpdate.Add(new Point(fog.Width, 0));
                fogAllFogUpdate.Add(new Point(fog.Width, fog.Height));
                fogAllFogUpdate.Add(new Point(0, fog.Height));

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
            if (lastTool == btnFogTool)
                UnsetFogTool();

            // Change the enabledness & colors as needed.
            if (btnSelectTool == enabledTool)
            {
                btnSelectTool.Enabled = false;
                btnSelectTool.BackColor = Color.White;
                btnFogTool.BackColor = initialFogToolColor;
                btnFogTool.Enabled = true;
            }
            else if (btnFogTool == enabledTool)
            {
                btnSelectTool.Enabled = true;
                btnSelectTool.BackColor = initialSelectToolColor;
                btnFogTool.BackColor = Color.White;
                btnFogTool.Enabled = false;
            }
            else
            {
                throw new NotImplementedException();
            }
            
            // Set the new tool
            if (btnFogTool == enabledTool)
                SetFogTool();

            lastTool = enabledTool;
        }

        private void SetFogTool()
        {
            pbxMap.Cursor = Cursors.Cross;
            pbxMap.MouseDown += new MouseEventHandler(pbxMap_MouseDown);
        }

        private void pbxMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left && e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            pbxMap.MouseMove += new MouseEventHandler(pbxMap_MouseMove);
            pbxMap.MouseUp += new MouseEventHandler(pbxMap_MouseUp);

            newFog = new Bitmap(fog.Width, fog.Height);

            currentFogUpdate = new FogUpdate((e.Button == System.Windows.Forms.MouseButtons.Left));
            currentFogUpdate.Add(e.Location);
        }

        private void pbxMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - lastMouseMoveTime < MouseMoveInterval)
                return;
            lastMouseMoveTime = DateTime.Now;

            // Update the New Fog image with the newly added point, so it can be drawn on the screen in real time.
            currentFogUpdate.Add(e.Location);
            UpdateNewFogImage(currentFogUpdate);

            pbxMap.Refresh();
        }
        
        private void pbxMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left && e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            pbxMap.MouseMove -= new MouseEventHandler(pbxMap_MouseMove);
            pbxMap.MouseUp -= new MouseEventHandler(pbxMap_MouseUp);

            // Commit the last point onto the main Fog Image then clear out the 'New Fog' temporary image altogether.
            currentFogUpdate.Add(e.Location);
            UpdateFogImage(currentFogUpdate);
            undoFogUpdates.AddLast(currentFogUpdate);
            undoLastFogAction.Enabled = true;
            redoFogUpdates.Clear();
            redoLastFogAction.Enabled = false;

            newFog.Dispose();
            newFog = null;
            pbxMap.Refresh();

            if (realTimeFogUpdates)
                connection.WriteFogUpdate(currentFogUpdate);

            currentFogUpdate = null;
        }

        private void UnsetFogTool()
        {
            pbxMap.Cursor = Cursors.Arrow;
            pbxMap.MouseDown -= new MouseEventHandler(pbxMap_MouseDown);
            pbxMap.MouseUp -= new MouseEventHandler(pbxMap_MouseUp);
        }

        private void SetMapImage(Image mapImage)
        {
            if (mapImage == null)
                return;

            var oldMap = map;

            map = mapImage;

            CreateFogImage();

            pbxMap.Image = map;
            pbxMap.Refresh();

            gbxCommands.Enabled = true;
            ToggleTools(btnSelectTool);

            // Re-send everything since we've just re-created the Map and Fog. This will also force a Blackout of the new image.
            SendAll(true);

            if (oldMap != null)
                oldMap.Dispose();
        }

        private void CreateFogImage()
        {
            var oldFog = fog;

            fog = new Bitmap(map.Width, map.Height);
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
                g.FillPolygon((fogUpdate.IsClearing) ? newFogClearBrush : newFogBrush, fogUpdate.Points);
            }
        }

        private void UpdateFogImage(FogUpdate fogUpdate)
        {
            using (var g = Graphics.FromImage(fog))
            {
                g.FillPolygon((fogUpdate.IsClearing) ? fogClearBrush : FOG_BRUSH, fogUpdate.Points);
            }
        }

        private void DrawOnGraphics(Graphics g)
        {
            if (map == null || fog == null)
                return;

            g.DrawImage(fog, new Rectangle(Point.Empty, pbxMap.Size), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);

            if (newFog != null)
                g.DrawImage(newFog, new Rectangle(Point.Empty, pbxMap.Size), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);

            if (chkShowGrid.Checked)
            {
                for (int x = (int)nudGridSize.Value; x < pbxMap.Width; x += (int)nudGridSize.Value)
                {
                    g.DrawLine(gridPen, x, 0, x, pbxMap.Height);
                }
                for (int y = (int)nudGridSize.Value; y < pbxMap.Height; y += (int)nudGridSize.Value)
                {
                    g.DrawLine(gridPen, 0, y, pbxMap.Width, y);
                }
            }
        }
    }
}
