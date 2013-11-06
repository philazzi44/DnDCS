using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using DnDCS.Libs.SimpleObjects;
using DnDCS.Win.Libs.Assets;
using System.ComponentModel;

namespace DnDCS.Win.Libs
{
    public class DnDServerPictureBox : DnDPictureBox
    {
        private bool IsToolFogAddOrRemove { get { return (this.CurrentTool == DnDMapConstants.Tool.FogAddTool || this.CurrentTool == DnDMapConstants.Tool.FogRemoveTool); } }

        public event Action<FogUpdate> OnOneFogUpdatesChanged;
        public event Action OnManyFogUpdatesChanged;

        private DnDMapConstants.Tool currentTool;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DnDMapConstants.Tool CurrentTool
        {
            get { return this.currentTool; }
            set
            {
                if (this.currentTool == value)
                    return;

                switch (this.currentTool)
                {
                    case DnDMapConstants.Tool.SelectTool:
                        break;
                    case DnDMapConstants.Tool.FogAddTool:
                    case DnDMapConstants.Tool.FogRemoveTool:
                        this.Cursor = Cursors.Arrow;
                        this.IsRemovingFog = null;
                        break;
                }

                switch (value)
                {
                    case DnDMapConstants.Tool.SelectTool:
                        break;
                    case DnDMapConstants.Tool.FogAddTool:
                        this.Cursor = Cursors.Cross;
                        this.IsRemovingFog = false;
                        break;
                    case DnDMapConstants.Tool.FogRemoveTool:
                        this.Cursor = Cursors.Cross;
                        this.IsRemovingFog = true;
                        break;
                }

                this.currentTool = value;
            }
        }

        private readonly List<FogUpdate> allFogUpdates = new List<FogUpdate>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<FogUpdate> AllFogUpdates { get { return this.allFogUpdates.ToArray(); } }

        private bool isNewFogClearing;
        private readonly List<Point> newFogUpdatePoints = new List<Point>();
        private DateTime lastMouseMoveNewFogPointTime = DateTime.MinValue;

