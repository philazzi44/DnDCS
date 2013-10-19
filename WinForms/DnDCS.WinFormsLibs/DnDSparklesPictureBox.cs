using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipperLib;
using DnDCS.Libs.SimpleObjects;
using DnDCS.WinFormsLibs.Assets;

namespace DnDCS.WinFormsLibs
{
    public class DnDSparklesPictureBox : UserControl
    {
        private Bitmap map;
        private Bitmap fog;
        private Point startPoint;
        private Point origin = new Point(0, 0);
        private Graphics gfx;
        private Rectangle sourceRect;
        private Rectangle destRect;
        private float zoomFactor;
        private Size apparentSize;
        private int drawWidth;
        private int drawHeight;
        private Point centerPoint;
        private static object lockObject = new object();
        private bool showGrid;
        private int? gridSize;
        private Pen gridPen;
        private int keyAcceleration = 5;

        public event Action<Keys> TryToggleFullScreen;

        public bool IsBlackoutOn { get; set; }

        public Size ApparentImageSize
        {
            get { return apparentSize; }
        }

        public Image Map
        {
            get { return map; }
            set
            {
                lock (lockObject)
                {
                    if (map != null)
                    {
                        map.Dispose();
                        origin = new Point(0, 0);
                        apparentSize = new Size(0, 0);
                        zoomFactor = 1;
                        GC.Collect();
                    }

                    if (value == null)
                    {
                        map = null;
                        Invalidate();
                        return;
                    }

                    var r = new Rectangle(0, 0, value.Width, value.Height);
                    map = new Bitmap(value);
                    map = map.Clone(r, PixelFormat.Format32bppArgb);
                }

                ApplyFullFog();
                Invalidate();
            }
        }

        public Point Origin
        {
            get { return origin; }
            set
            {
                origin = value;
                Invalidate();
            }
        }

        public float ZoomFactor
        {
            get { return zoomFactor; }
            set
            {
                zoomFactor = value;
                if (zoomFactor > 15)
                {
                    zoomFactor = 15;
                }
                if (zoomFactor < 0.05)
                {
                    zoomFactor = 0.05f;
                }

                if (map != null)
                {
                    apparentSize.Height = (int)(map.Height * zoomFactor);
                    apparentSize.Width = (int)(map.Width * zoomFactor);
                    ComputeDrawingArea();
                    CheckBounds();
                }

                Invalidate();
            }
        }

