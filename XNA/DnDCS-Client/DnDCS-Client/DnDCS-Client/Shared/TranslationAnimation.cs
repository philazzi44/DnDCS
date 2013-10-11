using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DnDCS_Client.Shared
{
    public class TranslationAnimation
    {
        public struct Easing
        {
            public readonly float StartPercent;
            public readonly float EndPercent;
            public readonly float StartPerSecondPercent;
            public readonly float EndPerSecondPercent;

            public Easing(float startPercent, float endPercent, float startPerSecondPercent, float endPerSecondPercent)
            {
                StartPercent = startPercent;
                EndPercent = endPercent;
                StartPerSecondPercent = startPerSecondPercent;
                EndPerSecondPercent = endPerSecondPercent;
            }

            public override string ToString()
            {
                return string.Format("{0} @ {1} to {2} @ {3}", StartPercent, StartPerSecondPercent, EndPercent, EndPerSecondPercent);
            }
        }

        public TimeSpan StartGameTime { get; private set; }
        public float StartX { get; private set; }
        public float StartY { get; private set; }

        public float XPerSecond { get; private set; }
        public float YPerSecond { get; private set; }

        private Easing[] xEasings;
        private Easing[] yEasings;

        [Obsolete]
        public float CurrentDuration { get { return (float)(CurrentGameTime.TotalSeconds - StartGameTime.TotalSeconds); } }

        private float currentToLastGameTimeDelta;
        public TimeSpan LastGameTime { get; private set; }
        public TimeSpan CurrentGameTime { get; private set; }
        public float CurrentX { get; private set; }
        public float CurrentXPercent { get { return (CurrentX - StartX) / (EndX - StartX); } }
        public float CurrentY { get; private set; }
        public float CurrentYPercent { get { return (CurrentY - StartY) / (EndY - StartY); } }

        public float EndX { get; private set; }
        public float EndY { get; private set; }

        public bool IsCompleteX { get; private set; }
        public bool IsCompleteY { get; private set; }
        public bool IsComplete { get { return (IsCompleteX && IsCompleteY); } }
        public Action OnComplete { get; set; }

        /// <summary> Creates an X/Y translation. </summary>
        /// <param name="startX"> The starting X coordinate. </param>
        /// <param name="startY">The starting Y coordinate. </param>
        /// <param name="endX"> The ending X coordinate. If no X translation required, set to same as startX. </param>
        /// <param name="endY"> The ending Y coordinate. If no Y translation required, set to same as startY. </param>
        /// <param name="totalDuration"> The total time (in fractional seconds) it should take for the animation to complete. Note that any Easing applied will cause this to take longer due to slowing down the animation. </param>
        /// <param name="gameTime"> The starting Game Time recorded. </param>
        public TranslationAnimation(float startX, float startY, float endX, float endY, float totalDuration, GameTime gameTime)
            : this(startX, startY, endX, endY, totalDuration, totalDuration, gameTime)
        {
        }

        /// <summary> Creates an X/Y translation. </summary>
        /// <param name="startX"> The starting X coordinate. </param>
        /// <param name="startY">The starting Y coordinate. </param>
        /// <param name="endX"> The ending X coordinate. If no X translation required, set to same as startX. </param>
        /// <param name="endY"> The ending Y coordinate. If no Y translation required, set to same as startY. </param>
        /// <param name="totalXDuration"> The total time (in fractional seconds) it should take for the X animation to complete. Note that any Easing applied will cause this to take longer due to slowing down the animation. </param>
        /// <param name="totalYDuration"> The total time (in fractional seconds) it should take for the Y animation to complete. Note that any Easing applied will cause this to take longer due to slowing down the animation. </param>
        /// <param name="gameTime"> The starting Game Time recorded. </param>
        public TranslationAnimation(float startX, float startY, float endX, float endY, float totalXDuration, float totalYDuration, GameTime gameTime)
        {
            if (totalXDuration == 0 && totalYDuration == 0)
                throw new InvalidOperationException("X and Y translations cannot both be zero, or there's no animation to be done.");

            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            XPerSecond = (totalXDuration == 0.0f) ? (endX - startX) : (endX - startX) / totalXDuration;
            YPerSecond = (totalYDuration == 0.0f) ? (endY - startY) : (endY - startY) / totalYDuration;
            StartGameTime = gameTime.TotalGameTime;

            CurrentX = StartX;
            CurrentY = StartY;
            CurrentGameTime = gameTime.TotalGameTime;
            LastGameTime = gameTime.TotalGameTime;
            IsCompleteX = (XPerSecond == 0);
            IsCompleteY = (YPerSecond == 0);
        }

        private static void AssertEasingPercents(float startDeltaPercent, float endDeltaPercent)
        {
            if (startDeltaPercent == 0.0f)
                throw new ArgumentException("Value cannot be zero or the animation will freeze upon hitting this speed percentage.", "startDeltaPercent");
            if (endDeltaPercent == 0.0f)
                throw new ArgumentException("Value cannot be zero or the animation will freeze upon hitting this speed percentage.", "endDeltaPercent");
        }

        /// <summary> Adds horizontal easing. This will delay the total duration of the animation depending on the percentages used. </summary>
        public void AddHorizontalEasing(float startTimePercent, float endTimePercent, float startDeltaPercent, float endDeltaPercent)
        {
            AssertEasingPercents(startDeltaPercent, endDeltaPercent);

            var newEasing = new Easing(startTimePercent, endTimePercent, startDeltaPercent, endDeltaPercent);
            if (xEasings == null)
                xEasings = new Easing[] { newEasing };
            else
                xEasings = xEasings.Concat(new Easing[] { newEasing }).OrderBy(x => x.StartPercent).ToArray();
        }

        /// <summary> Adds vertical easing. This will delay the total duration of the animation depending on the percentages used. </summary>
        public void AddVerticalEasing(float startTimePercent, float endTimePercent, float startDeltaPercent, float endDeltaPercent)
        {
            AssertEasingPercents(startDeltaPercent, endDeltaPercent);

            var newEasing = new Easing(startTimePercent, endTimePercent, startDeltaPercent, endDeltaPercent);
            if (yEasings == null)
                yEasings = new Easing[] { newEasing };
            else
                yEasings = yEasings.Concat(new Easing[] { newEasing }).OrderBy(y => y.StartPercent).ToArray();
        }

        public void Update(GameTime gameTime)
        {
            if (IsComplete)
                return;

            LastGameTime = CurrentGameTime;
            CurrentGameTime = gameTime.TotalGameTime;
            this.currentToLastGameTimeDelta = (float)(CurrentGameTime.TotalSeconds - LastGameTime.TotalSeconds);

            Update_X();
            Update_Y();
            
            if (IsComplete && OnComplete != null)
            {
                OnComplete();
            }
        }

        public void Update_X()
        {
            if (!IsCompleteX && XPerSecond != 0)
            {
                var xPerSecondPercent = GetPerSecondFactor(xEasings, CurrentXPercent);
                var newCurrentX = CurrentX + (float)(currentToLastGameTimeDelta * XPerSecond * xPerSecondPercent);

                Debug.Add("XPerSecondPercent: " + xPerSecondPercent);
                Debug.Add("newCurrentX: " + newCurrentX);

                if (XPerSecond > 0)
                    CurrentX = Math.Min(newCurrentX, EndX);
                else if (XPerSecond < 0)
                    CurrentX = Math.Max(newCurrentX, EndX);

                Debug.Add("CurrentX: " + CurrentX);

                IsCompleteX = (CurrentX == EndX);
            }
        }
        
        public void Update_Y()
        {
            if (!IsCompleteY && YPerSecond != 0)
            {
                var yPerSecondFactor = GetPerSecondFactor(yEasings, CurrentYPercent);
                var newCurrentY = CurrentY + (float)(currentToLastGameTimeDelta * YPerSecond * yPerSecondFactor);

                Debug.Add("yPerSecondFactor: " + yPerSecondFactor);
                Debug.Add("newCurrentY: " + newCurrentY);

                if (YPerSecond > 0)
                    CurrentY = Math.Min(newCurrentY, EndY);
                else if (YPerSecond < 0)
                    CurrentY = Math.Max(newCurrentY, EndY);

                Debug.Add("CurrentY: " + CurrentY);

                IsCompleteY = (CurrentY == EndY);
            }
        }

        /// <summary> Gets a value between 0.0f and 1.0f that we should multiply against the XPerSecond or YPerSecond value to know how many units we should move in this Update frame. </summary>
        private float GetPerSecondFactor(Easing[] easings, float currentPercent)
        {
            if (easings == null || !easings.Any())
                return 1.0f;

            Easing? closestEasing = null;
            if (easings[0].StartPercent > currentPercent)
            {
                // We're before the first easing we have, so we'll use the first easing's initial speed.
                return easings[0].StartPerSecondPercent;
            }

            foreach (var easing in easings)
            {
                if (easing.StartPercent <= currentPercent)
                {
                    if (easing.EndPercent >= currentPercent)
                    {
                        //return easing.EndPerSecondPercent;

                        // We're somewhere within this Easing, so let's get the specific percent of the PerSecond value we should be using.
                        // (Current - Start) / (End - Start)
                        // Example: 0.0 to 0.5, current at 0.2 -> (0.2 - 0.0) / (0.5 - 0.0) -> 0.2 / 0.5 = 0.4f
                        // Example: 0.5 to 0.75, current at 0.67 -> (0.67 - 0.5) / (0.75 - 0.5) -> 0.17 / 0.25 = 0.68f
                        // Taking this value gives us how much of the PerSecond we should be using for this round. However, the full PerSecond
                        // is also eased between the Start and End time slot, so we'll use the formula to determine how much of the full PerSecond
                        // we should be using.
                        var perSecondPercent = (currentPercent - easing.StartPercent) / (easing.EndPercent - easing.StartPercent);

                        // The below examples work for Acceleration and Deceleration)
                        //  Accel (0.25 to 0.8 at   0% time): 0.0f * (0.8 - 0.25) + 0.25 = 0.0 *  0.55f + 0.25 =  0.0f   + 0.25 = 0.25  = 25.0% of the YPerSecond for this frame.
                        //        (0.25 to 0.8 at  20% time): 0.2f * (0.8 - 0.25) + 0.25 = 0.2 *  0.55f + 0.25 =  0.11f  + 0.25 = 0.36  = 36.0% of the YPerSecond for this frame.
                        //        (0.25 to 0.8 at  50% time): 0.5f * (0.8 - 0.25) + 0.25 = 0.5 *  0.55f + 0.25 =  0.275f + 0.25 = 0.525 = 52.5% of the YPerSecond for this frame.
                        //        (0.25 to 0.8 at  80% time): 0.8f * (0.8 - 0.25) + 0.25 = 0.8 *  0.55f + 0.25 =  0.44f  + 0.25 = 0.69  = 69.0% of the YPerSecond for this frame.
                        //        (0.25 to 0.8 at 100% time): 1.0f * (0.8 - 0.25) + 0.25 = 1.0 *  0.55f + 0.25 =  0.55f  + 0.25 = 0.8   = 80.0% of the YPerSecond for this frame.
                        // Deccel (0.8 to 0.25 at   0% time): 0.0f * (0.25 - 0.8) + 0.8  = 0.0 * -0.55f + 0.8  =  0.0f   + 0.8  = 0.8   = 25.0% of the YPerSecond for this frame.
                        //        (0.8 to 0.25 at  20% time): 0.2f * (0.25 - 0.8) + 0.8  = 0.2 * -0.55f + 0.8  = -0.11f  + 0.8  = 0.69  = 69.0% of the YPerSecond for this frame.
                        //        (0.8 to 0.25 at  50% time): 0.5f * (0.25 - 0.8) + 0.8  = 0.5 * -0.55f + 0.8  = -0.275f + 0.8  = 0.525 = 52.5% of the YPerSecond for this frame.
                        //        (0.8 to 0.25 at  80% time): 0.8f * (0.25 - 0.8) + 0.8  = 0.8 * -0.55f + 0.8  = -0.44f  + 0.8  = 0.36  = 36.0% of the YPerSecond for this frame.
                        //        (0.8 to 0.25 at 100% time): 1.0f * (0.25 - 0.8) + 0.8  = 1.0 * -0.55f + 0.8  = -0.55f  + 0.8  = 0.25  = 25.0% of the YPerSecond for this frame.
                        return (perSecondPercent * (easing.EndPerSecondPercent - easing.StartPerSecondPercent)) + easing.StartPerSecondPercent;
                    }
                    else
                    {
                        // We're beyond the End of the easing, so this will be our new 'Closest' one to where we're at.
                        closestEasing = easing;
                    }
                }
            }

            // This easing is the last one where we passed the End but didn't fall into another bucket, so we'll go at the max of this easing bracket, if any. Otherwise, max speed.
            if (closestEasing.HasValue)
            {
                return closestEasing.Value.EndPerSecondPercent;
            }
            return 1.0f;
        }
    }
}
