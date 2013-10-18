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
        private Image fog;

        public bool IsBlackoutOn { get; set; }

        private readonly ImageAttributes fogAttributes = new ImageAttributes();
        private readonly SolidBrush fogClearBrush = new SolidBrush(Color.White);
        private readonly Brush fogBrush = Brushes.Black;
        private readonly Color fogColor = Color.Black;

        // If we're also showing the Blackout image, then show the text beneath it.
        protected override int ZoomFactorTextYOffset { get { return (IsBlackoutOn) ? AssetsLoader.BlackoutImage.Height : 0; } }
        
        public DnDClientPictureBox()
        { 
            
        }

        protected override void Initialize()
        {
            base.Initialize();

            fogAttributes.SetColorKey(fogClearBrush.Color, fogClearBrush.Color, ColorAdjustType.Bitmap);
        }

        protected override void Dispose(bool disposing)
        {
            if (fog != null)
                fog.Dispose();
            base.Dispose(disposing);
        }

        protected override void PaintAll(Graphics g)
        {
            if (this.IsBlackoutOn)
            {
                PaintBlackout(g);
            }
            else
            {
                if (this.LoadedMap == null)
                    return;

                PaintMap(g);
                PaintGrid(g);
                PaintFog(g);

                PaintZoomFactorText(g);
            }
        }

        public override void SetMapAsync(Image newMap)
        {
            var newFog = new Bitmap(newMap.Width, newMap.Height);
            using (var g = Graphics.FromImage(newFog))
                g.Clear(fogColor);

            this.BeginInvoke(new Action(() =>
            {
                base.LoadedMap = newMap;
                base.LoadedMapSize = newMap.Size;
                this.fog = newFog;
                base.RefreshMapPictureBox();
            }));
        }

        public void SetFogAsync(Image newFog)
        {
            this.BeginInvoke(new Action(() =>
            {
                this.fog = newFog;
                RefreshMapPictureBox();
            }));
        }

        public void SetFogUpdateAsync(FogUpdate fogUpdate)
        {
            Image fogImageToUpdate;
            var isNewFogImage = (this.fog == null);
            if (isNewFogImage)
                fogImageToUpdate = new Bitmap(base.LoadedMapSize.Width, base.LoadedMapSize.Height);
            else
                fogImageToUpdate = this.fog;

            using (var g = Graphics.FromImage(fogImageToUpdate))
            {
                if (isNewFogImage)
                    g.FillRectangle(fogBrush, 0, 0, fogImageToUpdate.Width, fogImageToUpdate.Height);
                g.FillPolygon((fogUpdate.IsClearing) ? fogClearBrush : fogBrush, fogUpdate.Points.Select(p => p.ToPoint()).ToArray());
            }

            if (isNewFogImage)
                this.fog = fogImageToUpdate;

            RefreshMapPictureBox();
        }

        public void CenterMap(SimplePoint centerMap)
        {
            // Take the point that we want to show, and center it on the client's UI.
            this.BeginInvoke(new Action(() =>
            {
                // The point that came in is raw on the map, so we need to account for the client's zoom factor.
                SetScroll((int)(centerMap.X * AssignedZoomFactor) - this.pbxMap.Width / 2, (int)(centerMap.Y * AssignedZoomFactor) - this.pbxMap.Height / 2);
            }));
        }

        private void PaintBlackout(Graphics g)
        {
            // Draw the Blackout Image in the center.
            g.Clear(Color.Black);
            g.DrawImage(AssetsLoader.BlackoutImage, this.pbxMap.Width / 2.0f - AssetsLoader.BlackoutImage.Width / 2.0f, this.pbxMap.Height / 2.0f - AssetsLoader.BlackoutImage.Height / 2.0f);
        }

        private void PaintFog(Graphics g)
        {
            if (fog != null)
            {
                g.TranslateTransform(-this.ScrollPosition.X, -this.ScrollPosition.Y);
                g.ScaleTransform(AssignedZoomFactor, AssignedZoomFactor);
                {
                    g.DrawImage(fog, new Rectangle(0, 0, LoadedMapSize.Width, LoadedMapSize.Height), 0, 0, fog.Width, fog.Height, GraphicsUnit.Pixel, fogAttributes);
                }
                g.ResetTransform();
            }
        }

    }
}
