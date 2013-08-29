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

namespace DnDCS.Client
{
    public partial class ClientControl : UserControl, IDnDCSControl
    {
        private ClientSocketConnection connection;
        private MenuItem connectItem;
        private Image map;
        private Image fog;
        private bool isBlackoutOn;

        // These two colors should be the same so the transparency works as expected.
        private readonly Brush fogClearBrush = Brushes.White;
        private readonly Color fogClearColor = Color.White;

        private readonly Brush fogBrush = Brushes.Black;

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
            fogAttributes.SetColorKey(fogClearColor, fogClearColor, ColorAdjustType.Bitmap);

            pbxMap.Paint += new PaintEventHandler(pbxMap_Paint);
        }

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
                connectItem = new MenuItem("Connect", OnConnect_Click),
                new MenuItem("Exit", OnExit_Click),
            });
            menu.MenuItems.Add(fileMenu);
            return menu;
        }

        private void OnConnect_Click(object sender, EventArgs e)
        {
            // Prompt for Name/IP address
            using (var getConnectIP = new GetConnectIPDialog())
            {
                if (getConnectIP.ShowDialog() == DialogResult.OK)
                {
                    Connect(getConnectIP.Address, getConnectIP.Port);
                }
            }
        }

        private void OnExit_Click(object sender, EventArgs e)
        {
            if (connection != null)
                connection.Stop();
            this.ParentForm.Close();
        }

        private void Connect(string address, int port)
        {
            if (connection != null)
                connection.Stop();

            connection = new ClientSocketConnection(address, port);
            connection.OnMapReceived += new Action<Image>(connection_OnMapReceived);
            connection.OnFogReceived += new Action<Image>(connection_OnFogReceived);
            connection.OnFogUpdateReceived += new Action<Point[], bool>(connection_OnFogUpdateReceived);
            connection.OnBlackoutReceived += new Action<bool>(connection_OnBlackoutReceived);
            connection.OnExitReceived += new Action(connection_OnExitReceived);

            connectItem.Text = "Connected...";
            connectItem.Enabled = false;
        }

        private void connection_OnBlackoutReceived(bool isBlackoutOn)
        {
            this.isBlackoutOn = isBlackoutOn;
            pbxMap.BeginInvoke((new Action(() =>
                                               {
                                                   pbxMap.Image = (this.isBlackoutOn) ? null : this.map;
                                                   pbxMap.Refresh();
                                               })));
        }

        private void connection_OnMapReceived(Image map)
        {
            this.map = map;
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

            pbxMap.BeginInvoke((new Action(() =>
                                               {
                                                   pbxMap.Refresh();
                                               })));
        }

        private void connection_OnFogUpdateReceived(Point[] fogUpdate, bool isClearing)
        {
            var isNewFogImage = (this.fog == null);
            if (isNewFogImage)
                this.fog = new Bitmap(this.map.Width, this.map.Height);

            using (var g = Graphics.FromImage(this.fog))
            {
                if (isNewFogImage)
                    g.FillRectangle(fogBrush, 0, 0, fog.Width, fog.Height);
                g.FillPolygon((isClearing) ? fogClearBrush : fogBrush, fogUpdate);
            }

            if (isBlackoutOn)
                return;

            pbxMap.BeginInvoke((new Action(() =>
            {
                pbxMap.Refresh();
            })));
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

        private void DrawOnGraphics(Graphics g)
        {
            if (this.isBlackoutOn)
                return;

            if (fog == null)
                return;

            g.DrawImage(fog, new Rectangle(Point.Empty, pbxMap.Size), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
        }
    }
}