        private readonly LinkedList<FogUpdate> undoFogUpdates = new LinkedList<FogUpdate>();
        private readonly LinkedList<FogUpdate> redoFogUpdates = new LinkedList<FogUpdate>();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AnyUndoFogUpdates { get { return this.undoFogUpdates.Any(); } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AnyRedoFogUpdates { get { return this.redoFogUpdates.Any(); } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool? IsRemovingFog { get; set; }

        /// <summary> When set, the Center Map Image will be shown at the location for a brief period of time. </summary>
        private readonly int centerMapImageDisplayDuration = 1000;
        private Point? lastCenterMapPoint;
        private Timer centerMapPointDrawTimer;
        private Image centerMapImage;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event Action<SimplePoint> PerformCenterMap;

        #region Init and Cleanup

        protected override void Initialize()
        {
            base.Initialize();

            centerMapImage = AssetsLoader.CenterMapOverlayIcon;
            centerMapPointDrawTimer = new Timer();
            centerMapPointDrawTimer.Tick += (o, e) =>
            {
                // When it fires, simply disable the timer altogether and hide the image.
                lastCenterMapPoint = null;
                centerMapPointDrawTimer.Stop();
                base.RefreshAll();
            };
            this.centerMapPointDrawTimer.Interval = centerMapImageDisplayDuration;

            this.MouseDoubleClick += new MouseEventHandler(HandleMouseDoubleClickEvent);
        }

        protected override ImageAttributes CreateFogAttributes()
        {
            // All colors are alpha blended by the alpha specified
            float[][] fogMatrixItems = { new float[] {1, 0, 0, 0, 0},
                                         new float[] {0, 1, 0, 0, 0},
                                         new float[] {0, 0, 1, 0, 0},
                                         new float[] {0, 0, 0, ((float)FogAlpha) / 255f, 0}, 
                                         new float[] {0, 0, 0, 0, 1}
                                    };
            var fogAttributes = new ImageAttributes();
            fogAttributes.SetColorMatrix(new ColorMatrix(fogMatrixItems), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            fogAttributes.SetColorKey(DnDMapConstants.FOG_CLEAR_BRUSH.Color, DnDMapConstants.FOG_CLEAR_BRUSH.Color, ColorAdjustType.Bitmap);
            return fogAttributes;
        }

        #endregion Init and Cleanup

        #region Setters

        protected override void OnNewMapAndFogSet()
        {
            allFogUpdates.Clear();
        }

        #endregion Setters

        #region Fog Actions

        public FogUpdate TryUndoLastFogAction()
        {
            if (AnyUndoFogUpdates)
            {
                var lastFogUpdate = undoFogUpdates.Last();
                lastFogUpdate.IsClearing = !lastFogUpdate.IsClearing;
                UpdateFogImage(lastFogUpdate);
                redoFogUpdates.AddLast(lastFogUpdate);
                undoFogUpdates.RemoveLast();
                base.RefreshAll();
                return lastFogUpdate;
            }

            return null;
        }

        public FogUpdate TryRedoLastFogAction()
        {
            if (AnyRedoFogUpdates)
            {
                var lastFogUpdate = redoFogUpdates.Last();
                lastFogUpdate.IsClearing = !lastFogUpdate.IsClearing;
                UpdateFogImage(lastFogUpdate);
                undoFogUpdates.AddLast(lastFogUpdate);
                redoFogUpdates.RemoveLast();
                this.RefreshAll();
                return lastFogUpdate;
            }

            return null;
        }

        public override void FogOrRevealAll(bool fogAll)
        {
            var fogAllFogUpdate = new FogUpdate(!fogAll);
            fogAllFogUpdate.Add(new SimplePoint(0, 0));
            fogAllFogUpdate.Add(new SimplePoint(this.Fog.Width, 0));
            fogAllFogUpdate.Add(new SimplePoint(this.Fog.Width, this.Fog.Height));
            fogAllFogUpdate.Add(new SimplePoint(0, this.Fog.Height));

            UpdateFogImage(fogAllFogUpdate, true, true);
            undoFogUpdates.Clear();
            redoFogUpdates.Clear();
            this.RefreshAll();
        }

        #endregion Fog Actions

        #region Fog Updates

        private void UpdateFogImage(FogUpdate fogUpdate, bool ignoreFogAlphaEffect = false, bool ignoreEvents = false)
        {
            UpdateFogImage(new[] { fogUpdate }, ignoreFogAlphaEffect, ignoreEvents);
        }

        private void UpdateFogImage(FogUpdate[] fogUpdates, bool ignoreFogAlphaEffect = false, bool ignoreEvents = false)
        {
            if (!ignoreFogAlphaEffect && UseFogAlphaEffect)
            {
                if (!ImageProcessing.ApplyFogInwards(this.Fog, fogUpdates))
                    return;
            }
            else
            {
                if (!ImageProcessing.ApplyFogDirect(this.Fog, fogUpdates))
                    return;
                this.RefreshAll(true);
            }

            allFogUpdates.AddRange(fogUpdates);

            if (!ignoreEvents)
            {
                if (fogUpdates.Length == 1 && OnOneFogUpdatesChanged != null)
                    OnOneFogUpdatesChanged(fogUpdates[0]);
                if (fogUpdates.Length > 1 && OnManyFogUpdatesChanged != null)
                    OnManyFogUpdatesChanged();
            }
        }

        public void SetFogUpdates(FogUpdate[] fogUpdates)
        {
            using (var g = Graphics.FromImage(this.Fog))
                g.Clear(DnDMapConstants.FOG_BRUSH_COLOR);

            this.allFogUpdates.Clear();
            UpdateFogImage(fogUpdates);
            base.RefreshAll();
        }

        #endregion Fog Updates

        #region Map Events

        private void HandleMouseDoubleClickEvent(object sender, MouseEventArgs e)
        {
            if (this.LoadedMap == null)
                return;
            // If we're currently dealing with drawing New Fog, we'll suck all Mouse Double Click events into oblivion.
            if (newFogUpdatePoints.Count > 2)
                return;

            // If we only want Left or Right, then comment one of these out.
            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) && e.Clicks >= 2)
            {
                if (PerformCenterMap != null)
                {
                    // Get the coordinates in real map coordinates by unwinding the Scroll and Zoom factor.
                    lastCenterMapPoint = e.Location.Translate(base.ScrollPosition);
                    PerformCenterMap(lastCenterMapPoint.Value.ToSimplePoint());
                    centerMapPointDrawTimer.Stop();
                    centerMapPointDrawTimer.Start();
                    this.RefreshAll();
                }
            }
        }

        protected override void HandleMouseDown(MouseEventArgs e)
        {
            // If we're currently dealing with drawing New Fog, we'll suck all Mouse Down events into oblivion.
            if (newFogUpdatePoints.Count > 0)
                return;

            // Anything this override cannot handle will delegate to the base implementation.
            if (base.LoadedMap == null ||
                IsZoomFactorInProgress
                || e.Button != System.Windows.Forms.MouseButtons.Left)
            {
                base.HandleMouseDown(e);
                return;
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this.CurrentTool == DnDMapConstants.Tool.SelectTool)
                {
                    HandleMouseDown_DragMap(e);
                }
                else if (IsToolFogAddOrRemove && IsRemovingFog.HasValue)
                {
                    UseHighQuality = false;
                    base.SuppressScroll = true;
                    newFogUpdatePoints.Clear();
                    isNewFogClearing = IsRemovingFog.Value;
                    newFogUpdatePoints.Add(e.Location.Translate(base.ScrollPosition));
                }
            }
        }

