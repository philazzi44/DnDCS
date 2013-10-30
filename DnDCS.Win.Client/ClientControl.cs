using System;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using DnDCS.Win.Libs;

namespace DnDCS.Win.Client
{
    public partial class ClientControl : UserControl, IDnDCSControl
    {
        // Parent Form Communication
        public Action<bool> ToggleFullScreen { get; set; }
        private string initialParentFormText;

        // Menu References
        private MenuItem fullScreenMenuItem;

        // Client Connection
        private ClientSocketConnection connection;

        /// <summary> The Server IP and Port to use at startup. </summary>
        public SimpleServerAddress StartupServerAddress { get; set; }

        #region Init and Cleanup

        public ClientControl()
        {
            InitializeComponent();
        }
        
        private void ClientControl_Load(object sender, EventArgs e)
        {
            initialParentFormText = this.ParentForm.Text;

            this.Disposed += new EventHandler(ClientControl_Disposed);

            this.ctlDnDMap.TryToggleFullScreen += new Action<Keys>(ctlDnDMap_TryToggleFullScreen);
            this.ctlDnDMap.AllowZoom = true;
            this.ctlDnDMap.Init();

            // Use the Name/IP address we had at startup, if any.
            if (StartupServerAddress != null)
                Connect(StartupServerAddress.Address, StartupServerAddress.Port);
            else
                Connect();
        }

        private void ctlDnDMap_TryToggleFullScreen(Keys keyCode)
        {
            if (keyCode == Keys.F11 || (keyCode == Keys.Escape && this.fullScreenMenuItem.Checked))
                this.fullScreenMenuItem.PerformClick();
        }

        private void ClientControl_Disposed(object sender, EventArgs e)
        {
            if (connection != null)
                connection.Stop();
        }

        #endregion Init

        #region Menu and Menu Callbacks

        public MainMenu GetMainMenu()
        {
            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
                fullScreenMenuItem = new MenuItem("Full Screen", OnFullScreen_Click) { Checked = false },
                new MenuItem("-"),
                new MenuItem("Flip View", OnFlipView_Click) { Checked = false },
                new MenuItem("-"),
                new MenuItem("Reconnect", OnReconnect_Click),
                new MenuItem("-"),
                new MenuItem("Exit", OnExit_Click),
            });
            menu.MenuItems.Add(fileMenu);
            return menu;
        }

        private void OnFlipView_Click(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;

            this.ctlDnDMap.IsFlippedView = (menuItem.Checked = !menuItem.Checked);
            this.ctlDnDMap.RefreshAll();
        }

        private void OnFullScreen_Click(object sender, EventArgs e)
        {
            if (ToggleFullScreen == null)
                return;

            var menuItem = sender as MenuItem;

            var goFullScreen = (menuItem.Checked = !menuItem.Checked);
            ToggleFullScreen(goFullScreen);
        }

        private void OnReconnect_Click(object sender, EventArgs e)
        {
            if (this.connection != null)
                this.Connect(this.connection.Address, this.connection.Port);
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
            connection.OnUseFogAlphaEffectReceived += new Action<bool>(connection_OnUseFogAlphaEffectReceived);
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
            this.ctlDnDMap.IsBlackoutOn = isBlackoutOn;
        }

        private void connection_OnMapReceived(SimpleImage mapImage)
        {
            try
            {
                // Since we received a new map, we'll automatically black out everything with fog until the Server tells us otherwise.
                var newMap = mapImage.Bytes.ToImage();
                this.ctlDnDMap.SetMapAsync(newMap);
            }
            catch (Exception e)
            {
                Logger.LogError("Map Received Failure", e);
            }
        }

        private void connection_OnCenterMapReceived(SimplePoint centerMap)
        {
            this.ctlDnDMap.SetCenterMap(centerMap);
        }

        private void connection_OnFogReceived(SimpleImage fogImage)
        {
            try
            {
                var newFog = fogImage.Bytes.ToImage();
                this.ctlDnDMap.SetFogAsync(newFog);
            }
            catch (Exception e)
            {
                Logger.LogError("Fog received failure.", e);
            }
        }

        private void connection_OnFogUpdateReceived(FogUpdate fogUpdate)
        {
            this.ctlDnDMap.SetFogUpdateAsync(fogUpdate);
        }

        private void connection_OnUseFogAlphaEffectReceived(bool useFogAlphaEffect)
        {
            this.ctlDnDMap.UseFogAlphaEffect = useFogAlphaEffect;
        }
        
        private void connection_OnGridSizeReceived(bool showGrid, int gridSize)
        {
            this.ctlDnDMap.SetGridSize(showGrid, gridSize);
        }

        private void connection_OnGridColorReceived(SimpleColor gridColor)
        {
            this.ctlDnDMap.SetGridColor(gridColor);
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
    }
}
