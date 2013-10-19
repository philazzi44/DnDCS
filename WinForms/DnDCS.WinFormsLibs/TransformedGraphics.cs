using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DnDCS.WinFormsLibs
{
    public class TransformedGraphics : IDisposable
    {
        public Graphics Graphics { get; private set; }

        public Point Scroll { get; set; }
        public float Zoom { get; set; }

        public TransformedGraphics(Graphics g, Point scroll, float zoom)
        {
            this.Graphics = g;
            this.Scroll = scroll;
            this.Zoom = zoom;

            this.Graphics.TranslateTransform(-scroll.X, -scroll.Y);
            this.Graphics.ScaleTransform(zoom, zoom);
        }

        /// <summary> Exposed to allow for a 'using' statement, where the underlying Graphics' Transforms are reset. </summary>
        public void Dispose()
        {
            this.Graphics.ResetTransform();
        }
    }
}
