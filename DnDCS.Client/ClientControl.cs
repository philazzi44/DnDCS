using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.SocketObjects;
using System.Drawing.Imaging;
using DnDCS.Libs.Assets;
using System.Threading;

namespace DnDCS.Client
{
    public partial class ClientControl : UserControl, IDnDCSControl
    {
        private const int MouseWheelDelayInterval = 500;

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

        private System.Threading.Timer mouseWheelHandlerDelayStart;
        private bool drawScaleFactor;

        public Point ScrollPosition
        {
            // Must return the individual values for this to work, as AutoScrollPosition getter appears to be wrong for some reason.
            get { return new Point(this.pnlMap.HorizontalScroll.Value, this.pnlMap.VerticalScroll.Value); }
            set
            {
                // Oh WinForms, you make me laugh. I need to set the value twice for it to actually "stick"...
                this.pnlMap.AutoScrollPosition = value;
                this.pnlMap.AutoScrollPosition = value;
            }
        }

        public ClientControl()
        {
            InitializeComponent();
        }
        
        private void ClientControl_Load(object sender, EventArgs e)
        {
            fogAttributes.SetColorKey(fogClearBrush.Color, fogClearBrush.Color, ColorAdjustType.Bitmap);

            mouseWheelHandlerDelayStart = new System.Threading.Timer(MouseWheelHandlerDelayStart);

            initialParentFormText = this.ParentForm.Text;
            this.BackColor = fogColor;
            pnlMap.BackColor = fogColor;
            pbxMap.Paint += new PaintEventHandler(pbxMap_Paint);
            pbxMap.MouseWheel += new MouseEventHandler(pbxMap_MouseWheel);
            this.Disposed += new EventHandler(ClientControl_Disposed);

            pbxMap.Focus();
            Connect();
        }

        private void ClientControl_Disposed(object sender, EventArgs e)
        {
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
            if (mouseWheelHandlerDelayStart != null)
                mouseWheelHandlerDelayStart.Dispose();
        }
        
        public MainMenu GetMainMenu()
        {
            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
                new MenuItem("Force Focus Map", new EventHandler((o, e) => pbxMap.Focus())),
                new MenuItem("-"),
                new MenuItem("Exit", OnExit_Click),
            });
            menu.MenuItems.Add(fileMenu);
            return menu;
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
            connection.OnMapReceived += new Action<Image>(connection_OnMapReceived);
            connection.OnFogReceived += new Action<Image>(connection_OnFogReceived);
            connection.OnFogUpdateReceived += new Action<Point[], bool>(connection_OnFogUpdateReceived);
            connection.OnGridSizeReceived += new Action<bool, int>(connection_OnGridSizeReceived);
            connection.OnGridColorReceived += new Action<Color>(connection_OnGridColorReceived);
            connection.OnBlackoutReceived += new Action<bool>(connection_OnBlackoutReceived);
            connection.OnExitReceived += new Action(connection_OnExitReceived);

