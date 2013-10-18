using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using DnDCS.WinFormsLibs;
using DnDCS.WinFormsLibs.Assets;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace DnDCS.Client
{
    public partial class ClientControl : UserControl, IDnDCSControl
    {
        private MenuItem fullScreenMenuItem;
        public Action<bool> ToggleFullScreen { get; set; }

        private string initialParentFormText;

        private ClientSocketConnection connection;

        private Image receivedMap;
        private Size receivedMapSize;
        private int LogicalMapWidth { get { return (int)(receivedMapSize.Width * assignedZoomFactor); } }
        private int LogicalMapHeight { get { return (int)(receivedMapSize.Height * assignedZoomFactor); } }
        private Image fog;

        private bool isBlackoutOn;

        private int? gridSize;
        private Pen gridPen;

        private readonly ImageAttributes fogAttributes = new ImageAttributes();
        private readonly SolidBrush fogClearBrush = new SolidBrush(Color.White);
        private readonly Brush fogBrush = Brushes.Black;
        private readonly Color fogColor = Color.Black;

        private float assignedZoomFactor = 1.0f;
        private float variableZoomFactor = 1.0f;
        private bool isZoomFactorInProgress;
        private Font zoomFactorFont;
        private static readonly string[] ZoomInstructionMessages = new[] {
                                                                            "Press Enter or Left Click to commit the zoom factor.",
                                                                            "Press Escape or Right Click to cancel."
                                                                         };

        private const float ScrollWheelStepPercent = 0.05f;
        private Point scrollPosition = Point.Empty;
        private Point lastScrollDragPosition;


        #region Init and Cleanup

        public ClientControl()
        {
            InitializeComponent();
        }
        
        private void ClientControl_Load(object sender, EventArgs e)
        {
            fogAttributes.SetColorKey(fogClearBrush.Color, fogClearBrush.Color, ColorAdjustType.Bitmap);

            this.zoomFactorFont = new Font(SystemFonts.DefaultFont.FontFamily, 24.0f);
            
            initialParentFormText = this.ParentForm.Text;
            this.BackColor = fogColor;
            pbxMap.Paint += new PaintEventHandler(pbxMap_Paint);

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

            pbxMap.PreviewKeyDown += new PreviewKeyDownEventHandler(pbxMap_PreviewKeyDown);
            this.Disposed += new EventHandler(ClientControl_Disposed);

            pbxMap.Focus();
            Connect();
        }

        private void ClientControl_Disposed(object sender, EventArgs e)
        {
            if (receivedMap != null)
                receivedMap.Dispose();
            if (fog != null)
                fog.Dispose();
            if (connection != null)
                connection.Stop();
            if (gridPen != null)
                gridPen.Dispose();
        }

        #endregion Init

        #region Menu and Menu Callbacks

        public MainMenu GetMainMenu()
        {
            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
                new MenuItem("Force Focus Map", new EventHandler((o, e) => pbxMap.Focus())),
                fullScreenMenuItem = new MenuItem("Full Screen", OnFullScreen_Click) { Checked = false },
                new MenuItem("-"),
                new MenuItem("Exit", OnExit_Click),
            });
            menu.MenuItems.Add(fileMenu);
            return menu;
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
            if (connection != null)
            {
                connection.Stop();
                connection = null;
            }
            this.ParentForm.Close();
        }

        #endregion Menu and Menu Callbacks

        #region Connection Logic and Callbacks

        private void Connect()
        {
            // Prompt for Name/IP address
            using (var getConnectIP = new GetConnectIPDialog())
            {
                if (getConnectIP.ShowDialog(this) == DialogResult.OK)
                {
                    Connect(getConnectIP.Address, getConnectIP.Port);
                }
                else
                {
                    this.ParentForm.Close();
                }
            }
        }

        private void Connect(string address, int port)
        {
            if (connection != null)
                connection.Stop();

            connection = new ClientSocketConnection(address, port);
            connection.OnConnectionEstablished += new Action(connection_OnConnectionEstablished);
            connection.OnServerNotFound += new Action(connection_OnServerNotFound);
            connection.OnMapReceived += new Action<SimpleImage>(connection_OnMapReceived);
            connection.OnCenterMapReceived += new Action<SimplePoint>(connection_OnCenterMapReceived);
            connection.OnFogReceived += new Action<SimpleImage>(connection_OnFogReceived);
            connection.OnFogUpdateReceived += new Action<FogUpdate>(connection_OnFogUpdateReceived);
            connection.OnGridSizeReceived += new Action<bool, int>(connection_OnGridSizeReceived);
            connection.OnGridColorReceived += new Action<SimpleColor>(connection_OnGridColorReceived);
            connection.OnBlackoutReceived += new Action<bool>(connection_OnBlackoutReceived);
            connection.OnExitReceived += new Action(connection_OnExitReceived);
            
            this.ParentForm.Text = string.Format("{0} - Connecting to {1}:{2}...", initialParentFormText, address, port);
            connection.Start();
        }
        
        private void connection_OnConnectionEstablished()
        {
            this.ParentForm.BeginInvoke(new Action(() =>
            {
                this.ParentForm.Text = string.Format("{0} - Connected to {1}:{2}", initialParentFormText, connection.Address, connection.Port);
            }));
        }

        private void connection_OnServerNotFound()
        {
            this.BeginInvoke(new Action(() =>
            {
                MessageBox.Show(this, "The server was not found. Client application will now close.",
                                "Server Connection Not Found", MessageBoxButtons.OK);
                this.ParentForm.Close();
            }));
        }

        private void connection_OnBlackoutReceived(bool isBlackoutOn)
        {
            this.isBlackoutOn = isBlackoutOn;
            this.RefreshMapPictureBox();
        }

        private void connection_OnMapReceived(SimpleImage mapImage)
        {
            try
            {
                // Since we received a new map, we'll automatically black out everything with fog until the Server tells us otherwise.
                var newMap = mapImage.Bytes.ToImage();
                var newFog = new Bitmap(newMap.Width, newMap.Height);
                using (var g = Graphics.FromImage(newFog))
                    g.Clear(fogColor);

                this.BeginInvoke(new Action(() =>
                                {
                                    this.receivedMap = newMap;
                                    this.receivedMapSize = this.receivedMap.Size;
                                    this.fog = newFog;
                                    this.RefreshMapPictureBox();
                                }));
            }
            catch (Exception e)
            {
                Logger.LogError("Map Received Failure", e);
            }
        }

        private void connection_OnCenterMapReceived(SimplePoint centerMap)
        {
            // Take the point that we want to show, and center it on the client's UI.
            this.BeginInvoke(new Action(() =>
            {
                // The point that came in is raw on the map, so we need to account for the client's zoom factor.
                SetScroll((int)(centerMap.X * assignedZoomFactor) - this.pbxMap.Width / 2, (int)(centerMap.Y * assignedZoomFactor) - this.pbxMap.Height / 2);
            }));
        }

        private void connection_OnFogReceived(SimpleImage fogImage)
        {
            try
            {
                var newFog = fogImage.Bytes.ToImage();
                this.BeginInvoke(new Action(() =>
                                                {
                                                    this.fog = newFog;
                                                    RefreshMapPictureBox();
                                                }));
            }
            catch (Exception e)
            {
                Logger.LogError("Fog received failure.", e);
            }
        }

        private void connection_OnFogUpdateReceived(FogUpdate fogUpdate)
        {
            Image fogImageToUpdate;
            var isNewFogImage = (this.fog == null);
            if (isNewFogImage)
                fogImageToUpdate = new Bitmap(this.receivedMapSize.Width, this.receivedMapSize.Height);
            else
                fogImageToUpdate = this.fog;

            using (var g = Graphics.FromImage(fogImageToUpdate))
            {
                if (isNewFogImage)
                    g.FillRectangle(fogBrush, 0, 0, fogImageToUpdate.Width, fogImageToUpdate.Height);
                g.FillPolygon((fogUpdate.IsClearing) ? fogClearBrush : fogBrush, fogUpdate.Points.Select(p => p.ToPoint()).ToArray());
            }

            if (isNewFogImage)
                this.fog = fogImageToUpdate;

            RefreshMapPictureBox();
        }
        
        private void connection_OnGridSizeReceived(bool showGrid, int gridSize)
        {
            this.gridSize = (showGrid) ? gridSize : new Nullable<int>();
            RefreshMapPictureBox();
        }

        private void connection_OnGridColorReceived(SimpleColor gridColor)
        {
            if (gridPen != null)
                gridPen.Dispose();
            gridPen = new Pen(Color.FromArgb(gridColor.A, gridColor.R, gridColor.G, gridColor.B));

            RefreshMapPictureBox();
        }

        private void connection_OnExitReceived()
        {
            this.BeginInvoke(new Action(() =>
            {
                MessageBox.Show(this, "The server has closed the connection. Client application will now close.",
                                "Server Connection Closed", MessageBoxButtons.OK);
                this.ParentForm.Close();
            }));
        }

        #endregion Connection Logic and Callbacks

        #region Map Events

        private void RefreshMapPictureBox(bool immediateRefresh = false)
        {
            if (pbxMap.InvokeRequired)
            {
                pbxMap.BeginInvoke(new Action(() => { RefreshMapPictureBox(immediateRefresh); }));
                return;
            }

            if (immediateRefresh)
                pbxMap.Refresh();
            else
                pbxMap.Invalidate();
        }

        private void pbxMap_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.F11 || (e.KeyCode == Keys.Escape && fullScreenMenuItem.Checked))
            {
                fullScreenMenuItem.PerformClick();
                return;
            }

            if (e.Control)
            {
                if ((e.KeyCode == Keys.Add || e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.Down))
                    ZoomInOrOut((e.KeyCode == Keys.Add || e.KeyCode == Keys.Up), e.Shift);
                return;
            }

            if (isZoomFactorInProgress)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                    CommitOrRollBackZoom((e.KeyCode == Keys.Enter));
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                    ZoomInOrOut((e.KeyCode == Keys.Up), e.Shift);
                return;
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        ScrollUpOrDown(true);
                        break;
                    case Keys.Down:
                        ScrollUpOrDown(false);
                        break;
                }

                switch (e.KeyCode)
                {
                    case Keys.Left:
                        ScrollLeftOrRight(true);
                        break;
                    case Keys.Right:
                        ScrollLeftOrRight(false);
                        break;
                }
            }
        }

        private void pbxMap_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                var isControl = Control.ModifierKeys.HasFlag(Keys.Control);
                var isShift = Control.ModifierKeys.HasFlag(Keys.Shift);

                if (isControl || isZoomFactorInProgress)
                {
                    ZoomInOrOut((e.Delta > 0), isShift);
                    ((HandledMouseEventArgs)e).Handled = true;
                }
                else if (isShift)
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

            RefreshMapPictureBox();
        }

        private void pbxMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (isZoomFactorInProgress && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right))
            {
                CommitOrRollBackZoom((e.Button == MouseButtons.Left));
            }
        }

        private void pbxMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (isZoomFactorInProgress)
                return;

            lastScrollDragPosition = e.Location;
            this.pbxMap.Cursor = Cursors.Hand;
        }

        private void pbxMap_MouseMove(object sender, MouseEventArgs e)
        {
            const int MoveThreshold = 3;

            if (isZoomFactorInProgress)
                return;

            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
                return;

            var newDragPosition = e.Location;

            // Scroll based on the amount of movement.
            var diffY = Math.Abs(newDragPosition.Y - lastScrollDragPosition.Y);
            if (diffY > MoveThreshold)
            {
                if (newDragPosition.Y < lastScrollDragPosition.Y)
                    ScrollUpOrDown(false, diffY);
                else if (newDragPosition.Y > lastScrollDragPosition.Y)
                    ScrollUpOrDown(true, diffY);
            }

            var diffX = Math.Abs(newDragPosition.X - lastScrollDragPosition.X);
            if (diffX > MoveThreshold)
            {
                if (newDragPosition.X < lastScrollDragPosition.X)
                    ScrollLeftOrRight(false, diffX);
                else if (newDragPosition.X > lastScrollDragPosition.X)
                    ScrollLeftOrRight(true, diffX);
            }

            lastScrollDragPosition = e.Location;
        }

        private void pbxMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (isZoomFactorInProgress)
                return;

            this.pbxMap.Cursor = Cursors.Default;
        }

        #endregion Map Events

        #region Zoom Logic
        
        private void ZoomInOrOut(bool zoomIn, bool doubleFactor)
        {
            if (zoomIn)
                variableZoomFactor = (float)Math.Round(Math.Min(variableZoomFactor + ((doubleFactor) ? Constants.ZoomLargeStep : Constants.ZoomStep), ConfigValues.MaximumGridZoomFactor), 1);
            else
                variableZoomFactor = (float)Math.Round(Math.Max(variableZoomFactor - ((doubleFactor) ? Constants.ZoomLargeStep : Constants.ZoomStep), ConfigValues.MinimumGridZoomFactor), 1);

            isZoomFactorInProgress = true;

            RefreshMapPictureBox();
        }

        private void CommitOrRollBackZoom(bool commit)
        {
            // Commit or rollback the zoom factor.
            isZoomFactorInProgress = false;
            if (commit)
            {
                assignedZoomFactor = variableZoomFactor;
                // This will validate that the current scroll values aren't too large for the new zoom factor.
                SetScroll(null, null);
            }
            else
            {
                variableZoomFactor = assignedZoomFactor;
            }
            RefreshMapPictureBox();
        }

        #endregion Zoom Logic

        #region Scroll Logic

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

            // If the map we are showing is smaller than the width/height, then no X/Y scrolling is allowed at all.
            // Otherwise, enforce that the value is at most the amount that would be needed to show the full map given the current size of the visible area.
            if (this.LogicalMapWidth < this.pbxMap.Width)
                desiredX = 0;
            else
                desiredX = Math.Min(desiredX.Value, this.LogicalMapWidth - this.pbxMap.Width);

            if (this.LogicalMapHeight < this.pbxMap.Height)
                desiredY = 0;
            else
                desiredY = Math.Min(desiredY.Value, this.LogicalMapHeight - this.pbxMap.Height);

            this.scrollPosition = new Point(desiredX.Value, desiredY.Value);
            RefreshMapPictureBox();
        }

        #endregion Scroll Logic

        #region Painting

        /// <summary> Repaint event occurs every time we request it, or when the user scrolls. </summary>
        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            if (this.receivedMap == null)
                return;

            // Note that there's no reason to set clipping now because the Picture Box that we are drawing on is set to Fill and never grows beyond that.
            var g = e.Graphics;

            if (this.isBlackoutOn)
            {
                PaintBlackout(g);
            }
            else
            {
                PaintMap(g);
                PaintGrid(g);
                PaintFog(g);
            }

            PaintZoomFactorText(g);
        }

        private void PaintBlackout(Graphics g)
        {
            // Draw the Blackout Image in the center.
            g.Clear(Color.Black);
            g.DrawImage(AssetsLoader.BlackoutImage, this.pbxMap.Width / 2.0f - AssetsLoader.BlackoutImage.Width / 2.0f, this.pbxMap.Height / 2.0f - AssetsLoader.BlackoutImage.Height / 2.0f);
        }

        private void PaintMap(Graphics g)
        {
            if (this.receivedMap != null)
            {
                g.TranslateTransform(-this.scrollPosition.X, -this.scrollPosition.Y);
                g.ScaleTransform(assignedZoomFactor, assignedZoomFactor);
                {
                    g.DrawImage(this.receivedMap, Point.Empty);
                }
                g.ResetTransform();
            }
        }

        private void PaintGrid(Graphics g)
        {
            // Because Paint events are sometimes scattered, we'll just draw the whole Grid rather than only part of it so there are no gaps.
            // Since our Grid Size is usually pretty big, this will never end up with more than maybe a hundred iterations.
            if (gridSize.HasValue)
            {
                g.TranslateTransform(-this.scrollPosition.X, -this.scrollPosition.Y);
                g.ScaleTransform(assignedZoomFactor, assignedZoomFactor);
                {
                    for (int x = 0; x < receivedMapSize.Width; x += gridSize.Value)
                    {
                        g.DrawLine(gridPen, x, 0, x, receivedMapSize.Height);
                    }
                    for (int y = 0; y < receivedMapSize.Height; y += gridSize.Value)
                    {
                        g.DrawLine(gridPen, 0, y, receivedMapSize.Width, y);
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
                    g.DrawImage(fog, new Rectangle(0, 0, receivedMapSize.Width, receivedMapSize.Height), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
                }
                g.ResetTransform();
            }
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

                    // If we're also showing the Blackout image, then show the text beneath it.
                    if (isBlackoutOn)
                        y += AssetsLoader.BlackoutImage.Height;

                    g.DrawString(zoomMsgs[i], font, Brushes.Aqua, x, y);
                }
            }
        }

        #endregion Painting
    }
}
