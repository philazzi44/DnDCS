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

namespace DnDCS.Client
{
    public partial class ClientControl : UserControl, IDnDCSControl
    {
        private bool isConnected;
        private string initialParentFormText;
        private ClientSocketConnection connection;
        private int mapWidth;
        private int mapHeight;
        private Image map;
        private Image fog;
        private bool isBlackoutOn;
        private int? gridSize;
        private Pen gridPen;

        private readonly SolidBrush fogClearBrush = new SolidBrush(Color.White);

        private readonly Brush fogBrush = Brushes.Black;
        private readonly Color fogColor = Color.Black;

        private readonly ImageAttributes fogAttributes = new ImageAttributes();

        public ClientControl()
        {
            InitializeComponent();
        }
        
        private void ClientControl_Load(object sender, EventArgs e)
        {
            float[][] matrixItems = { new float[] {1, 0, 0, 0, 0},
                                      new float[] {0, 1, 0, 0, 0},
                                      new float[] {0, 0, 1, 0, 0},
                                      new float[] {0, 0, 0, 1, 0}, 
                                      new float[] {0, 0, 0, 0, 1}
                                    };
            fogAttributes.SetColorMatrix(new ColorMatrix(matrixItems), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            fogAttributes.SetColorKey(fogClearBrush.Color, fogClearBrush.Color, ColorAdjustType.Bitmap);

            initialParentFormText = this.ParentForm.Text;
            pnlMap.BackColor = fogColor;
            pbxMap.Paint += new PaintEventHandler(pbxMap_Paint);
            pbxMap.MouseWheel += new MouseEventHandler(pbxMap_MouseWheel);
            this.Disposed += new EventHandler(ClientControl_Disposed);

            Connect();
        }

        private void pbxMap_MouseWheel(object sender, MouseEventArgs e)
        {
            switch (Math.Sign(e.Delta))
            {
                // Zoom in
                case 1:
                    break;

                // Zoom 2
                case -1:
                    break;
            }
        }

        private void ClientControl_Disposed(object sender, EventArgs e)
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

        //protected override void WndProc(ref Message m)
        //{
        //    const int WM_MOUSEWHEEL = 0x020A;

        //    // If mouse wheel moved
        //    switch (m.Msg)
        //    {
        //        case WM_MOUSEWHEEL:
        //            break;
        //        default:
        //            base.WndProc(ref m);
        //            return;
        //    }
        //}

        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            DrawOnGraphics(e.Graphics);
        }

        public MainMenu GetMainMenu()
        {
            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
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

            Image blackoutOrMap;
            if (this.isBlackoutOn)
            {
                blackoutOrMap = new Bitmap(this.mapWidth, this.mapHeight);
                using (var g = Graphics.FromImage(blackoutOrMap))
                {
                    g.Clear(Color.Black);
                }
            }
            else
            {
                blackoutOrMap = this.map;
            }
            pbxMap.BeginInvoke((new Action(() =>
                                               {
                                                   pbxMap.Image = blackoutOrMap;
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

            // Since we received a new map, we'll automatically black out everything with fog until the Server tells us otherwise.
            this.fog = new Bitmap(map.Width, map.Height);
            using (var g = Graphics.FromImage(this.fog))
                g.Clear(fogColor);

            this.map = map;
            this.mapWidth = this.map.Width;
            this.mapHeight = this.map.Height;

            if (isBlackoutOn)
                return;

            pbxMap.BeginInvoke((new Action(() =>
                                               {
                                                   pbxMap.Image = this.map;
                                                   pbxMap.Refresh();
                                               })));
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
                fogImageToUpdate = new Bitmap(this.mapWidth, this.mapHeight);

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

        private void pnlMap_Scroll(object sender, ScrollEventArgs e)
        {
            pbxMap.Refresh();
        }

        private void DrawOnGraphics(Graphics g)
        {
            if (this.map == null)
                return;

            if (this.isBlackoutOn)
            {
                // Draw the Blackout Image in the Top/Left of what is visible to the user.
                g.DrawImage(AssetsLoader.BlackoutImage, pnlMap.HorizontalScroll.Value, pnlMap.VerticalScroll.Value);
                return;
            }

            if (gridSize.HasValue)
            {
                for (int x = 0; x < pbxMap.Width; x += gridSize.Value)
                {
                    g.DrawLine(gridPen, x, 0, x, pbxMap.Height);
                }
                for (int y = 0; y < pbxMap.Height; y += gridSize.Value)
                {
                    g.DrawLine(gridPen, 0, y, pbxMap.Width, y);
                }
            }

            if (fog != null)
                g.DrawImage(fog, new Rectangle(Point.Empty, pbxMap.Size), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
        }
    }
}
