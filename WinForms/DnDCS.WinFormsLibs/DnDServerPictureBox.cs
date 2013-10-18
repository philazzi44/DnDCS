using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using DnDCS.Libs.SimpleObjects;
using System.Drawing.Imaging;
using DnDCS.WinFormsLibs.Assets;

namespace DnDCS.WinFormsLibs
{
    public class DnDServerPictureBox : DnDPictureBox
    {
        public enum Tool
        {
            SelectTool,
            FogAddTool,
            FogRemoveTool,
        }

        private Tool currentTool;
        public Tool CurrentTool
        {
            get { return this.currentTool; }
            set
            {
                if (this.currentTool == value)
                    return;

                switch (this.currentTool)
                {
                    case Tool.SelectTool:
                        break;
                    case Tool.FogAddTool:
                    case Tool.FogRemoveTool:
                        this.pbxMap.Cursor = Cursors.Arrow;
                        this.IsRemovingFog = null;
                        break;
                }

                switch (value)
                {
                    case Tool.SelectTool:
                        break;
                    case Tool.FogAddTool:
                        break;
                    case Tool.FogRemoveTool:
                        break;
                }

                this.currentTool = value;
            }
        }
        
        private List<FogUpdate> allFogUpdates = new List<FogUpdate>();

        public Bitmap Fog { get; private set; }
        private Bitmap newFog;

        private FogUpdate currentFogUpdate;
        private readonly LinkedList<FogUpdate> undoFogUpdates = new LinkedList<FogUpdate>();
        private readonly LinkedList<FogUpdate> redoFogUpdates = new LinkedList<FogUpdate>();

        public bool AnyUndoFogUpdates { get { return this.undoFogUpdates.Any(); } }
        public bool AnyRedoFogUpdates { get { return this.redoFogUpdates.Any(); } }

        public bool? IsRemovingFog { get; set; }

        private readonly Brush newFogClearBrush = Brushes.Red;

        private readonly SolidBrush fogClearBrush = new SolidBrush(Color.White);

        /// <summary> This is the brush that is used to draw on the Fog bitmap. </summary>
        private static readonly Brush FOG_BRUSH = Brushes.Black;
        private readonly Brush newFogBrush = Brushes.Gray;
        private readonly ImageAttributes fogAttributes = new ImageAttributes();

        private Pen gridPen;
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

        /// <summary> When set, the Center Map Image will be shown at the location for a brief period of time. </summary>
        private DateTime? centerMapImageAnimationStartTime;
        private Point? lastCenterMapPoint;
        private Timer centerMapPointDrawTimer;
        /// <summary> The duration of the Center Map Image icon being shown. </summary>
        private readonly TimeSpan centerMapImageAnimationDuration = TimeSpan.FromSeconds(1);
        private Image centerMapImage;
        public event Action<SimplePoint> PerformCenterMap;

        public DnDServerPictureBox()
        { 
        }

        protected override void Initialize()
        {
            base.Initialize();

            centerMapImage = AssetsLoader.CenterMapOverlayIcon;
            centerMapPointDrawTimer = new Timer();
            centerMapPointDrawTimer.Tick += new EventHandler(centerMapPointDrawTimer_Tick);
            this.centerMapPointDrawTimer.Interval = 1000;
        }

        public void TryUndoLastFogAction()
        {
            if (AnyUndoFogUpdates)
            {
                var lastFogUpdate = undoFogUpdates.Last();
                lastFogUpdate.IsClearing = !lastFogUpdate.IsClearing;
                UpdateFogImage(lastFogUpdate);
                redoFogUpdates.AddLast(lastFogUpdate);
                undoFogUpdates.RemoveLast();
                base.RefreshMapPictureBox();
            }
        }

        public void TryRedoLastFogAction()
        {
            if (AnyRedoFogUpdates)
            {
                var lastFogUpdate = redoFogUpdates.Last();
                lastFogUpdate.IsClearing = !lastFogUpdate.IsClearing;
                UpdateFogImage(lastFogUpdate);
                undoFogUpdates.AddLast(lastFogUpdate);
                redoFogUpdates.RemoveLast();
                pbxMap.Refresh();
            }
        }

        public void FogOrRevealAll(bool revealAll)
        {
            var fogAllFogUpdate = new FogUpdate(revealAll);
            fogAllFogUpdate.Add(new SimplePoint(0, 0));
            fogAllFogUpdate.Add(new SimplePoint(this.Fog.Width, 0));
            fogAllFogUpdate.Add(new SimplePoint(this.Fog.Width, this.Fog.Height));
            fogAllFogUpdate.Add(new SimplePoint(0, this.Fog.Height));

            UpdateFogImage(fogAllFogUpdate);
            undoFogUpdates.Clear();
            redoFogUpdates.Clear();
            this.RefreshMapPictureBox();
        }

