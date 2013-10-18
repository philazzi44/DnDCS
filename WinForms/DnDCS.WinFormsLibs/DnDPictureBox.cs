using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS.WinFormsLibs
{
    public partial class DnDPictureBox : UserControl
    {
        public Image LoadedMap { get; protected set; }
        protected Size LoadedMapSize { get; set; }
        private int LogicalMapWidth { get { return (int)(LoadedMapSize.Width * AssignedZoomFactor); } }
        private int LogicalMapHeight { get { return (int)(LoadedMapSize.Height * AssignedZoomFactor); } }

        private int? gridSize;
        private Pen gridPen;

        private float assignedZoomFactor = 1.0f;
        protected float AssignedZoomFactor { get { return this.assignedZoomFactor; } private set { this.assignedZoomFactor = value; } }
        private float variableZoomFactor = 1.0f;
        protected bool IsZoomFactorInProgress { get; private set; }
        private Font zoomFactorFont;
        private static readonly string[] ZoomInstructionMessages = new[] {
                                                                            "Press Enter or Left Click to commit the zoom factor.",
                                                                            "Press Escape or Right Click to cancel."
                                                                         };

        protected Point ScrollPosition { get; set; }
        private Point lastScrollDragPosition;

        protected virtual int ZoomFactorTextYOffset { get { return 0; } }

        public event Action<Keys> TryToggleFullScreen;

        public bool AllowZoom { get; set; }

        public DnDPictureBox()
        {
            this.InitializeComponent();
        }

        protected virtual void Initialize()
        { 
        }

        public void Init()
        {
            this.BackColor = Color.Black;

            this.zoomFactorFont = new Font(SystemFonts.DefaultFont.FontFamily, 24.0f);

            this.Initialize();

            // Force focus the Picture Box in all cases, so it can properly respond to events.
            this.pbxMap.Focus();

            this.pbxMap.LostFocus += new EventHandler(pbxMap_LostFocus);
            this.pbxMap.Paint += new System.Windows.Forms.PaintEventHandler(this.pbxMap_Paint);
            this.pbxMap.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseClick);
            this.pbxMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseDown);
            this.pbxMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseMove);
            this.pbxMap.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbxMap_MouseUp);
            this.pbxMap.MouseWheel += new MouseEventHandler(pbxMap_MouseWheel);
            this.pbxMap.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.pbxMap_PreviewKeyDown);
        }

        private void pbxMap_LostFocus(object sender, EventArgs e)
        {
            this.pbxMap.Focus();
        }

        protected override void Dispose(bool disposing)
        {
            if (LoadedMap != null)
                LoadedMap.Dispose();
            if (gridPen != null)
                gridPen.Dispose();
            base.Dispose(disposing);
        }

        public virtual void SetMapAsync(Image newMap)
        {
            this.BeginInvoke(new Action(() =>
            {
                this.LoadedMap = newMap;
                this.LoadedMapSize = newMap.Size;
                this.RefreshMapPictureBox();
            }));
        }

        public void SetGridSize(bool showGrid, int gridSize)
        {
            this.gridSize = (showGrid) ? gridSize : new Nullable<int>();
            RefreshMapPictureBox();
        }

        public void SetGridColor(SimpleColor gridColor)
        {
            if (gridPen != null)
                gridPen.Dispose();
            gridPen = new Pen(Color.FromArgb(gridColor.A, gridColor.R, gridColor.G, gridColor.B));

            RefreshMapPictureBox();
        }

        #region Map Events

        public void RefreshMapPictureBox(bool immediateRefresh = false)
        {
            if (pbxMap.InvokeRequired)
            {
                pbxMap.BeginInvoke(new Action(() => { RefreshMapPictureBox(immediateRefresh); }));
                return;
            }

            if (immediateRefresh)
                pbxMap.Refresh();
            else
                pbxMap.Invalidate();
        }

        private void pbxMap_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.F11 || e.KeyCode == Keys.Escape)
            {
                // We pass in the key so the caller can know which of the two actions to take.
                if (TryToggleFullScreen != null)
                    TryToggleFullScreen(e.KeyCode);
                return;
            }

            if (e.Control)
            {
                if ((e.KeyCode == Keys.Add || e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.Down))
                    ZoomInOrOut((e.KeyCode == Keys.Add || e.KeyCode == Keys.Up), e.Shift);
                return;
            }

            if (IsZoomFactorInProgress)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                    CommitOrRollBackZoom((e.KeyCode == Keys.Enter));
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                    ZoomInOrOut((e.KeyCode == Keys.Up), e.Shift);
                return;
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        ScrollUpOrDown(true);
                        break;
                    case Keys.Down:
                        ScrollUpOrDown(false);
                        break;
                }

                switch (e.KeyCode)
                {
                    case Keys.Left:
                        ScrollLeftOrRight(true);
                        break;
                    case Keys.Right:
                        ScrollLeftOrRight(false);
                        break;
                }
            }
        }

        private void pbxMap_MouseWheel(object sender, MouseEventArgs e)
        {
            HandleMouseWheelEvent(e);
        }

        public void HandleMouseWheelEvent(MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                var isControl = Control.ModifierKeys.HasFlag(Keys.Control);
                var isShift = Control.ModifierKeys.HasFlag(Keys.Shift);

                if (isControl || IsZoomFactorInProgress)
                {
                    ZoomInOrOut((e.Delta > 0), isShift);
                    ((HandledMouseEventArgs)e).Handled = true;
                }
                else if (isShift)
                {
                    ScrollLeftOrRight((e.Delta > 0));
                    ((HandledMouseEventArgs)e).Handled = true;
                }
                else
                {
                    ScrollUpOrDown((e.Delta > 0));
                    ((HandledMouseEventArgs)e).Handled = true;
                }
            }

            RefreshMapPictureBox();
        }

        private void pbxMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (IsZoomFactorInProgress && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right))
            {
                CommitOrRollBackZoom((e.Button == MouseButtons.Left));
            }
        }

        private void pbxMap_MouseDown(object sender, MouseEventArgs e)
        {
            HandleMouseDown(e);
        }

        protected virtual void HandleMouseDown(MouseEventArgs e)
        {
            if (this.LoadedMap == null)
                return;
            if (IsZoomFactorInProgress)
                return;

            HandleMouseDown_DragMap(e);
        }

        protected void HandleMouseDown_DragMap(MouseEventArgs e)
        {
            lastScrollDragPosition = e.Location;
            this.pbxMap.Cursor = Cursors.Hand;
        }

        private void pbxMap_MouseMove(object sender, MouseEventArgs e)
        {
            HandleMouseMove(e);
        }

        protected virtual void HandleMouseMove(MouseEventArgs e)
        {
            if (this.LoadedMap == null)
                return;
            if (IsZoomFactorInProgress)
                return;

            HandleMouseMove_DragMap(e);
        }

        protected void HandleMouseMove_DragMap(MouseEventArgs e)
        {
            const int MoveThreshold = 3;

            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
                return;

            var newDragPosition = e.Location;

            // Scroll based on the amount of movement.
            var diffY = Math.Abs(newDragPosition.Y - lastScrollDragPosition.Y);
            if (diffY > MoveThreshold)
            {
                if (newDragPosition.Y < lastScrollDragPosition.Y)
                    ScrollUpOrDown(false, diffY);
                else if (newDragPosition.Y > lastScrollDragPosition.Y)
                    ScrollUpOrDown(true, diffY);
            }

            var diffX = Math.Abs(newDragPosition.X - lastScrollDragPosition.X);
            if (diffX > MoveThreshold)
            {
                if (newDragPosition.X < lastScrollDragPosition.X)
                    ScrollLeftOrRight(false, diffX);
                else if (newDragPosition.X > lastScrollDragPosition.X)
                    ScrollLeftOrRight(true, diffX);
            }

            lastScrollDragPosition = e.Location;
        }

        private void pbxMap_MouseUp(object sender, MouseEventArgs e)
        {
            HandleMouseUp(e);
        }

        protected virtual void HandleMouseUp(MouseEventArgs e)
        {
            if (IsZoomFactorInProgress)
                return;

            HandleMouseUp_Drag(e);
        }

        protected void HandleMouseUp_Drag(MouseEventArgs e)
        {
            this.pbxMap.Cursor = Cursors.Default;
        }

        #endregion Map Events

        #region Zoom Logic

        private void ZoomInOrOut(bool zoomIn, bool doubleFactor)
        {
            if (!AllowZoom)
                return;

            if (zoomIn)
                variableZoomFactor = (float)Math.Round(Math.Min(variableZoomFactor + ((doubleFactor) ? Constants.ZoomLargeStep : Constants.ZoomStep), ConfigValues.MaximumGridZoomFactor), 1);
            else
                variableZoomFactor = (float)Math.Round(Math.Max(variableZoomFactor - ((doubleFactor) ? Constants.ZoomLargeStep : Constants.ZoomStep), ConfigValues.MinimumGridZoomFactor), 1);

            IsZoomFactorInProgress = true;

            RefreshMapPictureBox();
        }

        private void CommitOrRollBackZoom(bool commit)
        {
            // Commit or rollback the zoom factor.
            IsZoomFactorInProgress = false;
            if (commit)
            {
                AssignedZoomFactor = variableZoomFactor;
                // This will validate that the current scroll values aren't too large for the new zoom factor.
                SetScroll(null, null);
            }
            else
            {
                variableZoomFactor = AssignedZoomFactor;
            }
            RefreshMapPictureBox();
        }

        #endregion Zoom Logic

        #region Scroll Logic

        private void ScrollLeftOrRight(bool isLeft, int? distance = null)
        {
            // Scroll left/right
            int newValue;
            if (isLeft)
                newValue = this.ScrollPosition.X - (distance ?? (int)(pbxMap.Width * Constants.ScrollWheelStepScrollPercent));
            else
                newValue = this.ScrollPosition.X + (distance ?? (int)(pbxMap.Width * Constants.ScrollWheelStepScrollPercent));
            SetScroll(newValue, null);
        }

        private void ScrollUpOrDown(bool isUp, int? distance = null)
        {
            // Scroll up/down
            int newValue;
            if (isUp)
                newValue = this.ScrollPosition.Y - (distance ?? (int)(pbxMap.Width * Constants.ScrollWheelStepScrollPercent));
            else
                newValue = this.ScrollPosition.Y + (distance ?? (int)(pbxMap.Width * Constants.ScrollWheelStepScrollPercent));
            SetScroll(null, newValue);
        }

        protected void SetScroll(int? desiredX, int? desiredY)
        {
            if (!desiredX.HasValue)
                desiredX = this.ScrollPosition.X;
            if (!desiredY.HasValue)
                desiredY = this.ScrollPosition.Y;

            // Do not allow negative scrolling in any way.
            if (desiredX.Value < 0)
                desiredX = 0;
            if (desiredY.Value < 0)
                desiredY = 0;

            // If the map we are showing is smaller than the width/height, then no X/Y scrolling is allowed at all.
            // Otherwise, enforce that the value is at most the amount that would be needed to show the full map given the current size of the visible area.
            if (this.LogicalMapWidth < this.pbxMap.Width)
                desiredX = 0;
            else
                desiredX = Math.Min(desiredX.Value, this.LogicalMapWidth - this.pbxMap.Width);

            if (this.LogicalMapHeight < this.pbxMap.Height)
                desiredY = 0;
            else
                desiredY = Math.Min(desiredY.Value, this.LogicalMapHeight - this.pbxMap.Height);

            this.ScrollPosition = new Point(desiredX.Value, desiredY.Value);
            RefreshMapPictureBox();
        }

        #endregion Scroll Logic

        #region Painting

        /// <summary> Repaint event occurs every time we request it, or when the user scrolls. </summary>
        private void pbxMap_Paint(object sender, PaintEventArgs e)
        {
            PaintAll(e.Graphics);
        }

        protected virtual void PaintAll(Graphics g)
        {
            throw new NotImplementedException("Must be overridden.");
        }

        protected void PaintMap(Graphics g)
        {
            if (this.LoadedMap != null)
            {
                g.TranslateTransform(-this.ScrollPosition.X, -this.ScrollPosition.Y);
                g.ScaleTransform(AssignedZoomFactor, AssignedZoomFactor);
                {
                    g.DrawImage(this.LoadedMap, Point.Empty);
                }
                g.ResetTransform();
            }
        }

        protected void PaintGrid(Graphics g)
        {
            // Because Paint events are sometimes scattered, we'll just draw the whole Grid rather than only part of it so there are no gaps.
            // Since our Grid Size is usually pretty big, this will never end up with more than maybe a hundred iterations.
            if (gridSize.HasValue)
            {
                g.TranslateTransform(-this.ScrollPosition.X, -this.ScrollPosition.Y);
                g.ScaleTransform(AssignedZoomFactor, AssignedZoomFactor);
                {
                    for (int x = 0; x < LoadedMapSize.Width; x += gridSize.Value)
                    {
                        g.DrawLine(gridPen, x, 0, x, LoadedMapSize.Height);
                    }
                    for (int y = 0; y < LoadedMapSize.Height; y += gridSize.Value)
                    {
                        g.DrawLine(gridPen, 0, y, LoadedMapSize.Width, y);
                    }
                }
                g.ResetTransform();
            }
        }

        protected void PaintZoomFactorText(Graphics g)
        {
            if (IsZoomFactorInProgress)
            {
                var font = this.zoomFactorFont ?? System.Drawing.SystemFonts.DefaultFont;

                var zoomMsgs = new[] { string.Format("Zoom: {0}x", variableZoomFactor) }.Concat(ZoomInstructionMessages).ToArray();
                for (var i = 0; i < zoomMsgs.Length; i++)
                {
                    // Draw each line one after the other, separating them by the height of the message, centered on the screen.
                    var msgSize = g.MeasureString(zoomMsgs[i], font);
                    var x = (this.pbxMap.Width / 2.0f) - (msgSize.Width / 2.0f);
                    var y = (this.pbxMap.Height / 2.0f) - (msgSize.Height / 2.0f) + msgSize.Height * i;
                    y += ZoomFactorTextYOffset;

                    g.DrawString(zoomMsgs[i], font, Brushes.Aqua, x, y);
                }
            }
        }

        #endregion Painting
    }
}
