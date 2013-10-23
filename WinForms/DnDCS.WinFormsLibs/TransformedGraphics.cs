using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DnDCS.WinFormsLibs
{
    public class TransformedGraphics : IDisposable
    {
        public Graphics Graphics { get; private set; }

        public Point Scroll { get; private set; }
        public float Zoom { get; private set; }
        public bool IsFlippedView { get; private set; }

        public TransformedGraphics(Graphics g, Point scroll, Size fullSize, float zoom, bool isFlippedView)
        {
            this.Graphics = g;
            this.Scroll = scroll;
            this.Zoom = zoom;
            this.IsFlippedView = isFlippedView;
            
            if (scroll != Point.Empty)
                this.Graphics.TranslateTransform(-scroll.X, -scroll.Y);

            if (zoom != 1.0f)
                this.Graphics.ScaleTransform(zoom, zoom);

            if (isFlippedView)
            {
                // TODO: Better approach would be to figure out the necessary transform matrix we need to apply (and where in the transform sets it needs to sit)
                // this.Graphics.MultiplyTransform(new Matrix(-1, 0, 0, 1, 0, 0));
                this.Graphics.TranslateTransform((int)(fullSize.Width * zoom / 2), (int)(fullSize.Height * zoom / 2));
                this.Graphics.RotateTransform(180);
                this.Graphics.TranslateTransform(-(int)(fullSize.Width * zoom / 2), -(int)(fullSize.Height * zoom / 2));
            }
        }

        /// <summary> Exposed to allow for a 'using' statement, where the underlying Graphics' Transforms are reset. </summary>
        public void Dispose()
        {
            this.Graphics.ResetTransform();
        }
    }
}
