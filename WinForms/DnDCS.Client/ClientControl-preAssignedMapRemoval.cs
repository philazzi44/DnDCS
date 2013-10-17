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
        private const float ScrollWheelStepPercent = 0.05f;

        private bool isExiting;

        private float assignedZoomFactor = 1.0f;
        private float variableZoomFactor = 1.0f;

        private string initialParentFormText;
        private ClientSocketConnection connection;
        private int receivedMapWidth;
        private int receivedMapHeight;
        private Image assignedMap;
        private Image receivedMap;
        private Image fog;
        private bool isBlackoutOn;
        private int? gridSize;
        private Pen gridPen;

        private readonly SolidBrush fogClearBrush = new SolidBrush(Color.White);

        private readonly Brush fogBrush = Brushes.Black;
        private readonly Color fogColor = Color.Black;

        private readonly ImageAttributes fogAttributes = new ImageAttributes();

        private System.Threading.Thread zoomFactorHandlerThread;
        private AutoResetEvent zoomFactorHandlerEvent = new AutoResetEvent(false);
        private bool isZoomFactorInProgress;
        private bool isZoomFactorRunning;
        private Font zoomFactorFont;
        private const string ZoomInstructionMessage = "Press Enter or Left Click to commit the zoom factor, and Escape or Right Click to cancel.";

        private MenuItem fullScreenAction;

        private Point scrollPosition = Point.Empty;

        public Action<bool> ToggleFullScreen { get; set; }

        public ClientControl()
        {
            InitializeComponent();
        }
        
        private void ClientControl_Load(object sender, EventArgs e)
        {
            fogAttributes.SetColorKey(fogClearBrush.Color, fogClearBrush.Color, ColorAdjustType.Bitmap);

            this.zoomFactorFont = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 24.0f);

            zoomFactorHandlerThread = new System.Threading.Thread(ZoomFactorHandlerStart)
                                          {
                                              Name = "Zoom Factor Handler Thread",
                                              IsBackground = true
                                          };
            zoomFactorHandlerThread.Start();
            
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
            isExiting = true;

            if (assignedMap != null)
                assignedMap.Dispose();
            if (receivedMap != null)
                receivedMap.Dispose();
            if (fog != null)
                fog.Dispose();
            if (connection != null)
                connection.Stop();
            if (gridPen != null)
                gridPen.Dispose();
            if (zoomFactorHandlerThread != null && zoomFactorHandlerThread.IsAlive)
                zoomFactorHandlerThread.Interrupt();
            if (zoomFactorHandlerEvent != null)
            {
                zoomFactorHandlerEvent.Set();
                zoomFactorHandlerEvent.Dispose();
                zoomFactorHandlerEvent = null;
            }
        }
        
        public MainMenu GetMainMenu()
        {
            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
                new MenuItem("Force Focus Map", new EventHandler((o, e) => pbxMap.Focus())),
                fullScreenAction = new MenuItem("Full Screen", OnFullScreen_Click) { Checked = false },
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
            isExiting = true;
        }

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
                var map = mapImage.Bytes.ToImage();

                // Since we received a new map, we'll automatically black out everything with fog until the Server tells us otherwise.
                this.fog = new Bitmap(map.Width, map.Height);
                using (var g = Graphics.FromImage(this.fog))
                    g.Clear(fogColor);

                this.receivedMap = map;
                this.assignedMap = new Bitmap(map, (int)(map.Width * assignedZoomFactor), (int)(map.Height * assignedZoomFactor));
                this.receivedMapWidth = this.receivedMap.Width;
                this.receivedMapHeight = this.receivedMap.Height;

                this.RefreshMapPictureBox();
            }
            catch (Exception e)
            {
                Logger.LogError("Map Received Failure", e);
            }
        }

        private void connection_OnCenterMapReceived(SimplePoint centerMap)
        {
            // Take the point that we want to show, and center it on the client's UI.
            SetScroll(centerMap.X - this.Width / 2, centerMap.Y - this.Height / 2);
        }

        private void connection_OnFogReceived(SimpleImage fogImage)
        {
            try
            {
                this.fog = fogImage.Bytes.ToImage();
                if (isBlackoutOn)
                    return;

                RefreshMapPictureBox();
            }
            catch (Exception e)
            {
                Logger.LogError("Fog received failure.", e);
            }
        }

        private void connection_OnFogUpdateReceived(FogUpdate fogUpdate)
        {
            var fogImageToUpdate = this.fog;
            var isNewFogImage = (fogImageToUpdate == null);
            if (isNewFogImage)
                fogImageToUpdate = new Bitmap(this.receivedMapWidth, this.receivedMapHeight);

            using (var g = Graphics.FromImage(fogImageToUpdate))
            {
                if (isNewFogImage)
                    g.FillRectangle(fogBrush, 0, 0, fog.Width, fog.Height);
                g.FillPolygon((fogUpdate.IsClearing) ? fogClearBrush : fogBrush, fogUpdate.Points.Select(p => p.ToPoint()).ToArray());
            }

            if (isNewFogImage)
                this.fog = fogImageToUpdate;

            if (isBlackoutOn)
                return;

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

        private void RefreshMapPictureBox()
        {
            if (pbxMap.InvokeRequired)
            {
                pbxMap.BeginInvoke((Action)RefreshMapPictureBox);
                return;
            }
            // TODO: Invalidate or Refresh? Don't know which makes more sense, or if it matters in this case...
            pbxMap.Invalidate();
        }

        private void pbxMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (isZoomFactorInProgress)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Commit the zoom factor at this point by notifing the Zoom Factor event.
                    isZoomFactorInProgress = false;
                    isZoomFactorRunning = true;
                    pbxMap.Refresh();
                    zoomFactorHandlerEvent.Set();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // Cancel the zoom action being done.
                    variableZoomFactor = assignedZoomFactor;
                    isZoomFactorInProgress = false;
                    pbxMap.Refresh();
                    return;
                }
            }
        }

        private void pbxMap_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.F11 || (e.KeyCode == Keys.Escape && fullScreenAction.Checked))
            {
                fullScreenAction.PerformClick();
                return;
            }

            if (e.Control)
            {
                if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Up)
                    ZoomInOrOut(true, e.Shift);
                else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.Down)
                    ZoomInOrOut(false, e.Shift);
                return;
            }

            if (isZoomFactorInProgress)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    // Cancel the zoom action being done.
                    variableZoomFactor = assignedZoomFactor;
                    isZoomFactorInProgress = false;
                    pbxMap.Refresh();
                    return;
                }
                if (e.KeyCode == Keys.Enter)
                {
                    // Commit the zoom factor at this point by notifing the Zoom Factor event.
                    isZoomFactorInProgress = false;
                    isZoomFactorRunning = true;
                    pbxMap.Refresh();
                    zoomFactorHandlerEvent.Set();
                }
            }

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
            if (this.assignedMap.Width < this.Width)
                desiredX = 0;
            else
                desiredX = Math.Min(desiredX.Value, this.assignedMap.Width - this.Width);

            if (this.assignedMap.Height < this.Height)
                desiredY = 0;
            else
                desiredY = Math.Min(desiredY.Value, this.assignedMap.Height - this.Height);

            this.scrollPosition = new Point(desiredX.Value, desiredY.Value);
            pbxMap.Invalidate();
        }

        private void ZoomInOrOut(bool zoomIn, bool doubleFactor)
        {
            if (zoomIn)
                variableZoomFactor = (float)Math.Round(Math.Min(variableZoomFactor + ((doubleFactor) ? 0.2f : 0.1f), ConfigValues.MaximumGridZoomFactor), 1);
            else
                variableZoomFactor = (float)Math.Round(Math.Max(variableZoomFactor - ((doubleFactor) ? 0.2f : 0.1f), ConfigValues.MinimumGridZoomFactor), 1);

            isZoomFactorInProgress = true;

            pbxMap.Invalidate();
        }
        
        /// <summary> Threaded method that is used to process the zooming factor. </summary>
        /// <remarks>
        ///     By using a thread to handle the Mouse Wheel zooming functionality, we allow the user to implicitly "enqueue" zoom attempts
        ///     and we'll ignore any that happen while we're creating the new bitmap. Then we'll apply it and re-check the Scale Factor to 
        ///     see if our latest build of the bitmap was ok. If not, we'll create one. That means scaling from 1.0 to 10.0 in 0.1 increments
        ///     won't end up generating 100 bitmaps but rather only a subset of them, as any increment steps that are skipped while
        ///     a bitmap is being generated are never consumed.
        /// </remarks>
        private void ZoomFactorHandlerStart()
        {
            do
            {
                try
                {
                    if (isExiting)
                        break;

                    zoomFactorHandlerEvent.WaitOne();
                    if (isExiting)
                        break;

                    if (assignedZoomFactor == variableZoomFactor)
                    {
                        isZoomFactorRunning = false;
                        RefreshMapPictureBox();
                        continue;
                    }

                    // Create the new scaled bitmap
                    var oldMap = this.assignedMap;
                    var newMap = new Bitmap(this.receivedMap, (int)(receivedMapWidth * variableZoomFactor), (int)(receivedMapHeight * variableZoomFactor));

                    // TODO: This likely doesn't need to be Invoked anymore.
                    pbxMap.Invoke(new Action(() =>
                                                 {
                                                     this.assignedMap = newMap;
                                                     assignedZoomFactor = variableZoomFactor;
                                                     isZoomFactorRunning = false;

                                                     // This will validate that the current scroll values aren't too large for the new zoom factor.
                                                     SetScroll(null, null);
                                                 }));
                    if (oldMap != null)
                        oldMap.Dispose();

                    RefreshMapPictureBox();
                }
                catch (ThreadInterruptedException e)
                {
                    Logger.LogInfo("Zoom thread interrupted.", e);
                }
                catch (Exception e)
                {
                    Logger.LogError(string.Format("Zoom catastrophe when trying to zoom from {0} to {1}. Bitmap ctor params: {2}, {3}", assignedZoomFactor, variableZoomFactor, receivedMapWidth * variableZoomFactor, receivedMapHeight * variableZoomFactor), e);
                }
            }
            while (true);
        }

        /// <summary> Repaint event occurs every time we request it, or when the user scrolls. </summary>
        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            if (this.assignedMap == null)
                return;

            var g = e.Graphics;

            // Force clipping to the visible area only. This clipping will be translated as needed in the subsequent calls, but ensures
            // that we never try to draw beyond the visible area.
            g.SetClip(new Rectangle(0, 0, this.Width, this.Height));

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
            g.DrawImage(AssetsLoader.BlackoutImage, this.Width / 2.0f - AssetsLoader.BlackoutImage.Width / 2.0f, this.Height / 2.0f - AssetsLoader.BlackoutImage.Height / 2.0f);
        }

        private void PaintMap(Graphics g)
        {
            if (this.assignedMap != null)
            {
                g.TranslateTransform(-this.scrollPosition.X, -this.scrollPosition.Y);
                g.ScaleTransform(assignedZoomFactor, assignedZoomFactor);
                {
                    g.DrawImage(this.receivedMap, new Rectangle(0, 0, this.receivedMap.Width, this.receivedMap.Height), 0, 0, this.receivedMap.Width, this.receivedMap.Height, GraphicsUnit.Pixel, fogAttributes);
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
                    for (int x = 0; x < receivedMapWidth; x += gridSize.Value)
                    {
                        g.DrawLine(gridPen, x, 0, x, receivedMapHeight);
                    }
                    for (int y = 0; y < receivedMapHeight; y += gridSize.Value)
                    {
                        g.DrawLine(gridPen, 0, y, receivedMapWidth, y);
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
                    g.DrawImage(fog, new Rectangle(0, 0, receivedMapWidth, receivedMapHeight), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
                }
                g.ResetTransform();
            }
        }

        private void PaintZoomFactorText(Graphics g)
        {
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
                    var x = (this.Width / 2.0f) - (msgSize.Width / 2.0f);
                    var y = (this.Height / 2.0f) - (msgSize.Height / 2.0f) + msgSize.Height * i;

                    // If we're also showing the Blackout image, then show the text beneath it.
                    if (isBlackoutOn)
                        y += AssetsLoader.BlackoutImage.Height;

                    g.DrawString(zoomMsgs[i], font, Brushes.White, x, y);
                }
            }
        }

        private Point lastDragPosition;

        private void pbxMap_MouseDown(object sender, MouseEventArgs e)
        {
            lastDragPosition = e.Location;
        }

        private void pbxMap_MouseMove(object sender, MouseEventArgs e)
        {
            const int MoveThreshold = 3;

            if (e.Button != MouseButtons.Left)
                return;

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
}