        private void pbxMap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.LoadedMap == null)
                return;

            if (e.Button == MouseButtons.Left && e.Clicks >= 2)
            {
                if (PerformCenterMap != null)
                {
                    // Get the coordinates in real map coordinates by unwinding the Scroll and Zoom factor.
                    lastCenterMapPoint = e.Location.Translate(base.ScrollPosition);
                    PerformCenterMap(lastCenterMapPoint.Value.ToSimplePoint());
                    this.centerMapImageAnimationStartTime = DateTime.Now;
                    centerMapPointDrawTimer.Enabled = true;
                    this.RefreshMapPictureBox();
                }
            }
        }

        private Point lastDragPosition;

        protected override void HandleMouseDown(MouseEventArgs e)
        {
            if (base.LoadedMap == null)
                return;
            if (IsZoomFactorInProgress)
                return;

            var doMouseDrag = false;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                doMouseDrag = (this.CurrentTool == Tool.SelectTool);

                if ((this.CurrentTool == Tool.FogAddTool || this.CurrentTool == Tool.FogRemoveTool) && IsRemovingFog.HasValue)
                {
                    newFog = new Bitmap(Fog.Width, Fog.Height);

                    currentFogUpdate = new FogUpdate(this.IsRemovingFog.Value);
                    currentFogUpdate.Add(e.Location.Translate(base.ScrollPosition).ToSimplePoint());
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                doMouseDrag = (this.CurrentTool == Tool.SelectTool || ((this.CurrentTool == Tool.FogAddTool || this.CurrentTool == Tool.FogRemoveTool) && IsRemovingFog.HasValue));
            }

            if (doMouseDrag)
            {
                HandleMouseDown_DragMap(e);
            }
        }

        protected override void HandleMouseMove(MouseEventArgs e)
        {
            if (this.LoadedMap == null)
                return;
            if (IsZoomFactorInProgress)
                return;

            var doMouseDrag = false;

            if (e.Button == MouseButtons.Left)
            {
                doMouseDrag = (this.CurrentTool == Tool.SelectTool);

                if ((this.CurrentTool == Tool.FogAddTool || this.CurrentTool == Tool.FogRemoveTool) && IsRemovingFog.HasValue)
                {
                    if (currentFogUpdate == null)
                        return;

                    // We ignore events firing too fast so that we don't end up with several points that are simply too close to each other to matter.
                    if (DateTime.Now - lastMouseMoveTime < MouseMoveInterval)
                        return;
                    lastMouseMoveTime = DateTime.Now;

                    // Update the New Fog image with the newly added point, so it can be drawn on the screen in real time.
                    Console.WriteLine(e.Location.Translate(this.ScrollPosition).ToSimplePoint());
                    currentFogUpdate.Add(e.Location.Translate(this.ScrollPosition).ToSimplePoint());

                    UpdateNewFogImage(currentFogUpdate);

                    pbxMap.Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                doMouseDrag = true;
            }

            if (doMouseDrag)
            {
                HandleMouseMove_DragMap(e);
            }
        }

        protected void HandleMouseUp(object sender, MouseEventArgs e)
        {
            if (this.LoadedMap == null)
                return;
            if (IsZoomFactorInProgress)
                return;
            if (currentFogUpdate == null)
                return;
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            if ((this.CurrentTool == Tool.FogAddTool || this.CurrentTool == Tool.FogRemoveTool) && IsRemovingFog.HasValue)
            {
                var toBeDisposedFog = newFog;
                newFog = null;
                if (toBeDisposedFog != null)
                    toBeDisposedFog.Dispose();

                // Commit the last point onto the main Fog Image then clear out the 'New Fog' temporary image altogether. Note that if we don't have
                // at least 3 points, then we don't have a shape that can be used.

                currentFogUpdate.Add(e.Location.Translate(this.ScrollPosition).ToSimplePoint());
                if (currentFogUpdate.Length >= 3)
                {
                    UpdateFogImage(currentFogUpdate);
                    undoFogUpdates.AddLast(currentFogUpdate);

                    if (OnFogUpdateAdded != null)
                        OnFogUpdateAdded(currentFogUpdate);

                    this.RefreshMapPictureBox();
                    
                    currentFogUpdate = null;
                }
            }
        }

        public event Action<FogUpdate> OnFogUpdateAdded;
    }
}
