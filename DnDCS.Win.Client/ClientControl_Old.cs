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

namespace DnDCS.Client
{
    public partial class ClientControl_Old : UserControl, IDnDCSControl
    {
        private const float ScrollWheelStepPercent = 0.01f;

        private Point lastScrollPosition = Point.Empty;

        private bool isExiting;

        // Tracks the initial values on the form when we decide to toggle Full Screen mode.
        private bool initialFormTopMost;
        private FormBorderStyle initialFormBorderStyle;
        private FormWindowState initialFormWindowState;

        private float assignedScaleFactor = 1.0f;
        private float variableScaleFactor = 1.0f;

        private bool isConnected;
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
        private bool isScaleFactorInProgress;
        private bool isScaleFactorRunning;
        private Font zoomFactorFont;
        private const string ZoomInstructionMessage = "Press Enter to commit the zoom factor.";

        private MenuItem fullScreenAction;

        public SimplePoint ScrollPosition
        {
            // Must return the individual values for this to work, as AutoScrollPosition getter appears to be wrong for some reason.
            get { return new SimplePoint(this.pnlMap.HorizontalScroll.Value, this.pnlMap.VerticalScroll.Value); }
            set { SetScroll(value.X, value.Y); }
        }

        public Action<bool> ToggleFullScreen { get; set; }

        public ClientControl_Old()
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
            pnlMap.BackColor = fogColor;
            pbxMap.Paint += new PaintEventHandler(pbxMap_Paint);
            pbxMap.MouseWheel += new MouseEventHandler(pbxMap_MouseWheel);
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
            isConnected = true;
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
                this.assignedMap = new Bitmap(map, (int)(map.Width * assignedScaleFactor),
                                              (int)(map.Height * assignedScaleFactor));
                this.receivedMapWidth = this.receivedMap.Width;
                this.receivedMapHeight = this.receivedMap.Height;
                
                pbxMap.BeginInvoke((new Action(() =>
                                                   {
                                                       pbxMap.Image = this.assignedMap;
                                                       pbxMap.Refresh();
                                                   })));
            }
            catch (Exception e)
            {
                Logger.LogError("Map Received Failure", e);
            }
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

        private void pnlMap_Scroll(object sender, ScrollEventArgs e)
        {
            RefreshMapPictureBox();
            lastScrollPosition = new Point(pnlMap.HorizontalScroll.Value, pnlMap.VerticalScroll.Value);
        }

