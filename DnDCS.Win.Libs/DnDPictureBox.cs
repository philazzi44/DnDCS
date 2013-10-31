﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using DnDCS.Libs;
using DnDCS.Libs.SimpleObjects;
using System.ComponentModel;

namespace DnDCS.Win.Libs
{
    public partial class DnDPictureBox : UserControl
    {
        // Init Values
        private bool isInitialized;

        // Map Values
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image LoadedMap { get; protected set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected Size LoadedMapSize { get; set; }
        private int LogicalMapWidth { get { return (int)(LoadedMapSize.Width * AssignedZoomFactor); } }
        private int LogicalMapHeight { get { return (int)(LoadedMapSize.Height * AssignedZoomFactor); } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event Action<Image> OnNewMapSet;
        /// <summary> Gets the size that is visible to the user. </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size VisibleSize
        {
            get { return this.Size; }
        }

        // Grid Values
        private int? gridSize;
        private Pen gridPen = new Pen(Color.Aqua);
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen GridPen
        {
            get { return this.gridPen; }
            set
            {
                if (this.gridPen != null)
                    this.gridPen.Dispose();
                this.gridPen = value;
            }
        }

        // Fog Values
        private byte fogAlpha = 255;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte FogAlpha
        {
            get { return this.fogAlpha; }
            set
            {
                this.fogAlpha = value;
                if (isInitialized)
                    this.FogAttributes = CreateFogAttributes();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseFogAlphaEffect { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Bitmap Fog { get; protected set; }

        // Zoom Values
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AllowZoom { get; set; }
        private float assignedZoomFactor = 1.0f;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected float AssignedZoomFactor { get { return this.assignedZoomFactor; } private set { this.assignedZoomFactor = value; } }
        private float variableZoomFactor = 1.0f;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool IsZoomFactorInProgress { get; private set; }
        private Font zoomFactorFont;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual int ZoomFactorTextYOffset { get { return 0; } }
        private static readonly string[] ZoomInstructionMessages = new[] {
                                                                            "Press Enter or Left Click to commit the zoom factor.",
                                                                            "Press Escape or Right Click to cancel."
                                                                         };


        // Scroll Values
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected Point ScrollPosition { get; set; }
        private Point lastScrollDragPosition;
        private double keyboardScrollAccel = 1.0d;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool UseHighQuality { get; set; }
        private readonly Timer scrollHighQualityTimer = new Timer();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event Action<Point> OnScrollStep;
        private Cursor dragMapOldCursor = Cursors.Default;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool SuppressScroll { get; set; }

        // Flipped View Values
        private bool isFlippedView;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFlippedView
        {
            get { return isFlippedView; }
            set
            {
                if (value == isFlippedView)
                    return;

                isFlippedView = value;
                if (value)
                {
                    // Going from Unflipped to Flipped, so we can simply reverse the coordinates.
                    SetScroll(this.LoadedMapSize.Width - ScrollPosition.X, this.LoadedMapSize.Height - ScrollPosition.Y);
                }
                else
                {
                    // Going from Flipped to Unflipped, which means we actually want to reverse the bottom-right of our ScrollPosition, which will become our new Top-Left coordinate.
                    SetScroll(this.LoadedMapSize.Width - (ScrollPosition.X + this.Width), this.LoadedMapSize.Height - (ScrollPosition.Y + this.Height));
                }
            }
        }

        // Paint Values
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected ImageAttributes FogAttributes { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool UseNewPaintLogic = true;

        // Callbacks
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event Action<Keys> TryToggleFullScreen;

        #region Init and Cleanup

        public DnDPictureBox()
        {
            this.InitializeComponent();
        }

        public void Init()
        {
            this.BackColor = Color.Black;
            this.UseHighQuality = true;

            this.zoomFactorFont = new Font(SystemFonts.DefaultFont.FontFamily, 24.0f);

            this.FogAttributes = CreateFogAttributes();

            this.Initialize();

            // Force focus the Picture Box in all cases, so it can properly respond to events.
            this.Focus();

            this.LostFocus += new EventHandler(HandleLostFocus);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.HandlePaintEvent);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleMouseDownEvent);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseMoveEvent);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.HandleMouseUpEvent);
            this.MouseWheel += new MouseEventHandler(HandleMouseWheelEvent);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.HandlePreviewKeyDownEevent);
            this.KeyUp += new KeyEventHandler(HandleKeyUpEvent);

            // These are styles that apply to PictureBoxes by default, but since we're not using one, we need to set them explicitly.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            // This timer is used to re-enable High Quality Graphics, which are disabled when a scroll action is performed (for performance reasons).
            scrollHighQualityTimer.Interval = DnDMapConstants.OnScrollHighQualityTimerInterval;
            scrollHighQualityTimer.Tick += new EventHandler(scrollHighQualityTimer_Tick);
            
            isInitialized = true;
        }

        protected virtual void Initialize()
        { 
        }

        protected virtual ImageAttributes CreateFogAttributes()
        {
            var fogAttributes = new ImageAttributes();

            if (FogAlpha != 255)
            {
                // All colors are alpha blended by the alpha specified
                float[][] fogMatrixItems = {
                                               new float[] { 1, 0, 0, 0, 0 },
                                               new float[] { 0, 1, 0, 0, 0 },
                                               new float[] { 0, 0, 1, 0, 0 },
                                               new float[] { 0, 0, 0, ((float)FogAlpha) / 255f, 0 },
                                               new float[] { 0, 0, 0, 0, 1 }
                                           };
                fogAttributes.SetColorMatrix(new ColorMatrix(fogMatrixItems), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            }
            fogAttributes.SetColorKey(DnDMapConstants.FOG_CLEAR_BRUSH.Color, DnDMapConstants.FOG_CLEAR_BRUSH.Color, ColorAdjustType.Bitmap);
            return fogAttributes;
        }

        protected override void Dispose(bool disposing)
        {
            if (LoadedMap != null)
                LoadedMap.Dispose();
            if (gridPen != null)
                gridPen.Dispose();
            base.Dispose(disposing);
        }

        #endregion Init and Cleanup

        #region Setters

        public virtual void SetMapAsync(Image newMap)
        {
            if (newMap == null)
                return;

            var newFog = new Bitmap(newMap.Width, newMap.Height);
            using (var g = Graphics.FromImage(newFog))
                g.Clear(DnDMapConstants.FOG_BRUSH_COLOR);

            this.Invoke(new Action(() =>
            {
                var oldMap = this.LoadedMap;
                var oldFog = this.Fog;

                this.LoadedMap = newMap;
                this.LoadedMapSize = newMap.Size;
                this.Fog = newFog;
                OnNewMapAndFogSet();

                if (oldMap != null)
                    oldMap.Dispose();
                if (oldFog != null)
                    oldFog.Dispose();
            }));

            if (OnNewMapSet != null)
                OnNewMapSet(this.LoadedMap);

            SetScroll(0, 0);
            this.RefreshAll();
        }

        public virtual void SetFogAsync(Image newFog)
        {
            var newFogBitmap = newFog as Bitmap;
            if (newFogBitmap == null)
            {
                newFogBitmap = new Bitmap(newFogBitmap);
                newFog.Dispose();
            }

            this.BeginInvoke(new Action(() =>
            {
                this.Fog = newFogBitmap;
                RefreshAll();
            }));
        }

        protected virtual void OnNewMapAndFogSet()
        {
        }

        public void SetGridSize(bool showGrid, int? newGridSize)
        {
            if (showGrid && newGridSize.HasValue)
            {
                // Show the grid, but if the grid is already shown at that size, do nothing.
                if (this.gridSize.HasValue && this.gridSize.Value == newGridSize.Value)
                    return;
                this.gridSize = newGridSize.Value;
            }
            else
            {
                // Hide the grid, but if the grid was already hidden, do nothing.
                if (!this.gridSize.HasValue)
                    return;
                this.gridSize = null;
            }
            RefreshAll();
        }

        public void SetGridColor(SimpleColor gridColor)
        {
            if (gridPen != null)
                gridPen.Dispose();
            gridPen = new Pen(Color.FromArgb(gridColor.A, gridColor.R, gridColor.G, gridColor.B));

            RefreshAll();
        }

        public void SetCenterMap(SimplePoint centerMap)
        {
            // Take the point that we want to show, and center it on the client's UI.
            this.BeginInvoke(new Action(() =>
            {
                // The point that came in is raw on the map...
                var x = centerMap.X;
                var y = centerMap.Y;

                // If the view is Flipped, then we also need to flip the centering point so it's where it actually is on our map.
                if (IsFlippedView)
                {
                    x = this.LoadedMapSize.Width - x;
                    y = this.LoadedMapSize.Height - y;
                }

                // We also need to account for the client's zoom factor.
                x = (int)(x * AssignedZoomFactor) - this.Width / 2;
                y = (int)(y * AssignedZoomFactor) - this.Height / 2;

                SetScroll(x, y);
            }));
        }

        #endregion Setters

        #region Fog Actions

        public virtual void FogOrRevealAll(bool fogAll)
        {
            throw new NotImplementedException("Must be overridden.");
        }

        #endregion Fog Actions

        #region Map Events

        private void HandleLostFocus(object sender, EventArgs e)
        {
            this.Focus();
        }

        public void RefreshAll(bool immediateRefresh = false)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => { RefreshAll(immediateRefresh); }));
                return;
            }