        public DnDSparklesPictureBox()
        {
            KeyDown += HandleKeyDown;
            KeyUp += HandleKeyUp;
            MouseDown += HandleMouseDown;
            MouseMove += HandleMouseMove;
            MouseUp += HandleMouseUp;
            MouseWheel += HandleMouseWheel;

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void Init()
        {
            this.Height = this.Parent.Height;
            this.Width = this.Parent.Width;
            this.BackColor = Color.Black;
            ZoomFactor = 1;
        }

        public void RefreshMapPictureBox(bool immediateRefresh = false)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { RefreshMapPictureBox(immediateRefresh); }));
                return;
            }

            Invalidate();
        }

        public void CenterMap(SimplePoint centerMap)
        {

        }

        public void SetFogAsync(Image newFog)
        {

        }

        public void SetMapAsync(Image newMap)
        {

        }

        public void SetFogUpdateAsync(FogUpdate fogUpdate)
        {
            ApplyFog(fogUpdate.Points, fogUpdate.IsClearing);
        }

        public void SetGridSize(bool showGrid, int gridSize)
        {
            this.showGrid = showGrid;
            if (showGrid)
                this.gridSize = gridSize;
            else
                this.gridSize = null;
            Invalidate();
        }

        public void SetGridColor(SimpleColor gridColor)
        {
            if (gridPen != null)
            {
                gridPen.Dispose();
            }
            gridPen = new Pen(Color.FromArgb(gridColor.A, gridColor.R, gridColor.G, gridColor.B));
            Invalidate();
        }

        public void ZoomIn()
        {
            ZoomImage(true);
        }

        public void ZoomOut()
        {
            ZoomImage(false);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Oemplus && e.Control)
            {
                ZoomIn();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.OemMinus && e.Control)
            {
                ZoomOut();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                var deltaX = 0;
                var deltaY = 0;

                if (e.KeyCode == Keys.Up)
                    deltaY = -1;
                if (e.KeyCode == Keys.Down)
                    deltaY = 1;
                if (e.KeyCode == Keys.Left)
                    deltaX = -1;
                if (e.KeyCode == Keys.Right)
                    deltaX = 1;

                deltaY = deltaY * keyAcceleration;
                deltaX = deltaX * keyAcceleration;

                origin.X = (int)(origin.X + (deltaX / zoomFactor));
                origin.Y = (int)(origin.Y + (deltaY / zoomFactor));
                CheckBounds();
                Invalidate();
                keyAcceleration = Math.Min(100, keyAcceleration + 1);
            }
        }

        protected void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11 || e.KeyCode == Keys.Escape)
            {
                if (TryToggleFullScreen != null)
                {
                    TryToggleFullScreen(e.KeyCode);
                }
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                keyAcceleration = 5;
            }

            if (e.KeyCode == Keys.Home)
            {
                origin.X = map.Width / 2 - Width / 2;
                origin.Y = map.Height / 2 - Height / 2;
                ZoomFactor = 1;
            }
        }

        protected void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (map == null)
            {
                return;
            }

            startPoint = new Point(e.X, e.Y);
            Focus();

            if (e.Button == MouseButtons.Right)
            {
                Cursor = Cursors.SizeAll;
            }
        }

        protected void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (map == null)
            {
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                var deltaX = startPoint.X - e.X;
                var deltaY = startPoint.Y - e.Y;
                origin.X = (int)(origin.X + (deltaX / zoomFactor));
                origin.Y = (int)(origin.Y + (deltaY / zoomFactor));

                CheckBounds();
                startPoint.X = e.X;
                startPoint.Y = e.Y;
                Invalidate();
            }
        }

        protected void HandleMouseUp(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Default;
        }

        protected void HandleMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                ZoomImage(true);
            }
            else if (e.Delta < 0)
            {
                ZoomImage(false);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (IsBlackoutOn)
            {
                DrawBlackout(e.Graphics);
            }
            else
            {
                DrawImage(e.Graphics);
                DrawGrid(e.Graphics);
            }
            base.OnPaint(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            destRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
            ComputeDrawingArea();
            base.OnSizeChanged(e);
        }

        private unsafe void ApplyFullFog()
        {
            if (map == null)
            {
                return;
            }

            ApplyFog(new[] {
                new SimplePoint(0, 0),
                new SimplePoint(map.Width, 0),
                new SimplePoint(map.Width, map.Height),
                new SimplePoint(0, map.Height),
            }, false);

        }

        private unsafe void ApplyFog(SimplePoint[] points, bool isClearing)
        {
            if (map == null)
            {
                return;
            }

            var polygon = new List<IntPoint>(points.Select(x => new IntPoint(x.X, x.Y)));
            var polygons = new List<List<IntPoint>>() { polygon };
            polygons = Clipper.OffsetPolygons(polygons, 48, JoinType.jtRound);

            var offsetPoints = polygons[0].Select(x => new SimplePoint((int)x.X, (int)x.Y)).ToArray();

            var boundingBoxBuffered = GetBoundingBox(offsetPoints, 4);
            var boundingBox = GetBoundingBox(points, 0);

            lock (lockObject)
            {
                var bmd = map.LockBits(boundingBoxBuffered, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var pixelSize = 4;
                Parallel.For(0, bmd.Height, (y) =>
                {
                    var row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                    for (var x = 0; x < bmd.Width; x++)
                    {
                        var offsetX = x + boundingBoxBuffered.X;
                        var offsetY = y + boundingBoxBuffered.Y;

                        if (isClearing && row[x * pixelSize + 3] == 255)
                        {
                            continue;
                        }

                        if (IsPointInPolygon(points, offsetX, offsetY))
                        {
                            row[x * pixelSize + 3] = (byte)(isClearing ? 255 : 0);
                        }
                        else if (IsPointInPolygon(offsetPoints, offsetX, offsetY))
                        {
                            var testPoint = new SimplePoint(offsetX, offsetY);
                            var dist = LineToPointDistance2D(points[0], points[1], testPoint);
                            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                            {
                                var newDist = LineToPointDistance2D(points[j], points[i], testPoint);
                                if (newDist < dist)
                                    dist = newDist;
                            }

                            var alpha = (255 - 5.5 * dist);
                            alpha = Math.Max(Math.Floor(alpha), 0);
                            if (isClearing)
                                alpha = Math.Min(alpha + row[x * pixelSize + 3], 255);
                            else
                                alpha = Math.Max(row[x * pixelSize + 3] - alpha, 0);
                            row[x * pixelSize + 3] = (byte)(alpha);
                        }
                    }
                });

                map.UnlockBits(bmd);
            }
            Invalidate();
        }

        public bool IsPointInPolygon(SimplePoint[] polygon, float testx, float testy)
        {
            int nvert = polygon.Length;
            var vertx = polygon.Select(x => (float)(x.X)).ToArray();
            var verty = polygon.Select(x => (float)(x.Y)).ToArray();

            int i, j = 0;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verty[i] > testy) != (verty[j] > testy)) &&
                 (testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
                    c = !c;
            }
            return c;
        }

        private Rectangle GetBoundingBox(SimplePoint[] points, int buffer = 8)
        {
            if (points.Length == 0)
            {
                return new Rectangle(0, 0, 0, 0);
            }

            var left = points[0].X;
            var right = points[0].X;
            var top = points[0].Y;
            var bottom = points[0].Y;
            foreach (var point in points)
            {
                if (point.X < left)
                    left = point.X;
                if (point.X > right)
                    right = point.X;
                if (point.Y < top)
                    top = point.Y;
                if (point.Y > bottom)
                    bottom = point.Y;
            }

            var rect = new Rectangle(left, top, right - left, bottom - top);
            rect.X = Math.Max(0, rect.X - buffer);
            rect.Y = Math.Max(0, rect.Y - buffer);
            rect.Width = Math.Min(map.Width - rect.X, rect.Width + buffer);
            rect.Height = Math.Min(map.Height - rect.Y, rect.Height + buffer);
            return rect;
        }

        private void CheckBounds()
        {
            if (map == null)
            {
                return;
            }

            if (origin.X > map.Width - (ClientSize.Width / zoomFactor))
            {
                origin.X = (int)(map.Width - (ClientSize.Width / zoomFactor));
            }
            if (origin.Y > map.Height - (ClientSize.Height / zoomFactor))
            {
                origin.Y = (int)(map.Height - (ClientSize.Height / zoomFactor));
            }

            if (origin.X < 0)
            {
                origin.X = 0;
            }
            if (origin.Y < 0)
            {
                origin.Y = 0;
            }
        }

        private void ComputeDrawingArea()
        {
            drawHeight = (int)(Height / zoomFactor);
            drawWidth = (int)(Width / zoomFactor);
        }

        private void DrawBlackout(Graphics g)
        {
            g.Clear(Color.Black);
            g.DrawImage(AssetsLoader.BlackoutImage, Width / 2.0f - AssetsLoader.BlackoutImage.Width / 2.0f, Height / 2.0f - AssetsLoader.BlackoutImage.Height / 2.0f);
        }

        private void DrawGrid(Graphics g)
        {
            if (gridSize.HasValue && gridPen != null)
            {
                g.TranslateTransform(-1 * this.origin.X, -1 * this.origin.Y);
                g.ScaleTransform(zoomFactor, zoomFactor);
                for (int x = 0; x < map.Width; x += gridSize.Value)
                {
                    g.DrawLine(gridPen, x, 0, x, map.Height);
                }
                for (int y = 0; y < map.Height; y += gridSize.Value)
                {
                    g.DrawLine(gridPen, 0, y, map.Width, y);
                }
                g.ResetTransform();
            }
        }

        private void DrawImage(Graphics g)
        {
            if (map == null)
            {
                return;
            }

            lock (lockObject)
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;

                sourceRect = new Rectangle(origin.X, origin.Y, drawWidth, drawHeight);
                g.DrawImage(map, destRect, sourceRect, GraphicsUnit.Pixel);
            }
        }

        private void ZoomImage(bool zoomIn = true)
        {
            centerPoint.X = origin.X + sourceRect.Width / 2;
            centerPoint.Y = origin.Y + sourceRect.Height / 2;
            if (zoomIn)
            {
                ZoomFactor = (float)Math.Round(zoomFactor * 1.1, 2);
            }
            else
            {
                ZoomFactor = (float)Math.Round(zoomFactor * 0.9, 2);
            }

            origin.X = (int)(centerPoint.X - ClientSize.Width / zoomFactor / 2);
            origin.Y = (int)(centerPoint.Y - ClientSize.Height / zoomFactor / 2);

            CheckBounds();
        }

        #region Maths
        //Compute the dot product AB . AC
        private double DotProduct(double[] pointA, double[] pointB, double[] pointC)
        {
            double[] AB = new double[2];
            double[] BC = new double[2];
            AB[0] = pointB[0] - pointA[0];
            AB[1] = pointB[1] - pointA[1];
            BC[0] = pointC[0] - pointB[0];
            BC[1] = pointC[1] - pointB[1];
            double dot = AB[0] * BC[0] + AB[1] * BC[1];

            return dot;
        }

        //Compute the cross product AB x AC
        private double CrossProduct(double[] pointA, double[] pointB, double[] pointC)
        {
            double[] AB = new double[2];
            double[] AC = new double[2];
            AB[0] = pointB[0] - pointA[0];
            AB[1] = pointB[1] - pointA[1];
            AC[0] = pointC[0] - pointA[0];
            AC[1] = pointC[1] - pointA[1];
            double cross = AB[0] * AC[1] - AB[1] * AC[0];

            return cross;
        }

        //Compute the distance from A to B
        private double Distance(double[] pointA, double[] pointB)
        {
            double d1 = pointA[0] - pointB[0];
            double d2 = pointA[1] - pointB[1];

            return Math.Sqrt(d1 * d1 + d2 * d2);
        }

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        private double LineToPointDistance2D(SimplePoint linePoint1, SimplePoint linePoint2, SimplePoint pointTest, bool isSegment = true)
        {
            if (linePoint1.X == linePoint1.X && linePoint1.Y == linePoint2.Y)
                return 255;

            var pointA = new double[] { linePoint1.X, linePoint1.Y };
            var pointB = new double[] { linePoint2.X, linePoint2.Y };
            var pointC = new double[] { pointTest.X, pointTest.Y };
            double dist = CrossProduct(pointA, pointB, pointC) / Distance(pointA, pointB);
            if (isSegment)
            {
                double dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return Distance(pointB, pointC);

                double dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return Distance(pointA, pointC);
            }

            return Math.Abs(dist);
        } 
        #endregion
    }
}
