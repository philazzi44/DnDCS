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
using DnDCS.Libs.SocketObjects;

namespace DnDCS.Server
{
    public partial class ServerControl : UserControl, IDisposable, IDnDCSControl
    {
        private Image map;

        private Bitmap oldFog;
        private Bitmap fog;
        private Bitmap newFog;
        private Button lastTool;
        private bool isBlackOutSet;
        private bool newFogIsClearing;
        private LinkedList<Point> newFogToolPoints;
        private MenuItem undoLastFogAction;

        private readonly Brush newFogClearBrush = Brushes.Red;

        // These two colors should be the same so the transparency works as expected.
        private readonly Brush fogClearBrush = Brushes.White;
        private readonly Color fogClearColor = Color.White;
        private readonly Brush fogBrush = Brushes.Black;
        private readonly Brush newFogBrush = Brushes.Gray;
        private readonly ImageAttributes fogAttributes = new ImageAttributes();

        private ServerSocketConnection connection;

        public ServerControl()
        {
            InitializeComponent();
        }

        private void ServerControl_Load(object sender, EventArgs e)
        {
            float[][] matrixItems = { new float[] {1, 0, 0, 0, 0},
                                      new float[] {0, 1, 0, 0, 0},
                                      new float[] {0, 0, 1, 0, 0},
                                      new float[] {0, 0, 0, 0.35f, 0}, 
                                      new float[] {0, 0, 0, 0, 1}
                                    };
            fogAttributes.SetColorMatrix(new ColorMatrix(matrixItems), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            fogAttributes.SetColorKey(fogClearColor, fogClearColor, ColorAdjustType.Bitmap);

            pbxMap.Paint += new PaintEventHandler(pbxMap_Paint);
            this.Disposed += new EventHandler(ServerControl_Disposed);

            ToggleTools(btnSelectTool);

            connection = new ServerSocketConnection(ConfigValues.DefaultServerPort);
            connection.OnClientConnected += new Action(connection_OnClientConnected);
        }

        private void connection_OnClientConnected()
        {
            connection.WriteMap(map);
            connection.WriteFog(fog);
        }

        private void ServerControl_Disposed(object sender, EventArgs e)
        {
            if (map != null)
                map.Dispose();
            if (fog != null)
                fog.Dispose();
            if (connection != null)
                connection.Stop();
        }
        
        public MainMenu GetMainMenu()
        {
            var menu = new MainMenu();
            var fileMenu = new MenuItem("File");
            fileMenu.MenuItems.AddRange(new MenuItem[]
            {
                new MenuItem("Load Image", OnLoadImage_Click, Shortcut.CtrlShiftO),
                new MenuItem("Load Image From Url", OnLoadImageUrl_Click, Shortcut.CtrlShiftK),
                new MenuItem("Save State", OnSaveState_Click, Shortcut.CtrlS),
                new MenuItem("Load State", OnLoadState_Click, Shortcut.CtrlO),
                new MenuItem("Exit", OnExit_Click),
            });

            var optionsMenu = new MenuItem("Options");
            undoLastFogAction = new MenuItem("Undo Last Fog Action", OnUndoLastFogAction_Click, Shortcut.CtrlZ);
            undoLastFogAction.Enabled = false;
            optionsMenu.MenuItems.Add(undoLastFogAction);
            optionsMenu.MenuItems.AddRange(new MenuItem[] 
            {
                undoLastFogAction,
                new MenuItem("Sync Connected Clients", OnSyncClients_Click)
            });

            menu.MenuItems.Add(fileMenu);
            menu.MenuItems.Add(optionsMenu);
            return menu;
        }

        private void OnLoadImage_Click(object sender, EventArgs e)
        {
            TryLoadImage();
        }

        private void OnLoadImageUrl_Click(object sender, EventArgs e)
        {
            TryLoadImageUrl();
        }

        private void OnSaveState_Click(object sender, EventArgs e)
        {
            // TODO: Commit the png and overlay information to a file
        }

        private void OnLoadState_Click(object sender, EventArgs e)
        {
            // TODO: Load a png and overlay information to a file
        }

        private void OnExit_Click(object sender, EventArgs e)
        {
            // TODO: Prompt for save and save if needed.
            connection.Stop();
            this.ParentForm.Close();
        }

        private void OnUndoLastFogAction_Click(object sender, EventArgs e)
        {
            if (oldFog != null)
            {
                // Flip the current fog with the backup.
                var flipFog = fog;
                fog = oldFog;
                oldFog = flipFog;
                pbxMap.Refresh();

                connection.WriteFog(fog);
            }
        }

        private void OnSyncClients_Click(object sender, EventArgs e)
        {
            connection.WriteMap(map);
            connection.WriteFog(fog);
        }

        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            DrawOnGraphics(e.Graphics);
        }

        private void btnSelectTool_Click(object sender, EventArgs e)
        {
            ToggleTools(btnSelectTool);
        }

        private void btnFogTool_Click(object sender, EventArgs e)
        {
            ToggleTools(btnFogTool);
        }

        private void btnToggleBlackout_Click(object sender, EventArgs e)
        {
            if (isBlackOutSet)
            {
                // Send message to client to stop doing full blackouts, and obey the fog of war map being sent over
                isBlackOutSet = false;
                btnToggleBlackout.Text = "Blackout";
            }
            else
            {
                // Send message to client to do a full blackout, ignoring any fog of war map that may exist
                isBlackOutSet = true;
                btnToggleBlackout.Text = "Stop Blackout";
            }

            // Map and Fog Updates would have been sent to the client in real-time but masked on their end, so we can simply inform them of the change.
            connection.WriteBlackout(isBlackOutSet);
        }
        
