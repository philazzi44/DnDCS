using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
        private double zoomFactor;
        private Size apparentSize;
        private int drawWidth;
        private int drawHeight;
        private Point centerPoint;

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

        public double ZoomFactor
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
                    zoomFactor = 0.05;
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

        }

        public void SetGridColor(SimpleColor gridColor)
        {

        }

        public void ZoomIn()
        {
            ZoomImage(true);
        }

        public void ZoomOut()
        {
            ZoomImage(false);
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
            //if (IsBlackoutOn)
            //{
            //    DrawBlackout(e.Graphics);
            //}
            //else
            //{
                DrawImage(e.Graphics);
            //}
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
                new SimplePoint(map.Width, map.Height)
            }, false);

        }

        private unsafe void ApplyFog(SimplePoint[] points, bool isClearing)
        {
            if (map == null)
            {
                return;
            }

            var boundingBoxBuffered = GetBoundingBox(points);
            var boundingBox = GetBoundingBox(points, 0);

            var bmd = map.LockBits(boundingBoxBuffered, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var pixelSize = 4;
            for (var y = 0; y < bmd.Height; y++)
            {
                var row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                for (var x = 0; x < bmd.Width; x++)
                {
                    row[x * pixelSize + 3] = (byte)(isClearing ? 255 : 96);
                }
            }
            map.UnlockBits(bmd);
            Invalidate();
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

            if (origin.X < 0)
            {
                origin.X = 0;
            }
            if (origin.Y < 0)
            {
                origin.Y = 0;
            }
            if (origin.X > map.Width - (ClientSize.Width / zoomFactor))
            {
                origin.X = (int)(map.Width - (ClientSize.Width / zoomFactor));
            }
            if (origin.Y > map.Height - (ClientSize.Height / zoomFactor))
            {
                origin.Y = (int)(map.Height - (ClientSize.Height / zoomFactor));
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

        private void DrawImage(Graphics g)
        {
            if (map == null)
            {
                return;
            }

            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            sourceRect = new Rectangle(origin.X, origin.Y, drawWidth, drawHeight);
            g.DrawImage(map, destRect, sourceRect, GraphicsUnit.Pixel);
        }

        private void ZoomImage(bool zoomIn = true)
        {
            centerPoint.X = origin.X + sourceRect.Width / 2;
            centerPoint.Y = origin.Y + sourceRect.Height / 2;
            if (zoomIn)
            {
                ZoomFactor = Math.Round(zoomFactor * 1.1, 2);
            }
            else
            {
                ZoomFactor = Math.Round(zoomFactor * 0.9, 2);
            }

            origin.X = (int)(centerPoint.X - ClientSize.Width / zoomFactor / 2);
            origin.Y = (int)(centerPoint.Y - ClientSize.Height / zoomFactor / 2);

            CheckBounds();
        }
    }
}
