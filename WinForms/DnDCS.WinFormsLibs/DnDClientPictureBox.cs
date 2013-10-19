using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using DnDCS.WinFormsLibs.Assets;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS.WinFormsLibs
{
    public class DnDClientPictureBox : DnDPictureBox
    {
        private bool isBlackoutOn;
        public bool IsBlackoutOn
        {
            get { return this.isBlackoutOn; }
            set
            {
                this.isBlackoutOn = value;
                this.RefreshMapPictureBox();
            }
        }

        // If we're also showing the Blackout image, then show the text beneath it.
        protected override int ZoomFactorTextYOffset { get { return (IsBlackoutOn) ? AssetsLoader.BlackoutImage.Height : 0; } }

        #region Init and Cleanup
        
        protected override void Dispose(bool disposing)
        {
            if (Fog != null)
                Fog.Dispose();
            base.Dispose(disposing);
        }

        #endregion Init and Cleanup

        #region Setters

        // TODO: This is 99% identical to the Server version
        public override void SetMapAsync(Image newMap)
        {
            if (newMap == null)
                return;

            var newFog = new Bitmap(newMap.Width, newMap.Height);
            using (var g = Graphics.FromImage(newFog))
                g.Clear(DnDMapConstants.FOG_BRUSH_COLOR);

            this.BeginInvoke(new Action(() =>
            {
                var oldMap = base.LoadedMap;
                var oldFog = this.Fog;

                base.LoadedMap = newMap;
                base.LoadedMapSize = newMap.Size;
                this.Fog = newFog;
                base.RefreshMapPictureBox();

                if (oldMap != null)
                    oldMap.Dispose();
                if (oldFog != null)
                    oldFog.Dispose();

            }));
        }

        public void SetFogAsync(Image newFog)
        {
            this.BeginInvoke(new Action(() =>
            {
                this.Fog = newFog;
                RefreshMapPictureBox();
            }));
        }

        public void SetFogUpdateAsync(FogUpdate fogUpdate)
        {
            Image fogImageToUpdate;
            var isNewFogImage = (this.Fog == null);
            if (isNewFogImage)
                fogImageToUpdate = new Bitmap(base.LoadedMapSize.Width, base.LoadedMapSize.Height);
            else
                fogImageToUpdate = this.Fog;

            using (var g = Graphics.FromImage(fogImageToUpdate))
            {
                if (isNewFogImage)
                    g.FillRectangle(DnDMapConstants.FOG_BRUSH, 0, 0, fogImageToUpdate.Width, fogImageToUpdate.Height);
                g.FillPolygon((fogUpdate.IsClearing) ? DnDMapConstants.FOG_CLEAR_BRUSH : DnDMapConstants.FOG_BRUSH, fogUpdate.Points.Select(p => p.ToPoint()).ToArray());
            }

            if (isNewFogImage)
                this.Fog = fogImageToUpdate;

            RefreshMapPictureBox();
        }

        public void SetCenterMap(SimplePoint centerMap)
        {
            // Take the point that we want to show, and center it on the client's UI.
            this.BeginInvoke(new Action(() =>
            {
                // The point that came in is raw on the map, so we need to account for the client's zoom factor.
                SetScroll((int)(centerMap.X * AssignedZoomFactor) - this.pbxMap.Width / 2, (int)(centerMap.Y * AssignedZoomFactor) - this.pbxMap.Height / 2);
            }));
        }

        #endregion Setters

        #region Painting

        protected override void PaintAll(Graphics g)
        {
            if (this.IsBlackoutOn)
            {
                PaintBlackout(g);
            }
            else
            {
                using (var transformedGraphics = TranslateAndZoom(g))
                {
                    PaintMap(transformedGraphics);
                    PaintGrid(transformedGraphics);
                    PaintFog(transformedGraphics);
                }

                PaintZoomFactorText(g);
            }
        }

        private void PaintBlackout(Graphics g)
        {
            // Draw the Blackout Image in the center.
            g.Clear(Color.Black);
            g.DrawImage(AssetsLoader.BlackoutImage, this.pbxMap.Width / 2.0f - AssetsLoader.BlackoutImage.Width / 2.0f, this.pbxMap.Height / 2.0f - AssetsLoader.BlackoutImage.Height / 2.0f);
        }

        #endregion Painting
    }
}
