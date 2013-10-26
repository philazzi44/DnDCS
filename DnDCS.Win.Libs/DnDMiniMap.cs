using System;
using System.Drawing;
using System.Windows.Forms;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS.Win.Libs
{
    public partial class DnDMiniMap : UserControl
    {
        public DnDServerPictureBox DnDMapControl { get; set; }

        private bool isDraggingMap;
        private Size loadedMapSize;
        private Image miniMap;
        private Size miniMapMarkerSize;

        private Point miniMapCenterMap;
        private Point MiniMapCenterMap
        {
            get { return miniMapCenterMap; }
            set
            {
                miniMapCenterMap = value;
                this.Refresh();
            }
        }

        /// <summary> Event raised when the user a new Center Map location is set via the minimap. </summary>
        public event Action<SimplePoint> OnNewCenterMap;

        private int penIndex = 0;
        private static readonly Pen[] availablePens = new Pen[] {
                                                                    Pens.Black,
                                                                    Pens.White,
                                                                    Pens.Aqua,
                                                                    Pens.Yellow,
                                                                    Pens.Lime,
                                                                    Pens.Fuchsia,
                                                                };

        public DnDMiniMap()
        {
            InitializeComponent();
        }

        public void Init()
        {
            DnDMapControl.OnNewMapSet += new Action<Image>(DnDMapControl_OnNewMapSet);
            DnDMapControl.OnScrollStep += new Action<Point>(DnDMapControl_OnScrollStep);
            DnDMapControl.SizeChanged += new EventHandler(DnDMapControl_SizeChanged);

            // These are styles that apply to PictureBoxes by default, but since we're not using one, we need to set them explicitly.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void DnDMapControl_OnNewMapSet(Image loadedMap)
        {
            // MiniMap images will always be the size of the Mini Map control.
            if (miniMap == null)
            {
                miniMap = new Bitmap(this.Width, this.Height);
            }

            // Draw the Map into the MiniMap image, scaled down to fit.
            using (var g = Graphics.FromImage(miniMap))
            {
                g.Clear(Color.Black);
                g.DrawImage(loadedMap, 0, 0, miniMap.Width, miniMap.Height);
            }

            loadedMapSize = loadedMap.Size;
            
            // Defaults to (0, 0) centered in the mini map area.
            SetMiniMapMarkerSize();
            MiniMapCenterMap = new Point(miniMapMarkerSize.Width / 2, miniMapMarkerSize.Height / 2);

            TryRaiseOnNewCenterMap();
        }
        
        private void DnDMapControl_OnScrollStep(Point loadedMapTopLeftScrollCoordinates)
        {
            MiniMapCenterMap = ToCenterMapLocation(loadedMapTopLeftScrollCoordinates);
        }

        private void DnDMapControl_SizeChanged(object sender, EventArgs e)
        {
            if (this.miniMap == null)
                return;

            // We keep the same center spot, but the bounding box drawn just ends up changing (larger/smaller) to reflect the new proportions.
            SetMiniMapMarkerSize();
            this.Invalidate();
        }

        private void SetMiniMapMarkerSize()
        {
            // The size of the Mini Map Marker will be based on how much of the actual map the user can see. If the map is smaller than the 
            // visible area, then our marker will be the max of that axis.
            var mapActualSize = loadedMapSize;
            var mapVisibleSize = DnDMapControl.VisibleSize;
            miniMapMarkerSize = new Size((int)Math.Min((double)this.Width - 1, (double)this.Width * ((double)mapVisibleSize.Width / (double)mapActualSize.Width)),
                                         (int)Math.Min((double)this.Height - 1, (double)this.Height * ((double)mapVisibleSize.Height / (double)mapActualSize.Height)));
        }

        private void DnDMiniMap_Paint(object sender, PaintEventArgs e)
        {
            if (this.miniMap == null)
                return;

            var g = e.Graphics;

            // The Mini Map image is the size of this control, so we can simply draw it at 0, 0.
            g.DrawImage(this.miniMap, 0, 0);

            var x = Math.Max(0, Math.Min(miniMapCenterMap.X - (miniMapMarkerSize.Width / 2), this.Width - miniMapMarkerSize.Width - 1));
            var y = Math.Max(0, Math.Min(miniMapCenterMap.Y - (miniMapMarkerSize.Height / 2), this.Height - miniMapMarkerSize.Height - 1));
            g.DrawRectangle(availablePens[penIndex], x, y, miniMapMarkerSize.Width, miniMapMarkerSize.Height);
        }

        private void DnDMiniMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.miniMap == null)
                return;

            if (e.Button == MouseButtons.Left)
            {
                isDraggingMap = true;

                MiniMapCenterMap = e.Location;
                TryRaiseOnNewCenterMap();
            }
            else if (e.Button == MouseButtons.Right)
            {
                penIndex = (penIndex + 1 == availablePens.Length) ? 0 : penIndex + 1;
                this.Invalidate();
            }
        }

        private void DnDMiniMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.miniMap == null)
                return;

            if (e.Button != MouseButtons.Left)
                return;
            if (!isDraggingMap)
                return;

            MiniMapCenterMap = e.Location;
            TryRaiseOnNewCenterMap();
        }

        private void DnDMiniMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.miniMap == null)
                return;

            if (e.Button != MouseButtons.Left)
                return;
            if (!isDraggingMap)
                return;

            isDraggingMap = false;
            MiniMapCenterMap = e.Location;
            TryRaiseOnNewCenterMap();
        }

        private Point ToCenterMapLocation(Point loadedMapTopLeftPoint)
        {
            var loadedMapX = loadedMapTopLeftPoint.X;
            var loadedMapY = loadedMapTopLeftPoint.Y;

            // We'll shrink the X/Y based on how much we shrink the Map to fit into the Mini Map.
            var miniMapX = ((double)loadedMapX / (double)loadedMapSize.Width) * this.miniMap.Width;
            var miniMapY = ((double)loadedMapY / (double)loadedMapSize.Height) * this.miniMap.Height;

            // If this is the top/left of the Mini Map, then we need to offset it by half of the Marker Size (keeping care to not
            // go beyond the boundaries).
            miniMapX += Math.Min((miniMapMarkerSize.Width / 2.0d), this.Width - (miniMapMarkerSize.Width / 2.0d));
            miniMapY += Math.Min((miniMapMarkerSize.Height / 2.0d), this.Height - (miniMapMarkerSize.Height / 2.0d));

            return new Point((int)miniMapX, (int)miniMapY);
        }

        private SimplePoint ToLoadedMapLocation(Point miniMapCenterPoint)
        {
            var miniMapX = miniMapCenterPoint.X;
            var miniMapY = miniMapCenterPoint.Y;

            // We'll bloat the X/Y based on how much we shrunk the Map to fit into the Mini Map.
            var loadedMapX = ((double)miniMapX / (double)miniMap.Width) * this.loadedMapSize.Width;
            var loadedMapY = ((double)miniMapY / (double)miniMap.Height) * this.loadedMapSize.Height;
            return new SimplePoint((int)loadedMapX, (int)loadedMapY);
        }

        private void TryRaiseOnNewCenterMap()
        {
            if (OnNewCenterMap != null)
                OnNewCenterMap(ToLoadedMapLocation(MiniMapCenterMap));
        }
    }
}