        protected override void HandleMouseMove(MouseEventArgs e)
        {
            // Anything this override cannot handle will delegate to the base implementation.
            if (base.LoadedMap == null ||
                IsZoomFactorInProgress
                || e.Button != System.Windows.Forms.MouseButtons.Left)
            {
                base.HandleMouseMove(e);
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (this.CurrentTool == DnDMapConstants.Tool.SelectTool)
                {
                    HandleMouseMove_DragMap(e);
                }
                else if (IsToolFogAddOrRemove && IsRemovingFog.HasValue && newFogUpdatePoints.Count > 0)
                {
                    var now = DateTime.Now;

                    // We ignore events firing too fast so that we don't end up with several points that are simply too close to each other to matter.
                    if (now - lastMouseMoveNewFogPointTime < DnDMapConstants.MouseMoveNewFogPointIgnoreInterval)
                        return;
                    lastMouseMoveNewFogPointTime = now;

                    // Add the new location to the list of points for the new fog, so it can be drawn on the screen in real time.
                    newFogUpdatePoints.Add(e.Location.Translate(this.ScrollPosition));
                    
                    // Because MouseMove events happen very often, we need to ensure the Repaint happens every time.
                    this.RefreshAll(true);
                }
            }
        }

        protected override void HandleMouseUp(MouseEventArgs e)
        {
            // If we're currently dealing with drawing New Fog, we'll suck all Mouse Up events into oblivion that aren't Left Click.
            if (newFogUpdatePoints.Count > 0 && e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            // Anything this override cannot handle will delegate to the base implementation.
            if (base.LoadedMap == null || 
                IsZoomFactorInProgress || 
                newFogUpdatePoints.Count == 0
                || e.Button != System.Windows.Forms.MouseButtons.Left)
            {
                base.HandleMouseUp(e);
                return;
            }

            if (IsToolFogAddOrRemove)
            {
                // Commit the last point onto the main Fog Image. Note that if we don't have at least 3 points, then we don't have a shape that can be used.
                newFogUpdatePoints.Add(e.Location.Translate(this.ScrollPosition));
                if (newFogUpdatePoints.Count >= 3)
                {
                    var fogUpdate = new FogUpdate(newFogUpdatePoints.Select(p => p.ToSimplePoint()), isNewFogClearing);
                    undoFogUpdates.AddLast(fogUpdate);
                    UpdateFogImage(fogUpdate);
                }
                newFogUpdatePoints.Clear();
                base.SuppressScroll = false;
                UseHighQuality = true;
                base.RefreshAll();
            }
            else
            {
                base.HandleMouseUp(e);
            }
        }

        #endregion Map Events

        #region Painting

        protected override void PaintAll(Graphics g)
        {
            using (var transformedGraphics = TranslateAndZoom(g))
            {
                PaintMap(transformedGraphics);
                PaintGrid(transformedGraphics);
                PaintFog(transformedGraphics);
            }

            PaintNewFog(g);
            PaintCenterMapOverlayIcon(g);
            PaintZoomFactorText(g);
        }

        private void PaintNewFog(Graphics graphics)
        {
            if (newFogUpdatePoints.Count >= 3)
            {
                using (var g = Translate(graphics))
                {
                    g.Graphics.FillPolygon((isNewFogClearing) ? DnDMapConstants.NEW_FOG_CLEAR_BRUSH : DnDMapConstants.NEW_FOG_BRUSH, newFogUpdatePoints.ToArray());
                }
            }
        }

        private void PaintCenterMapOverlayIcon(Graphics graphics)
        {
            if (centerMapImage != null && lastCenterMapPoint.HasValue)
            {
                // Draw it at the location, so that the bottom/center is at the centered point.
                using (var g = Translate(graphics))
                {
                    g.Graphics.DrawImage(centerMapImage, lastCenterMapPoint.Value.Translate(-(centerMapImage.Width / 2), -centerMapImage.Height));
                }
            }
        }

        #endregion Painting
    }
}
