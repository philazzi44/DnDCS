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

namespace DnDCS.Client
{
    public partial class ClientControl : UserControl, IDnDCSControl
    {
        private ClientSocketConnection connection;
        private MenuItem connectItem;
        private Image map;
        private Image fog;
        private bool isBlackoutOn;

        private readonly Brush fogClearBrush = Brushes.Transparent;
        private readonly Brush fogBrush = Brushes.Black;

        public ClientControl()
        {
            InitializeComponent();
        }

        private void ClientControl_Load(object sender, EventArgs e)
        {
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
                    Connect(getConnectIP.Address);
                }
            }
        }

        private void OnExit_Click(object sender, EventArgs e)
        {
            if (connection != null)
                connection.Stop();
            this.ParentForm.Close();
        }

        private void Connect(string address)
        {
            if (connection != null)
                connection.Stop();

            connection = new ClientSocketConnection(address, SocketConstants.Port);
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
            pbxMap.BeginInvoke((new Action(() =>
                                               {
                                                   pbxMap.Image = this.map;
                                                   pbxMap.Refresh();
                                               })));
        }

        private void connection_OnFogReceived(Image fog)
        {
            this.fog = fog;
            pbxMap.BeginInvoke((new Action(() =>
                                               {
                                                   pbxMap.Refresh();
                                               })));
        }

        private void connection_OnFogUpdateReceived(Point[] fogUpdate, bool isClearing)
        {
            /*
             * 
ERROR @ 2013-08-28T17:04:02
Message: Client Socket - An error occurred connecting to the server.
Exception: System.OverflowException: Overflow error.
   at System.Drawing.Graphics.CheckErrorStatus(Int32 status)
   at System.Drawing.Graphics.FillPolygon(Brush brush, Point[] points, FillMode fillMode)
   at System.Drawing.Graphics.FillPolygon(Brush brush, Point[] points)
   at DnDCS.Client.ClientControl.connection_OnFogUpdateReceived(Point[] fogUpdate, Boolean isClearing) in C:\Users\pazzi\Documents\GitHub\DnDCS\DnDCS.Client\ClientControl.cs:line 127
   at DnDCS.Libs.ClientSocketConnection.Start() in C:\Users\pazzi\Documents\GitHub\DnDCS\DnDCS.Libs\ClientSocketConnection.cs:line 84
             * */

            var isNewFogImage = (this.fog == null);
            if (isNewFogImage)
                this.fog = new Bitmap(this.map.Width, this.map.Height);

            using (var g = Graphics.FromImage(this.fog))
            {
                if (isNewFogImage)
                    g.FillRectangle(fogBrush, 0, 0, fog.Width, fog.Height);
                g.FillPolygon((isClearing) ? fogClearBrush : fogBrush, fogUpdate);
            }
        }

        private void DrawOnGraphics(Graphics g)
        {
            if (this.isBlackoutOn)
                return;

            if (fog == null)
                return;

            g.DrawImage(fog, new Rectangle(Point.Empty, pbxMap.Size), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel);
        }

        private void connection_OnExitReceived()
        {
            MessageBox.Show(this, "The server has closed the connection. Client application will now close.",
                            "Server Connection Closed", MessageBoxButtons.OK);
            this.ParentForm.Close();
        }
    }
}
