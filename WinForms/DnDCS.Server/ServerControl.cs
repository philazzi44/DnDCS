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
using DnDCS.Libs.SimpleObjects;
using DnDCS.Libs.ServerEvents;
using DnDCS.WinFormsLibs;

namespace DnDCS.Server
{
    public partial class ServerControl : UserControl, IDnDCSControl
    {
        private const byte DEFAULT_FOG_BRUSH_ALPHA = 90;

        private static readonly TimeSpan MouseMoveInterval = TimeSpan.FromMilliseconds(25d);
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

        private MenuItem fitMapToScreenAction;
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

        public SimplePoint ScrollPosition
        {
            // Must return the individual values for this to work, as AutoScrollPosition getter appears to be wrong for some reason.
            get { return new SimplePoint(this.pnlMap.HorizontalScroll.Value, this.pnlMap.VerticalScroll.Value); }
            set
            {
                // Oh WinForms, you make me laugh. I need to set the value twice for it to actually "stick"...
                this.pnlMap.AutoScrollPosition = value.ToPoint();
                this.pnlMap.AutoScrollPosition = value.ToPoint();
            }
        }

        public Action<bool> ToggleFullScreen { get; set; }

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
            pbxMap.SizeMode = serverData.FitMapToScreen ? PictureBoxSizeMode.StretchImage : PictureBoxSizeMode.AutoSize;
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

            if (map != null)
                connection.WriteMap(map.Width, map.Height, map.ToBytes());
            if (fog != null)
                connection.WriteFog(fog.Width, fog.Height, fog.ToBytes());
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
                // This works, but performance with large images is so bad that I had to turn it off...
                fitMapToScreenAction = new MenuItem("Fit Map to Screen (Server Only)", OnFitMapToScreen_Click) {Checked = serverData.FitMapToScreen },
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
                    connection.WriteFog(fog.Width, fog.Height, fog.ToBytes());
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
                    connection.WriteFog(fog.Width, fog.Height, fog.ToBytes());
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

        private void OnFitMapToScreen_Click(object sender, EventArgs e)
        {
            // Wipe any history, because the coordinates won't translate correctly anymore.
            undoFogUpdates.Clear();
            redoFogUpdates.Clear();
            undoLastFogAction.Enabled = redoLastFogAction.Enabled = false;

            var menuItem = sender as MenuItem;
            var fitMapToScreen = (menuItem.Checked = !menuItem.Checked);
            this.pnlMap.HorizontalScroll.Value = 0;
            this.pnlMap.VerticalScroll.Value = 0;
            this.pbxMap.SizeMode = (fitMapToScreen) ? PictureBoxSizeMode.StretchImage : PictureBoxSizeMode.AutoSize;
            this.pbxMap.Location = Point.Empty;

            var serverData = Persistence.LoadServerData();
            serverData.FitMapToScreen = (this.pbxMap.SizeMode == PictureBoxSizeMode.StretchImage);
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
                    connection.WriteFog(fog.Width, fog.Height, fog.ToBytes());
            }
        }

        private void btnSyncFog_Click(object sender, EventArgs e)
        {
            // TODO: More efficient to send the list of updates rather than the full fog map.
            connection.WriteFog(fog.Width, fog.Height, fog.ToBytes());
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

        private SimplePoint ConvertPointToStretchedImage(Point pt)
        {
            // If we're stretching the image to fit, then our point is actually somewhere else on the map by the reverse of how much we've stretched.
            return (pbxMap.SizeMode == PictureBoxSizeMode.StretchImage) ? new SimplePoint((int)(pt.X * ((float)map.Width / (float)pbxMap.Width)), (int)(pt.Y * ((float)map.Height / (float)pbxMap.Height))) : pt.ToDnDPoint();
        }

        private void pbxMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left && e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            pbxMap.MouseMove += new MouseEventHandler(pbxMap_MouseMove);
            pbxMap.MouseUp += new MouseEventHandler(pbxMap_MouseUp);

            newFog = new Bitmap(fog.Width, fog.Height);

            currentFogUpdate = new FogUpdate((e.Button == System.Windows.Forms.MouseButtons.Left));
            currentFogUpdate.Add(ConvertPointToStretchedImage(e.Location));
        }

        private void pbxMap_MouseMove(object sender, MouseEventArgs e)
        {
            // We ignore events firing too fast so that we don't end up with several points that are simply too close to each other to matter.
            if (DateTime.Now - lastMouseMoveTime < MouseMoveInterval)
                return;
            lastMouseMoveTime = DateTime.Now;

            // Update the New Fog image with the newly added point, so it can be drawn on the screen in real time.
            currentFogUpdate.Add(ConvertPointToStretchedImage(e.Location));

            UpdateNewFogImage(currentFogUpdate);

            // Invalidate only the region that we can see.
            pbxMap.Invalidate(new Rectangle(this.pnlMap.HorizontalScroll.Value, this.pnlMap.VerticalScroll.Value, pnlMap.Width, pnlMap.Height));
        }
        
