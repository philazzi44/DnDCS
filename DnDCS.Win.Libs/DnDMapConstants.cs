using System;
using System.Drawing;

namespace DnDCS.Win.Libs
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
        public const int FogAlphaEffectInwardsDelta = -30;
        public const int FogAlphaEffectOutwardsDelta = 30;
        public const int FogAlphaEffectRetryDelta = 5;

        public static readonly Brush FOG_BRUSH = Brushes.Black;
        public static readonly Color FOG_BRUSH_COLOR = Color.Black;
        public static readonly Brush NEW_FOG_CLEAR_BRUSH = Brushes.Red;
        public static readonly Brush NEW_FOG_BRUSH = Brushes.Gray;

        // For some reason, using Brushes.White doesn't end up with the desired result. TODO: Confirm this
        public static readonly SolidBrush FOG_CLEAR_BRUSH = new SolidBrush(Color.White);

        public static readonly TimeSpan MouseMoveDrawFogInterval = TimeSpan.FromMilliseconds(25d);

        /// <summary> The duration (in ms) between performing a scroll action and going back to using high quality graphics. </summary>
        public const int OnScrollHighQualityTimerInterval = 1000;

        public static readonly Color SelectedToolColor = Color.DarkRed;
    }
}