            this.ParentForm.Text = string.Format("{0} - Connecting to {1}:{2}...", initialParentFormText, address, port);
        }

        private void connection_OnBlackoutReceived(bool isBlackoutOn)
        {
            this.isBlackoutOn = isBlackoutOn;
            pbxMap.BeginInvoke((new Action(() =>
                                               {
                                                   pbxMap.Image = (this.isBlackoutOn) ? null : this.assignedMap;
                                                   pbxMap.Refresh();
                                               })));
        }

        private void connection_OnMapReceived(Image map)
        {
            if (!isConnected)
            {
                isConnected = true;
                this.ParentForm.BeginInvoke(new Action(() =>
                {
                    this.ParentForm.Text = string.Format("{0} - Connected to {1}:{2}", initialParentFormText, connection.Address, connection.Port);
                }));
            }

            try
            {

                // Since we received a new map, we'll automatically black out everything with fog until the Server tells us otherwise.
                this.fog = new Bitmap(map.Width, map.Height);
                using (var g = Graphics.FromImage(this.fog))
                    g.Clear(fogColor);

                this.receivedMap = map;
                this.assignedMap = new Bitmap(map, (int)(map.Width * assignedScaleFactor),
                                              (int)(map.Height * assignedScaleFactor));
                this.receivedMapWidth = this.receivedMap.Width;
                this.receivedMapHeight = this.receivedMap.Height;

                if (isBlackoutOn)
                    return;

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

        private void connection_OnFogReceived(Image fog)
        {
            this.fog = fog;
            if (isBlackoutOn)
                return;

            RefreshMapPictureBox();
        }

        private void connection_OnFogUpdateReceived(Point[] fogUpdate, bool isClearing)
        {
            var fogImageToUpdate = this.fog;
            var isNewFogImage = (fogImageToUpdate == null);
            if (isNewFogImage)
                fogImageToUpdate = new Bitmap(this.receivedMapWidth, this.receivedMapHeight);

            using (var g = Graphics.FromImage(fogImageToUpdate))
            {
                if (isNewFogImage)
                    g.FillRectangle(fogBrush, 0, 0, fog.Width, fog.Height);
                g.FillPolygon((isClearing) ? fogClearBrush : fogBrush, fogUpdate);
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

        private void connection_OnGridColorReceived(Color gridColor)
        {
            if (gridPen != null)
                gridPen.Dispose();
            gridPen = new Pen(gridColor);

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
            pbxMap.Refresh();
        }

        private void pbxMap_MouseWheel(object sender, MouseEventArgs e)
        {
            if (this.isBlackoutOn)
                return;

            if (!Control.ModifierKeys.HasFlag(Keys.Control))
                return;

            var doubleScale = Control.ModifierKeys.HasFlag(Keys.Shift);
            switch (Math.Sign(e.Delta))
            {
                // Zoom in
                case 1:
                    variableScaleFactor = (float)Math.Round(Math.Min(variableScaleFactor + ((doubleScale) ? 0.2f : 0.1f), ConfigValues.MaximumGridZoomFactor), 1);
                    break;

                // Zoom out
                case -1:
                    variableScaleFactor = (float)Math.Round(Math.Max(variableScaleFactor - ((doubleScale) ? 0.2f : 0.1f), ConfigValues.MinimumGridZoomFactor), 1);
                    break;

                default:
                    return;
            }

            // Setup the delayed handler, so we only generate the new image after the mouse wheel sits idle for half a second, in 
            // case the user is actively scrolling.
            mouseWheelHandlerDelayStart.Change(MouseWheelDelayInterval, Timeout.Infinite);
            drawScaleFactor = true;
            pbxMap.Refresh();

            ((HandledMouseEventArgs)e).Handled = true;
        }
        
        /// <summary> Timer event that is raised after a period of inactivity by the user and his precious mouse wheel. </summary>
        /// <remarks>
        ///     By using a timer/thread to handle the Mouse Wheel zooming functionality, we allow the user to implicitly "enqueue" zoom attempts
        ///     and we'll ignore any that happen while we're creating the new bitmap. Then we'll apply it and re-check the Scale Factor to 
        ///     see if our latest build of the bitmap was ok. If not, we'll create one. That means scaling from 1.0 to 10.0 in 0.1 increments
        ///     won't end up generating 100 bitmaps but rather only a subset of them, as any increment steps that are skipped while
        ///     a bitmap is being generated are never consumed.
        /// </remarks>
        private void MouseWheelHandlerDelayStart(object state)
        {
            try
            {
                var oldMap = this.assignedMap;
                var newMap = new Bitmap(this.receivedMap, (int)(receivedMapWidth * variableScaleFactor), (int)(receivedMapHeight * variableScaleFactor));
                pbxMap.Invoke(new Action(() =>
                {
                    pbxMap.Image = this.assignedMap = newMap;
                    assignedScaleFactor = variableScaleFactor;
                    drawScaleFactor = false;
                    //pbxMap.Refresh();
                }));
                if (oldMap != null)
                    oldMap.Dispose();

                RefreshMapPictureBox();
            }
            catch (Exception e)
            {
                Logger.LogError("Zoom catastrophe!", e);
            }
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
                // Draw the Blackout Image in the Top/Left of what is visible to the user.
                g.DrawImage(AssetsLoader.BlackoutImage, pnlMap.HorizontalScroll.Value, pnlMap.VerticalScroll.Value);
                return;
            }

            // These scrolling offsets tell us the top/left of any image we need to draw.
            var scrollOffsetX = this.pnlMap.HorizontalScroll.Value;
            var scrollOffsetY = this.pnlMap.VerticalScroll.Value;

            g.ScaleTransform(assignedScaleFactor, assignedScaleFactor);

            // Because Paint events are sometimes scattered, we'll just draw the whole Grid rather than only part of it so there are no gaps.
            // Since our Grid Size is usually pretty big, this will never end up with more than maybe a hundred iterations.
            if (gridSize.HasValue)
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

            if (fog != null)
            {
                g.DrawImage(fog, new Rectangle(0, 0, receivedMapWidth, receivedMapHeight), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
                // TODO: This kind of failed during heavy load.
                //g.DrawImage(fog, // Draw this
                //            new Rectangle(scrollOffsetX, scrollOffsetY, pnlMap.Width, pnlMap.Height), // Onto this area
                //            scrollOffsetX, scrollOffsetY, pnlMap.Width, pnlMap.Height, // From this area
                //            GraphicsUnit.Pixel, // In Pixel units
                //            fogAttributes); // With Alpha shading
            }

            if (drawScaleFactor)
            {
                g.ResetTransform();
                g.DrawString(string.Format("Zoom: {0}x", variableScaleFactor), System.Drawing.SystemFonts.DefaultFont, Brushes.White, pnlMap.HorizontalScroll.Value, pnlMap.VerticalScroll.Value);
            }
        }
    }
}