        private void pbxMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left && e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            pbxMap.MouseMove -= new MouseEventHandler(pbxMap_MouseMove);
            pbxMap.MouseUp -= new MouseEventHandler(pbxMap_MouseUp);
            
            var toBeDisposedFog = newFog;
            newFog = null;
            if (toBeDisposedFog != null)
                toBeDisposedFog.Dispose();

            // Commit the last point onto the main Fog Image then clear out the 'New Fog' temporary image altogether. Note that if we don't have
            // at least 3 points, then we don't have a shape that can be used.

            currentFogUpdate.Add(ConvertPointToStretchedImage(e.Location));
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

            // Arbitrary maximum enforced for allowing Fit Map functionality, because higher than this ends up with awful performance.
            if (mapImage.Width > 1536 || mapImage.Height > 1152)
            {
                if (fitMapToScreenAction.Checked)
                    fitMapToScreenAction.PerformClick();
                fitMapToScreenAction.Enabled = false;
            }
            else
            {
                fitMapToScreenAction.Enabled = true;
            }

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
                g.FillPolygon((fogUpdate.IsClearing) ? newFogClearBrush : newFogBrush, fogUpdate.Points.Select(p => p.ToPoint()).ToArray());
            }
        }

        private void UpdateFogImage(FogUpdate fogUpdate)
        {
            using (var g = Graphics.FromImage(fog))
            {
                g.FillPolygon((fogUpdate.IsClearing) ? fogClearBrush : FOG_BRUSH, fogUpdate.Points.Select(p => p.ToPoint()).ToArray());
            }
        }
        
        private void pnlMap_SizeChanged(object sender, EventArgs e)
        {
            // If we're stretching the Map as per the settings, then we'll enforce that the Picture Box is always the same size as the Panel.
            if (pbxMap.SizeMode == PictureBoxSizeMode.StretchImage)
            {
                pbxMap.Size = pnlMap.Size;
                pbxMap.Refresh();
            }
        }

        /// <summary> Repaint event occurs every time we request it, or when the user scrolls. </summary>
        /// <remarks> Only need to realistically draw what the user can see. </remarks>
        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            if (map == null || fog == null)
                return;

            {
                var g = e.Graphics;

                // Note that if we're stretching the map to fit the space, then we need to scale accordingly.
                if (pbxMap.SizeMode == PictureBoxSizeMode.StretchImage)
                {
                    g.ScaleTransform((float)pbxMap.Width / (float)map.Width, (float)pbxMap.Height / (float)map.Height);
                }

                // We can draw only the area that is visible when newFog is set, because that means the user isn't scrolling the page at all.
                if (newFog != null)
                {
                    // These scrolling offsets tell us the top/left of any image we need to draw.
                    var scrollOffsetX = this.pnlMap.HorizontalScroll.Value;
                    var scrollOffsetY = this.pnlMap.VerticalScroll.Value;

                    g.DrawImage(fog, // Draw this
                                new Rectangle(scrollOffsetX, scrollOffsetY, map.Width, map.Height), // Onto this area
                                scrollOffsetX, scrollOffsetY, map.Width, map.Height, // From this area
                                GraphicsUnit.Pixel, // In Pixel units
                                fogAttributes); // With Alpha shading

                    g.DrawImage(newFog, // Draw this
                                new Rectangle(scrollOffsetX, scrollOffsetY, map.Width, map.Height), // Onto this area
                                scrollOffsetX, scrollOffsetY, map.Width, map.Height, // From this area
                                GraphicsUnit.Pixel, // In Pixel units
                                fogAttributes); // With Alpha shading
                }
                else
                {
                    // To prevent clipping while scrolling, we'll simply draw the full fog.
                    g.DrawImage(fog, new Rectangle(Point.Empty, map.Size), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
                    if (newFog != null)
                        g.DrawImage(newFog, new Rectangle(Point.Empty, map.Size), 0, 0, newFog.Width, newFog.Height, GraphicsUnit.Pixel, fogAttributes);
                }

                // Because Paint events are sometimes scattered, we'll just draw the whole Grid rather than only part of it so there are no gaps.
                // Since our Grid Size is usually pretty big, this will never end up with more than maybe a hundred iterations.
                if (chkShowGrid.Checked)
                {
                    for (int x = (int)nudGridSize.Value; x < map.Width; x += (int)nudGridSize.Value)
                    {
                        g.DrawLine(gridPen, x, 0, x, map.Height);
                    }
                    for (int y = (int)nudGridSize.Value; y < map.Height; y += (int)nudGridSize.Value)
                    {
                        g.DrawLine(gridPen, 0, y, map.Width, y);
                    }
                }

                return;
            }
        }
    }
}