        private void btnFogAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "This will fog the entire map. Are you sure?", "Fog Entire Map?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                // Keep a copy of the fog before we blow it away, in case the user messes up.
                oldFog = (Bitmap)fog.Clone();
                using (var g = Graphics.FromImage(fog))
                {
                    g.FillRectangle(fogBrush, 0, 0, fog.Width, fog.Height);
                    pbxMap.Refresh();
                }
                connection.WriteFog(fog);
            }
        }

        private void ToggleTools(Button enabledTool)
        {
            // Unset the previous tool
            if (lastTool == btnFogTool)
                UnsetFogTool();

            btnSelectTool.Enabled = (btnSelectTool != enabledTool);
            btnFogTool.Enabled = (btnFogTool != enabledTool);

            // Set the new tool
            if (btnFogTool == enabledTool)
                SetFogTool();

            lastTool = enabledTool;
        }

        private void SetFogTool()
        {
            pbxMap.Cursor = Cursors.Cross;
            pbxMap.MouseDown += new MouseEventHandler(pbxMap_MouseDown);
            pbxMap.MouseUp += new MouseEventHandler(pbxMap_MouseUp);
        }

        private void pbxMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left && e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            pbxMap.MouseMove += new MouseEventHandler(pbxMap_MouseMove);

            // Keep track of the old fog to support an Undo functionality. Make a new image for the New Fog to track the updates being made.
            oldFog = (Bitmap)fog.Clone();
            newFog = new Bitmap(fog.Width, fog.Height);
            // TODO: Do we need to forcibly set the newfog as all transparent?

            // Start tracking all the points the mouse moves.
            newFogToolPoints = new LinkedList<Point>();
            newFogIsClearing = (e.Button == System.Windows.Forms.MouseButtons.Left);
            newFogToolPoints.AddLast(e.Location);
        }

        private void pbxMap_MouseMove(object sender, MouseEventArgs e)
        {
            // Update the New Fog image with the newly added point, so it can be drawn on the screen in real time.
            newFogToolPoints.AddLast(e.Location);
            UpdateFogImage(newFog, newFogIsClearing ? newFogClearBrush : newFogBrush, newFogToolPoints.ToArray());

            pbxMap.Refresh();
        }
        
        private void pbxMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left && e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            pbxMap.MouseMove -= new MouseEventHandler(pbxMap_MouseMove);

            // Commit the last point onto the main Fog Image then clear out the 'New Fog' temporary image altogether.
            newFogToolPoints.AddLast(e.Location);
            UpdateFogImage(fog, (newFogIsClearing) ? fogClearBrush : fogBrush, newFogToolPoints.ToArray());

            newFog.Dispose();
            newFog = null;

            // We should still have an OldFog backup image, so we'll allow undoing to it.
            undoLastFogAction.Enabled = (oldFog != null);

            pbxMap.Refresh();

            connection.WriteFogUpdate(newFogToolPoints.ToArray(), newFogIsClearing);
            newFogToolPoints = null;
        }

        private void UnsetFogTool()
        {
            pbxMap.Cursor = Cursors.Arrow;
            pbxMap.MouseDown -= new MouseEventHandler(pbxMap_MouseDown);
            pbxMap.MouseUp -= new MouseEventHandler(pbxMap_MouseUp);
        }

        private void TryLoadImage()
        {
            // Launch a popup to get a Url for an image
            var imageFile = GetLoadImageFile();
            if (string.IsNullOrWhiteSpace(imageFile))
                return;

            SetMapImage(Image.FromFile(imageFile));
        }

        private string GetLoadImageFile()
        {
            // Launch a popup to get a png
            using (var openFile = new OpenFileDialog())
            {
                openFile.Title = "Select a .PNG image.";
                openFile.CheckFileExists = true;
                openFile.CheckPathExists = true;
                openFile.Filter = "Image Files (*.png)|*.png";

                var result = openFile.ShowDialog(this);
                if (result == DialogResult.OK)
                    return openFile.FileName;
            }
            return null;
        }

        private void TryLoadImageUrl()
        {
            // Launch a popup to get a Url for an image
            var image = GetLoadImageUrl();
            if (image == null)
                return;

            SetMapImage(image);
        }

        private Image GetLoadImageUrl()
        {
            using (var loadImage = new GetUrlDialog())
            {
                var result = loadImage.ShowDialog(this);
                if (result == DialogResult.OK)
                    return loadImage.LoadedImage;
            }
            return null;
        }

        private void SetMapImage(Image mapImage)
        {
            if (mapImage == null)
                return;
            map = mapImage;

            CreateFogImage();

            pbxMap.Image = map;
            pbxMap.Refresh();

            connection.WriteMap(map);
        }

        private void CreateFogImage()
        {
            fog = new Bitmap(map.Width, map.Height);
            using (var g = Graphics.FromImage(fog))
            {
                g.FillRectangle(fogBrush, 0, 0, fog.Width, fog.Height);
            }

            connection.WriteFog(fog);
        }

        private void UpdateFogImage(Bitmap fogImage, Brush brush, Point[] fogToolPoints)
        {
            using (var g = Graphics.FromImage(fogImage))
            {
                g.FillPolygon(brush, fogToolPoints);
            }
        }

        private void DrawOnGraphics(Graphics g)
        {
            if (map == null || fog == null)
                return;

            g.DrawImage(fog, new Rectangle(Point.Empty, pbxMap.Size), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);

            if (newFog != null)
                g.DrawImage(newFog, new Rectangle(Point.Empty, pbxMap.Size), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
        }
    }
}
