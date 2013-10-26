using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.PersistenceObjects;
using DnDCS.Libs.ServerEvents;
using DnDCS.Libs.SimpleObjects;
using DnDCS.Win.Libs;

namespace DnDCS.Win.Server
{
    public partial class ServerControl : UserControl, IDnDCSControl
    {
        // Parent Form Communication
        private MenuItem fullScreenMenuItem;
        public Action<bool> ToggleFullScreen { get; set; }
        private string initialParentFormText;

        private string mapUrl;

        // Menu References
        private MenuItem loadImage;
        private MenuItem undoLastFogAction;
        private MenuItem redoLastFogAction;

        // Server Connection
        private ServerSocketConnection connection;

        #region Init and Cleanup

        public ServerControl()
        {
            InitializeComponent();
        }
        
        private void ServerControl_Load(object sender, EventArgs e)
        {
            initialParentFormText = this.ParentForm.Text;

            this.Disposed += new EventHandler(ServerControl_Disposed);

            this.ParentForm.Text = initialParentFormText + " (0 clients connected)";

            this.ctlDnDMap.AllowZoom = false;
            this.ctlDnDMap.PerformCenterMap += new Action<SimplePoint>(ctlDnDMap_PerformCenterMap);
            this.ctlDnDMap.OnOneFogUpdatesChanged += new Action<FogUpdate>(ctlDnDMap_OnOneFogUpdatesChanged);
            this.ctlDnDMap.OnManyFogUpdatesChanged += new Action(ctlDnDMap_OnManyFogUpdatesChanged);
            this.ctlDnDMap.FogAlpha = DnDMapConstants.DEFAULT_FOG_BRUSH_ALPHA;
            this.ctlDnDMap.TryToggleFullScreen += new Action<Keys>(ctlDnDMap_TryToggleFullScreen);
            this.ctlDnDMap.Init();

            var serverData = Persistence.LoadServerData();
            this.ctlDnDMap.UseFogAlphaEffect = serverData.UseFogAlphaEffect;
            if (serverData.IsGridColorSet)
                this.ctlDnDMap.GridPen = new Pen(Color.FromArgb(serverData.GridColorA, serverData.GridColorR, serverData.GridColorG, serverData.GridColorB));

            connection = new ServerSocketConnection(ConfigValues.DefaultServerPort);
            connection.OnClientConnected += connection_OnClientConnected;
            connection.OnClientCountChanged += new Action<int>(connection_OnClientCountChanged);
            connection.OnSocketEvent += new Action<ServerEvent>(connection_OnSocketEvent);

            this.ctlControlPanel.Connection = connection;
            this.ctlControlPanel.DnDMapControl = this.ctlDnDMap;
            this.ctlControlPanel.LoadImageMenuItem = this.loadImage;
            this.ctlControlPanel.UndoLastFogActionMenuItem = this.undoLastFogAction;
            this.ctlControlPanel.RedoLastFogActionMenuItem = this.redoLastFogAction;
            this.ctlControlPanel.Init();
        }

        private void ctlDnDMap_TryToggleFullScreen(Keys keyCode)
        {
            if (keyCode == Keys.F11 || (keyCode == Keys.Escape && this.fullScreenMenuItem.Checked))
                this.fullScreenMenuItem.PerformClick();
        }

        private void ServerControl_Disposed(object sender, EventArgs e)
        {
            if (connection != null)
                connection.Stop();
        }

        #endregion Init and Cleanup

        #region Connection Logic and Callbacks

