using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DnDCS.WinFormsLibs
{
    public static class DnDMapConstants
    {
        public enum Tool
        {
            SelectTool,
            FogAddTool,
            FogRemoveTool,
        }

        public const float ZoomStep = 0.1f;
        public const float ZoomLargeStep = 0.2f;
        public const float ScrollWheelStepScrollPercent = 0.05f;
        public const byte DEFAULT_FOG_BRUSH_ALPHA = 90;

        public static readonly Brush FOG_BRUSH = Brushes.Black;
        public static readonly Color FOG_BRUSH_COLOR = Color.Black;
        // For some reason, using Brushes.White doesn't end up with the desired result. TODO: Confirm this
        public static readonly SolidBrush FOG_CLEAR_BRUSH = new SolidBrush(Color.White);

        public static readonly TimeSpan MouseMoveDrawFogInterval = TimeSpan.FromMilliseconds(25d);

    }
}