            if (immediateRefresh)
                this.Refresh();
            else
                this.Invalidate();
        }

        private void HandlePreviewKeyDownEevent(object sender, PreviewKeyDownEventArgs e)
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
                if ((e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Down))
                    ZoomInOrOut((e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Up), e.Shift);
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
                        ScrollUpOrDown(true, null, keyboardScrollAccel);
                        keyboardScrollAccel += 0.5d;
                        break;
                    case Keys.Down:
                        ScrollUpOrDown(false, null, keyboardScrollAccel);
                        keyboardScrollAccel += 0.5d;
                        break;
                }

                switch (e.KeyCode)
                {
                    case Keys.Left:
                        ScrollLeftOrRight(true, null, keyboardScrollAccel);
                        keyboardScrollAccel += 0.5d;
                        break;
                    case Keys.Right:
                        ScrollLeftOrRight(false, null, keyboardScrollAccel);
                        keyboardScrollAccel += 0.5d;
                        break;
                }
            }
        }

        private void HandleKeyUpEvent(object sender, KeyEventArgs e)
        {
            keyboardScrollAccel = 1.0d;
        }

        private void HandleMouseWheelEvent(object sender, MouseEventArgs e)
        {
            HandleMouseWheel(e);
        }

        public void HandleMouseWheel(MouseEventArgs e)
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

            RefreshAll();
        }

        private void HandleMouseClick(object sender, MouseEventArgs e)
        {
            if (IsZoomFactorInProgress && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right))
            {
                CommitOrRollBackZoom((e.Button == MouseButtons.Left));
            }
        }

        private void HandleMouseDownEvent(object sender, MouseEventArgs e)
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
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
                return;

            lastScrollDragPosition = e.Location;
            dragMapOldCursor = this.Cursor;
            this.Cursor = Cursors.SizeAll;
        }

        private void HandleMouseMoveEvent(object sender, MouseEventArgs e)
        {
            HandleMouseMove(e);
        }

        /// <summary> By default, scroll drags for left/right buttons. Override to change this behavior. </summary>
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
            // If we need to cause a minimum-threshold for dragging, we can put it here.
            const int MoveThreshold = 0;

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

            // Because MouseMove events happen very often, we need to ensure the Repaint happens every time.
            this.RefreshAll(true);
        }

        private void HandleMouseUpEvent(object sender, MouseEventArgs e)
        {
            HandleMouseUp(e);
        }

        protected virtual void HandleMouseUp(MouseEventArgs e)
        {
            if (this.LoadedMap == null)
                return;
            if (IsZoomFactorInProgress)
                return;

            HandleMouseUp_Drag(e);
        }

        protected void HandleMouseUp_Drag(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
                return;

            this.Cursor = dragMapOldCursor;
        }

        #endregion Map Events

        #region Zoom Logic

        private void ZoomInOrOut(bool zoomIn, bool doubleFactor)
        {
            if (!AllowZoom)
                return;

            if (zoomIn)
                variableZoomFactor = (float)Math.Round(Math.Min(variableZoomFactor + ((doubleFactor) ? DnDMapConstants.ZoomLargeStep : DnDMapConstants.ZoomStep), ConfigValues.MaximumGridZoomFactor), 1);
            else
                variableZoomFactor = (float)Math.Round(Math.Max(variableZoomFactor - ((doubleFactor) ? DnDMapConstants.ZoomLargeStep : DnDMapConstants.ZoomStep), ConfigValues.MinimumGridZoomFactor), 1);

            IsZoomFactorInProgress = true;

            RefreshAll();
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
            RefreshAll();
        }

        #endregion Zoom Logic

        #region Scroll Logic

        private void ScrollLeftOrRight(bool isLeft, int? distance = null, double factor = 1.0)
        {
            // Scroll left/right
            int newValue;
            if (isLeft)
                newValue = this.ScrollPosition.X - (int)((distance ?? (int)(this.Width * DnDMapConstants.ScrollWheelStepScrollPercent)) * factor);
            else
                newValue = this.ScrollPosition.X + (int)((distance ?? (int)(this.Width * DnDMapConstants.ScrollWheelStepScrollPercent)) * factor);
            SetScroll(newValue, null);
        }

        private void ScrollUpOrDown(bool isUp, int? distance = null, double factor = 1.0)
        {
            // Scroll up/down
            int newValue;
            if (isUp)
                newValue = this.ScrollPosition.Y - (int)((distance ?? (int)(this.Width * DnDMapConstants.ScrollWheelStepScrollPercent)) * factor);
            else
                newValue = this.ScrollPosition.Y + (int)((distance ?? (int)(this.Width * DnDMapConstants.ScrollWheelStepScrollPercent)) * factor);
            SetScroll(null, newValue);
        }

        protected void SetScroll(int? desiredX, int? desiredY)
        {
            if (SuppressScroll)
                return;

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
            if (this.LogicalMapWidth < this.Width)
                desiredX = 0;
            else
                desiredX = Math.Min(desiredX.Value, this.LogicalMapWidth - this.Width);

            if (this.LogicalMapHeight < this.Height)
                desiredY = 0;
            else
                desiredY = Math.Min(desiredY.Value, this.LogicalMapHeight - this.Height);

            this.ScrollPosition = new Point(desiredX.Value, desiredY.Value);

            UseHighQuality = false;
            scrollHighQualityTimer.Start();

            RefreshAll();

            if (OnScrollStep != null)
                OnScrollStep(this.ScrollPosition);
        }

        private void scrollHighQualityTimer_Tick(object sender, EventArgs e)
        {
            UseHighQuality = true;
            scrollHighQualityTimer.Stop();
            this.RefreshAll();
        }

        #endregion Scroll Logic

        #region Painting

        /// <summary> Repaint event occurs every time we request it, or when the user scrolls. </summary>
        private void HandlePaintEvent(object sender, PaintEventArgs e)
        {
            try
            {
                if (!UseHighQuality)
                {
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    e.Graphics.SmoothingMode = SmoothingMode.None;
                    e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                }

                PaintAll(e.Graphics);
            }
            catch (Exception e1)
            {
                Logger.LogError("Painting Failure", e1);
            }
        }

        protected virtual void PaintAll(Graphics graphics)
        {
            throw new NotImplementedException("Must be overridden.");
        }

        protected void PaintMap(TransformedGraphics g)
        {
            if (this.LoadedMap != null)
            {
                // Because our Graphics instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
                // take from the Fog Image starting at the Translated Location and go the full width of our client view only. Note that we explicitly Min/Max the values to prevent trying
                // to source from off the image.
                var sourceX = Math.Max(0, this.ScrollPosition.X);
                var sourceY = Math.Max(0, this.ScrollPosition.Y);
                var sourceWidth = Math.Min(this.LoadedMap.Width - sourceX, this.VisibleSize.Width);
                var sourceHeight = Math.Min(this.LoadedMap.Height - sourceY, this.VisibleSize.Height);
                var source = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);

                var destinationX = Math.Max(0, this.ScrollPosition.X);
                var destinationY = Math.Max(0, this.ScrollPosition.Y);
                var destinationWidth = Math.Min(this.LoadedMap.Width - sourceX, this.VisibleSize.Width);
                var destinationHeight = Math.Min(this.LoadedMap.Height - sourceY, this.VisibleSize.Height);
                var destination = new Rectangle(destinationX, destinationY, destinationWidth, destinationHeight);

                g.Graphics.DrawImage(this.LoadedMap, destination, source, GraphicsUnit.Pixel);
            }
        }

        protected void PaintGrid(TransformedGraphics g)
        {
            if (!gridSize.HasValue)
                return;

            // Because our Graphics instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
            // start at the Translated location, and go the full width of our client view only. Note that because our scroll
            // may be in between two grid lines, we'll need to pull back (or go forward) one full step in all directions to ensure
            // the client view is fully grid lined. The end result is that we'll only draw as many grid lines as we need, starting 
            // and ending just beyond what the user can actually see.
            var startX = this.ScrollPosition.X;
            startX = startX - (startX % gridSize.Value);
            var endX = this.ScrollPosition.X + this.VisibleSize.Width;
            endX = endX + (gridSize.Value - (endX % gridSize.Value));

            var startY = this.ScrollPosition.Y;
            startY = startY - (startY % gridSize.Value);
            var endY = this.ScrollPosition.Y + this.VisibleSize.Height;
            endY = endY + (gridSize.Value - (endY % gridSize.Value));

            // Constrain our start/end to within the map itself.
            startX = Math.Max(0, Math.Min(startX, this.LoadedMapSize.Width));
            startY = Math.Max(0, Math.Min(startY, this.LoadedMapSize.Height));
            endX = Math.Min(endX, this.LoadedMapSize.Width);
            endY = Math.Min(endY, this.LoadedMapSize.Height);

            var x = startX;
            var y = startY;
            while (x < endX || y < endY)
            {
                if (x < endX)
                {
                    g.Graphics.DrawLine(gridPen, x, startY, x, endY);
                    x += gridSize.Value;
                }
                if (y < endY)
                {
                    g.Graphics.DrawLine(gridPen, startX, y, endX, y);
                    y += gridSize.Value;
                }
            }

            // This commented block draws the full Grid.
            //for (var x = 0; x < LoadedMapSize.Width; x += gridSize.Value)
            //{
            //    g.Graphics.DrawLine(gridPen, x, 0, x, LoadedMapSize.Height);
            //}
            //for (var y = 0; y < LoadedMapSize.Height; y += gridSize.Value)
            //{
            //    g.Graphics.DrawLine(gridPen, 0, y, LoadedMapSize.Width, y);
            //}
        }

        protected void PaintFog(TransformedGraphics g)
        {
            if (Fog != null)
            {
                // Because our Graphics instance is already translated, (0, 0) may be somewhere off screen (further top/left), so we'll
                // take from the Fog Image starting at the Translated Location and go the full width of our client view only. Note that we explicitly Min/Max the values to prevent trying
                // to source from off the image.
                var sourceX = Math.Max(0, this.ScrollPosition.X);
                var sourceY = Math.Max(0, this.ScrollPosition.Y);
                var sourceWidth = Math.Min(this.Fog.Width - sourceX, this.VisibleSize.Width);
                var sourceHeight = Math.Min(this.Fog.Height - sourceY, this.VisibleSize.Height);

                var destinationX = Math.Max(0, this.ScrollPosition.X);
                var destinationY = Math.Max(0, this.ScrollPosition.Y);
                var destinationWidth = Math.Min(this.Fog.Width - sourceX, this.VisibleSize.Width);
                var destinationHeight = Math.Min(this.Fog.Height - sourceY, this.VisibleSize.Height);

                g.Graphics.DrawImage(Fog, new Rectangle(destinationX, destinationY, destinationWidth, destinationHeight), sourceX, sourceY, sourceWidth, sourceHeight, GraphicsUnit.Pixel, this.FogAttributes);
            }
        }

        protected TransformedGraphics Translate(Graphics g)
        {
            return g.TranslateAndZoom(this.ScrollPosition, this.LoadedMapSize, 1.0f, this.IsFlippedView);
        }

        protected TransformedGraphics TranslateAndZoom(Graphics g)
        {
            return g.TranslateAndZoom(this.ScrollPosition, this.LoadedMapSize, AssignedZoomFactor, this.IsFlippedView);
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
                    var x = (this.Width / 2.0f) - (msgSize.Width / 2.0f);
                    var y = (this.Height / 2.0f) - (msgSize.Height / 2.0f) + msgSize.Height * i;
                    y += ZoomFactorTextYOffset;

                    g.DrawString(zoomMsgs[i], font, Brushes.Aqua, x, y);
                }
            }
        }

        #endregion Painting
    }
}
