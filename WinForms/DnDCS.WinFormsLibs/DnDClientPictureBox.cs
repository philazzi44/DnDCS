using System;
using System.Drawing;
using System.Linq;
using DnDCS.Libs.SimpleObjects;
using DnDCS.WinFormsLibs.Assets;

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
                this.RefreshAll();
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

        public void SetFogAsync(Image newFog)
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

        public void SetFogUpdateAsync(FogUpdate fogUpdate)
        {
            Bitmap fogImageToUpdate;
            var isNewFogImage = (this.Fog == null);
            if (isNewFogImage)
            {
                fogImageToUpdate = new Bitmap(base.LoadedMapSize.Width, base.LoadedMapSize.Height);
                using (var g = Graphics.FromImage(fogImageToUpdate))
                {
                    g.FillRectangle(DnDMapConstants.FOG_BRUSH, 0, 0, fogImageToUpdate.Width, fogImageToUpdate.Height);
                }
            }
            else
            {
                fogImageToUpdate = this.Fog;
            }

            var doAction = new Action(() =>
                    {
                        if (this.UseFogAlphaEffect)
                        {
                            ImageProcessing.ApplyFogInwards(fogImageToUpdate, fogUpdate);
                        }
                        else
                        {
                            ImageProcessing.ApplyFogDirect(fogImageToUpdate, fogUpdate);
                        }
                    });

            // For new images, it's not tied to the control in any way so we can perform the update on the thread. Otherwise, we need
            // to sync up to the main thread to draw on the image.
            if (isNewFogImage || !this.InvokeRequired)
            {
                doAction();
                if (isNewFogImage)
                    this.Fog = fogImageToUpdate;
            }
            else
                this.Invoke(doAction);

            RefreshAll();
        }

        public void SetCenterMap(SimplePoint centerMap)
        {
            // Take the point that we want to show, and center it on the client's UI.
            this.BeginInvoke(new Action(() =>
            {
                // The point that came in is raw on the map, so we need to account for the client's zoom factor.
                SetScroll((int)(centerMap.X * AssignedZoomFactor) - this.Width / 2, (int)(centerMap.Y * AssignedZoomFactor) - this.Height / 2);
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
            g.DrawImage(AssetsLoader.BlackoutImage, this.Width / 2.0f - AssetsLoader.BlackoutImage.Width / 2.0f, this.Height / 2.0f - AssetsLoader.BlackoutImage.Height / 2.0f);
        }

        #endregion Painting
    }
}