        private void pnlMap_SizeChanged(object sender, EventArgs e)
        {
            pbxMap.MinimumSize = pnlMap.Size;

            // Oh WinForms, you make me laugh. I need to set the value twice for it to actually "stick"...
            pnlMap.HorizontalScroll.Value = lastScrollPosition.X;
            pnlMap.HorizontalScroll.Value = lastScrollPosition.X;
            pnlMap.VerticalScroll.Value = lastScrollPosition.Y;
            pnlMap.VerticalScroll.Value = lastScrollPosition.Y;

            RefreshMapPictureBox();
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

            if (isScaleFactorInProgress)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Commit the zoom factor at this point by notifing the Zoom Factor event.
                    isScaleFactorInProgress = false;
                    isScaleFactorRunning = true;
                    zoomFactorHandlerEvent.Set();
                    pbxMap.Refresh();
                }
            }

            // TODO: This doesn't work because Win Forms is being a pain as far as focused controls.
            // switch (e.KeyCode)
            // {
            //     case Keys.Up:
            //         ScrollUpOrDown(true);
            //         pbxMap.Refresh();
            //         break;
            //     case Keys.Down:
            //         ScrollUpOrDown(false);
            //         pbxMap.Refresh();
            //         break;
            // }
               
            // switch (e.KeyCode)
            // {
            //     case Keys.Left:
            //         ScrollLeftOrRight(true);
            //         pbxMap.Refresh();
            //         break;
            //     case Keys.Right:
            //         ScrollLeftOrRight(false);
            //         pbxMap.Refresh();
            //         break;
            // }
        }

        private void ScrollLeftOrRight(bool isLeft)
        {
            // Scroll left/right
            int newValue;
            if (isLeft)
                newValue = Math.Max(pnlMap.HorizontalScroll.Value - (int)(pbxMap.Width * ScrollWheelStepPercent), pnlMap.HorizontalScroll.Minimum);
            else
                newValue = Math.Min(pnlMap.HorizontalScroll.Value + (int)(pbxMap.Width * ScrollWheelStepPercent), pnlMap.HorizontalScroll.Maximum);
            SetScroll(newValue, null);
        }

        private void ScrollUpOrDown(bool isUp)
        {
            // Scroll up/down
            int newValue;
            if (isUp)
                newValue = Math.Max(pnlMap.VerticalScroll.Value - (int)(pbxMap.Width * ScrollWheelStepPercent), pnlMap.VerticalScroll.Minimum);
            else
                newValue = Math.Min(pnlMap.VerticalScroll.Value + (int)(pbxMap.Width * ScrollWheelStepPercent), pnlMap.VerticalScroll.Maximum);
            SetScroll(null, newValue);
        }

        private void pbxMap_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                var isControl = Control.ModifierKeys.HasFlag(Keys.Control);
                var isShift = Control.ModifierKeys.HasFlag(Keys.Shift);

                if (isControl)
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

        private void SetScroll(int? x, int? y)
        {
            // Oh WinForms, you make me laugh. I need to set the value twice for it to actually "stick"...
            if (x.HasValue)
            {
                pnlMap.HorizontalScroll.Value = x.Value;
                pnlMap.HorizontalScroll.Value = x.Value;
            }
            if (y.HasValue)
            {
                pnlMap.VerticalScroll.Value = y.Value;
                pnlMap.VerticalScroll.Value = y.Value;
            }

            lastScrollPosition = new Point(x ?? pnlMap.HorizontalScroll.Value, y ?? pnlMap.VerticalScroll.Value);
        }

        private void ZoomInOrOut(bool zoomIn, bool doubleFactor)
        {
            if (zoomIn)
                variableScaleFactor = (float)Math.Round(Math.Min(variableScaleFactor + ((doubleFactor) ? 0.2f : 0.1f), ConfigValues.MaximumGridZoomFactor), 1);
            else
                variableScaleFactor = (float)Math.Round(Math.Max(variableScaleFactor - ((doubleFactor) ? 0.2f : 0.1f), ConfigValues.MinimumGridZoomFactor), 1);

            isScaleFactorInProgress = true;

            pbxMap.Refresh();
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

                    if (assignedScaleFactor == variableScaleFactor)
                        continue;

                    // Create the new scaled bitmap
                    var oldMap = this.assignedMap;
                    var newMap = new Bitmap(this.receivedMap, (int)(receivedMapWidth * variableScaleFactor), (int)(receivedMapHeight * variableScaleFactor));

                    pbxMap.Invoke(new Action(() =>
                                                 {
                                                     pbxMap.Image = this.assignedMap = newMap;
                                                     assignedScaleFactor = variableScaleFactor;
                                                     isScaleFactorRunning = false;
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
                    Logger.LogError(string.Format("Zoom catastrophe when trying to zoom from {0} to {1}. Bitmap ctor params: {2}, {3}", assignedScaleFactor, variableScaleFactor, receivedMapWidth * variableScaleFactor, receivedMapHeight * variableScaleFactor), e);
                }
            }
            while (true);
        }

        /// <summary> Repaint event occurs every time we request it, or when the user scrolls. </summary>
        /// <remarks> TODO: Only need to realistically draw what the user can see. </remarks>
        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            if (this.assignedMap == null)
                return;

            var g = e.Graphics;

            if (this.isBlackoutOn)
            {
                PaintBlackout(g);
            }
            else
            {
                PaintGrid(g);
                PaintFog(g);
            }

            PaintZoomFactorText(g);
        }

        private void PaintBlackout(Graphics g)
        {
            // These scrolling offsets tell us the top/left of any image we need to draw.
            var scrollOffsetX = this.pnlMap.HorizontalScroll.Value;
            var scrollOffsetY = this.pnlMap.VerticalScroll.Value;

            g.TranslateTransform(scrollOffsetX, scrollOffsetY);
            {
                // Draw the Blackout Image in the center of what is visible to the user.
                g.Clear(Color.Black);
                g.DrawImage(AssetsLoader.BlackoutImage, pnlMap.Width / 2.0f - AssetsLoader.BlackoutImage.Width / 2.0f, pnlMap.Height / 2.0f - AssetsLoader.BlackoutImage.Height / 2.0f);
            }
            g.ResetTransform();
        }

        private void PaintGrid(Graphics g)
        {
            // Because Paint events are sometimes scattered, we'll just draw the whole Grid rather than only part of it so there are no gaps.
            // Since our Grid Size is usually pretty big, this will never end up with more than maybe a hundred iterations.
            if (gridSize.HasValue)
            {
                g.ScaleTransform(assignedScaleFactor, assignedScaleFactor);
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
                // These scrolling offsets tell us the top/left of any image we need to draw.
                //var scrollOffsetX = this.pnlMap.HorizontalScroll.Value;
                //var scrollOffsetY = this.pnlMap.VerticalScroll.Value;

                g.ScaleTransform(assignedScaleFactor, assignedScaleFactor);
                {
                    g.DrawImage(fog, new Rectangle(0, 0, receivedMapWidth, receivedMapHeight), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
                    // TODO: This kind of failed during heavy load.
                    //g.DrawImage(fog, // Draw this
                    //            new Rectangle(scrollOffsetX, scrollOffsetY, pnlMap.Width, pnlMap.Height), // Onto this area
                    //            scrollOffsetX, scrollOffsetY, pnlMap.Width, pnlMap.Height, // From this area
                    //            GraphicsUnit.Pixel, // In Pixel units
                    //            fogAttributes); // With Alpha shading
                }
                g.ResetTransform();
            }
        }

        private void PaintZoomFactorText(Graphics g)
        {
            // These scrolling offsets tell us the top/left of any image we need to draw.
            var scrollOffsetX = this.pnlMap.HorizontalScroll.Value;
            var scrollOffsetY = this.pnlMap.VerticalScroll.Value;

            string[] zoomMsgs = null;
            if (isScaleFactorInProgress)
                zoomMsgs = new[] { string.Format("Zoom: {0}x", variableScaleFactor), ZoomInstructionMessage };
            else if (isScaleFactorRunning)
                zoomMsgs = new[] { string.Format("Zooming to {0}x...", variableScaleFactor) };
            if (zoomMsgs != null)
            {
                g.TranslateTransform(scrollOffsetX, scrollOffsetY);
                {
                    var font = this.zoomFactorFont ?? System.Drawing.SystemFonts.DefaultFont;
                    for (var i = 0; i < zoomMsgs.Length; i++)
                    {
                        // Draw each line one after the other, separating them by the height of the message, centered on the screen.
                        var msgSize = g.MeasureString(zoomMsgs[i], font);
                        var x = (pnlMap.Width / 2.0f) - (msgSize.Width / 2.0f);
                        var y = (pnlMap.Height / 2.0f) - (msgSize.Height / 2.0f) + msgSize.Height * i;

                        // If we're also showing the Blackout image, then show the text beneath it.
                        if (isBlackoutOn)
                            y += AssetsLoader.BlackoutImage.Height;

                        g.DrawString(zoomMsgs[i], font, Brushes.White, x, y);
                    }
                }
                g.ResetTransform();
            }
        }
    }
}