        private void connection_OnClientConnected()
        {
            if (connection.IsStopping)
                return;
            SendAll(true);
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

        private void connection_OnSocketEvent(ServerEvent socketEvent)
        {
            this.ctlControlPanel.AppendToUILog(socketEvent.ToString());
        }

        private void SendAll(bool sendBlackout)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { SendAll(sendBlackout); }));
                return;
            }

            if (!this.ctlControlPanel.IsBlackOutSet)
                this.ctlControlPanel.ToggleBlackout();
            else
                connection.WriteBlackout(true);

            if (this.ctlDnDMap.LoadedMap != null)
                connection.WriteMap(this.ctlDnDMap.LoadedMap);
            if (this.ctlDnDMap.Fog != null)
                connection.WriteFog(this.ctlDnDMap.Fog);
            connection.WriteUseFogAlphaEffect(this.ctlDnDMap.UseFogAlphaEffect);
            connection.WriteGridSize(this.ctlControlPanel.ShowGrid, this.ctlControlPanel.ShowGrid ? this.ctlControlPanel.GridSize : 0);
            connection.WriteGridColor(this.ctlDnDMap.GridPen.Color.ToSocketColor());
        }

        #endregion Connection Logic and Callbacks

        #region Menu and Menu Callbacks

        public MainMenu GetMainMenu()
        {
            var serverData = Persistence.LoadServerData();

            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
                loadImage = new MenuItem("Load Image", OnLoadImage_Click, Shortcut.CtrlShiftO),
                new MenuItem("-"),
                fullScreenMenuItem = new MenuItem("Full Screen", OnFullScreen_Click) { Checked = false },
                new MenuItem("-"),
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
                new MenuItem("Use Fog Alpha Effect", OnUseFogAlphaEffect_Click) { Checked = serverData.UseFogAlphaEffect },

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
                if (result == DialogResult.OK && loadImage.LoadedImageUrl != this.mapUrl)
                {
                    var log = string.Format("Loaded image url '{0}'.", loadImage.LoadedImageUrl);
                    Logger.LogInfo(log);
                    this.ctlControlPanel.AppendToUILog(log);
                    SetMapImage(loadImage.LoadedImageUrl, loadImage.LoadedImage);

                    var hasFogData = Persistence.PeekServerFogData(loadImage.LoadedImageUrl);
                    if (hasFogData)
                    {
                        var useMapData = MessageBox.Show(this, "Map has been loaded before. Would you like to reload the revealed fog?", "Load Fog Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                        if (useMapData)
                        {
                            var fogData = Persistence.LoadServerFogData(loadImage.LoadedImageUrl);
                            if (fogData == null)
                            {
                                TryPurgeMapData(loadImage.LoadedImageUrl);
                            }
                            else
                            {
                                var fogUpdates = fogData.Data.ToFogUpdate();
                                this.ctlDnDMap.SetFogUpdates(fogUpdates);
                                // Seems easier to just blast out the full fog, rather than a series of Fog Updates.
                                connection.WriteFog(this.ctlDnDMap.Fog);
                            }
                        }
                        else
                        {
                            TryPurgeMapData(loadImage.LoadedImageUrl);
                        }
                    }
                }
            }
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
            // TODO: Prompt for save and save if needed.
            connection.Stop();
            this.ParentForm.Close();
        }

        private void OnUndoLastFogAction_Click(object sender, EventArgs e)
        {
            var lastFogAction = this.ctlDnDMap.TryUndoLastFogAction();
            undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
            redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

            if (lastFogAction != null && this.ctlControlPanel.RealTimeFogUpdates)
                connection.WriteFogUpdate(lastFogAction);
        }

        private void OnRedoLastFogAction_Click(object sender, EventArgs e)
        {
            var lastFogAction = this.ctlDnDMap.TryRedoLastFogAction();
            undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
            redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

            if (lastFogAction != null && this.ctlControlPanel.RealTimeFogUpdates)
                connection.WriteFogUpdate(lastFogAction);
        }

        private void OnUseFogAlphaEffect_Click(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;

            var useFogAlphaEffect = (menuItem.Checked = !menuItem.Checked);

            this.ctlDnDMap.UseFogAlphaEffect = useFogAlphaEffect;
            var serverData = Persistence.LoadServerData();
            serverData.UseFogAlphaEffect = useFogAlphaEffect;
            Persistence.SaveServerData(serverData);

            connection.WriteUseFogAlphaEffect(useFogAlphaEffect);
        }

        private void OnRealTimeFogUpdates_Click(object sender, EventArgs e)
        {
            this.ctlControlPanel.SetSelectTool();

            var menuItem = (MenuItem)sender;
            this.ctlControlPanel.RealTimeFogUpdates = menuItem.Checked = !menuItem.Checked;

            var serverData = Persistence.LoadServerData();
            serverData.RealTimeFogUpdates = this.ctlControlPanel.RealTimeFogUpdates;
            Persistence.SaveServerData(serverData);
        }

        private void OnShowGridValues_Click(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var showGridValues = menuItem.Checked = !menuItem.Checked;
            this.ctlControlPanel.ShowGridValues = showGridValues;

            var serverData = Persistence.LoadServerData();
            serverData.ShowGridValues = showGridValues;
            Persistence.SaveServerData(serverData);
        }

        private void OnShowLog_Click(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var showLogValues = menuItem.Checked = !menuItem.Checked;
            this.ctlControlPanel.ShowLogValues = showLogValues;

            var serverData = Persistence.LoadServerData();
            serverData.ShowLog = showLogValues;
            Persistence.SaveServerData(serverData);
        }

        private void OnSetColorOptions_Click(object sender, EventArgs e)
        {
            using (var colorOptions = new ColorOptionsDialog())
            {
                var serverData = Persistence.LoadServerData();

                colorOptions.GridLineColor = this.ctlDnDMap.GridPen.Color;
                if (colorOptions.ShowDialog(this) == DialogResult.OK)
                {
                    var newColor = colorOptions.GridLineColor;
                    this.ctlDnDMap.GridPen = new Pen(newColor);

                    serverData.GridColorA = newColor.A;
                    serverData.GridColorR = newColor.R;
                    serverData.GridColorG = newColor.G;
                    serverData.GridColorB = newColor.B;
                    serverData.IsGridColorSet = true;

                    Persistence.SaveServerData(serverData);

                    connection.WriteGridColor(colorOptions.GridLineColor.ToSocketColor());

                    this.ctlDnDMap.SetGridColor(colorOptions.GridLineColor.ToSocketColor());
                    this.ctlDnDMap.RefreshAll();
                }
            }
        }

        #endregion Menu and Menu Callbacks

        #region DnD Map Events

        private void ctlDnDMap_PerformCenterMap(SimplePoint centerMap)
        {
            connection.WriteCenterMap(centerMap);
        }

        private void ctlDnDMap_OnOneFogUpdatesChanged(FogUpdate fogUpdate)
        {
            undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
            redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

            if (this.ctlControlPanel.RealTimeFogUpdates)
                connection.WriteFogUpdate(fogUpdate);

            Persistence.SaveServerFogData(this.mapUrl, new ServerFogData()
            {
                Data = this.ctlDnDMap.AllFogUpdates.Select(f => new FogData()
                                                           {
                                                               IsClearing = f.IsClearing,
                                                               Points = f.Points
                                                           }).ToArray()
            });
        }

        private void ctlDnDMap_OnManyFogUpdatesChanged()
        {
            undoLastFogAction.Enabled = this.ctlDnDMap.AnyUndoFogUpdates;
            redoLastFogAction.Enabled = this.ctlDnDMap.AnyRedoFogUpdates;

            connection.WriteFog(this.ctlDnDMap.Fog);

            Persistence.SaveServerFogData(this.mapUrl, new ServerFogData()
            {
                Data = this.ctlDnDMap.AllFogUpdates.Select(f => new FogData()
                                                           {
                                                               IsClearing = f.IsClearing,
                                                               Points = f.Points
                                                           }).ToArray()
            });
        }

        #endregion DnD Map Events

        private void TryPurgeMapData(string imageUrl)
        {
            var purgeMapData = MessageBox.Show(this, "Would you like to purge the previously stored fog? Otherwise, when drawing on the map, you will overwrite it.", "Purge Map Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            if (purgeMapData)
            {
                Persistence.SaveServerFogData(imageUrl, null);
            }
        }
        
        private void SetMapImage(string imageUrl, Image mapImage)
        {
            if (mapImage == null)
                return;

            mapUrl = imageUrl;
            this.ctlDnDMap.SetMapAsync(mapImage);

            this.ctlControlPanel.EnableControlPanel();

            // Re-send everything since we've just re-created the Map and Fog. This will also force a Blackout of the new image.
            SendAll(true);
        }
    }
}
