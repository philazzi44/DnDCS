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
            public readonly float StartTimePercent;
            public readonly float EndTimePercent;
            public readonly float StartTimePerSecondPercent;
            public readonly float EndTimePerSecondPercent;

            public Easing(float startTimePercent, float endTimePercent, float startTimePerSecondPercent, float endTimePerSecondPercent)
            {
                StartTimePercent = startTimePercent;
                EndTimePercent = endTimePercent;
                StartTimePerSecondPercent = startTimePerSecondPercent;
                EndTimePerSecondPercent = endTimePerSecondPercent;
            }
        }

        public TimeSpan StartGameTime { get; private set; }
        public float StartX { get; private set; }
        public float StartY { get; private set; }

        public float XPerSecond { get; private set; }
        public float YPerSecond { get; private set; }

        private Easing[] xEasings;
        private Easing[] yEasings;

        public float CurrentDuration { get { return (float)(CurrentGameTime.TotalSeconds - StartGameTime.TotalSeconds); } }

        private float currentToLastGameTimeDelta;
        public TimeSpan LastGameTime { get; private set; }
        public TimeSpan CurrentGameTime { get; private set; }
        public float CurrentX { get; private set; }
        public float CurrentY { get; private set; }
        public float CurrentXDurationPercent { get { return Math.Min(CurrentDuration / ExpectedXDuration, 1.0f); } }
        public float CurrentYDurationPercent { get { return Math.Min(CurrentDuration / ExpectedYDuration, 1.0f); } }

        public float EndX { get; private set; }
        public float EndY { get; private set; }
        public float ExpectedXDuration { get; private set; }
        public float ExpectedYDuration { get; private set; }

        public bool IsCompleteX { get; private set; }
        public bool IsCompleteY { get; private set; }
        public bool IsComplete { get { return (IsCompleteX && IsCompleteY); } }
        public Action OnComplete { get; set; }

        public static TranslationAnimation CreateXTranslation(float startX, float startY, float endY, float yPerSecond, GameTime gameTime)
        {
            return new TranslationAnimation(startX, startY, startX, endY, 0, yPerSecond, gameTime);
        }

        public static TranslationAnimation CreateYTranslation(float startX, float startY, float endX, float xPerSecond, GameTime gameTime)
        {
            return new TranslationAnimation(startX, startY, startX, startY, xPerSecond, 0, gameTime);
        }

        /// <summary> Creates an X/Y translation. Static mehtods exist to create individual translations. </summary>
        /// <param name="startX"> The starting X coordinate. </param>
        /// <param name="startY">The starting Y coordinate. </param>
        /// <param name="endX"> The ending X coordinate. If no X translation required, set to same as startX. </param>
        /// <param name="endY"> The ending Y coordinate. If no Y translation required, set to same as startY. </param>
        /// <param name="xPerSecond"> The X position to move per second. May be set to zero. </param>
        /// <param name="yPerSecond"> The Y position to move per second. May be set to zero. </param>
        /// <param name="gameTime"> The starting Game Time recorded. </param>
        public TranslationAnimation(float startX, float startY, float endX, float endY, float xPerSecond, float yPerSecond, GameTime gameTime)
        {
            if (xPerSecond == 0 && yPerSecond == 0)
                throw new InvalidOperationException("X and Y translation per second cannot both be zero.");

            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            XPerSecond = xPerSecond;
            YPerSecond = yPerSecond;
            StartGameTime = gameTime.TotalGameTime;

            // From 100 to 500 at 100/s would last 4s.
            ExpectedXDuration = Math.Abs(Math.Abs(EndX) - Math.Abs(StartX)) / Math.Abs(xPerSecond);
            ExpectedYDuration = Math.Abs(Math.Abs(EndY) - Math.Abs(StartY)) / Math.Abs(yPerSecond);

            CurrentX = StartX;
            CurrentY = StartY;
            CurrentGameTime = gameTime.TotalGameTime;
            LastGameTime = gameTime.TotalGameTime;
            IsCompleteX = (xPerSecond == 0);
            IsCompleteY = (yPerSecond == 0);
        }

        public void AddXEasing(float startTimePercent, float endTimePercent, float startDeltaPercent, float endDeltaPercent)
        {
            var newEasing = new Easing(startTimePercent, endTimePercent, startDeltaPercent, endDeltaPercent);
            if (xEasings == null)
                xEasings = new Easing[] { newEasing };
            else
                xEasings = xEasings.Concat(new Easing[] { newEasing }).OrderBy(x => x.StartTimePercent).ToArray();
        }

        public void AddYEasing(float startTimePercent, float endTimePercent, float startDeltaPercent, float endDeltaPercent)
        {
            var newEasing = new Easing(startTimePercent, endTimePercent, startDeltaPercent, endDeltaPercent);
            if (yEasings == null)
                yEasings = new Easing[] { newEasing };
            else
                yEasings = yEasings.Concat(new Easing[] { newEasing }).OrderBy(y => y.StartTimePercent).ToArray();
        }

        public void Update(GameTime gameTime)
        {
            if (IsComplete)
                return;

            LastGameTime = CurrentGameTime;
            CurrentGameTime = gameTime.TotalGameTime;
            this.currentToLastGameTimeDelta = (float)(CurrentGameTime.TotalSeconds - LastGameTime.TotalSeconds);

            Update_X(gameTime);
            Update_Y(gameTime);
            
            if (IsComplete && OnComplete != null)
            {
                OnComplete();
            }
        }

        public void Update_X(GameTime gameTime)
        {
            if (!IsCompleteX && XPerSecond != 0)
            {
                var xPerSecondPercent = GetPerSecondFactor(gameTime, xEasings, CurrentXDurationPercent);
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
        
        public void Update_Y(GameTime gameTime)
        {
            if (!IsCompleteY && YPerSecond != 0)
            {
                var yPerSecondFactor = GetPerSecondFactor(gameTime, yEasings, CurrentYDurationPercent);
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
        private float GetPerSecondFactor(GameTime gameTime, Easing[] easings, float currentDurationPercent)
        {
            if (easings == null || !easings.Any())
                return 1.0f;

            Easing? closestEasing = null;
            if (easings[0].StartTimePercent > currentDurationPercent)
            {
                // We're before the first easing we have, so we'll use the first easing's initial speed.
                return easings[0].StartTimePerSecondPercent;
            }

            foreach (var easing in easings)
            {
                if (easing.StartTimePercent <= currentDurationPercent)
                {
                    if (easing.EndTimePercent >= currentDurationPercent)
                    {
                        // We're somewhere within this Easing, so let's get the specific percent of the PerSecond value we should be using.
                        // (Current - Start) / (End - Start)
                        // Example: 0.0 to 0.5, current at 0.2 -> (0.2 - 0.0) / (0.5 - 0.0) -> 0.2 / 0.5 = 0.4f
                        // Example: 0.5 to 0.75, current at 0.67 -> (0.67 - 0.5) / (0.75 - 0.5) -> 0.17 / 0.25 = 0.68f
                        // Taking this value gives us how much of the PerSecond we should be using for this round. However, the full PerSecond
                        // is also eased between the Start and End time slot, so we'll use the formula to determine how much of the full PerSecond
                        // we should be using.
                        var perSecondPercent = (currentDurationPercent - easing.StartTimePercent) / (easing.EndTimePercent - easing.StartTimePercent);

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
                        return (perSecondPercent * (easing.EndTimePerSecondPercent - easing.StartTimePerSecondPercent)) + easing.StartTimePerSecondPercent;
                    }
                    else
                    {
                        // We're beyond the End of the easing, so this will be our new 'Closest' one to where we're at.
                        closestEasing = easing;
                    }
                }
            }

            // This easing is the last one where we passed the End time but didn't fall into another bucket, so we'll go at the max of this easing bracket, if any. Otherwise, max speed.
            if (closestEasing.HasValue)
            {
                return closestEasing.Value.EndTimePerSecondPercent;
            }
            return 1.0f;
        }
    }
}
