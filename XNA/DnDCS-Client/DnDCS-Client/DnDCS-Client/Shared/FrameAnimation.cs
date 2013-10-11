using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DnDCS_Client.Shared
{
    public class FrameAnimation<T>
    {
        public bool IsStarted { get; private set; }

        public TimeSpan StartGameTime { get; private set; }
        private float ElapsedSinceStart { get { return (float)(CurrentGameTime.TotalSeconds - StartGameTime.TotalSeconds); } }

        public T[] Frames { get; private set; }
        public Tuple<float, int>[] FrameIntervals { get; private set; }

        public bool IsComplete { get; private set; }
        public Action OnComplete { get; set; }

        public bool Repeat { get; private set; }
        public float RepeatDelay { get; private set; }

        public TimeSpan LastGameTime { get; private set; }
        public TimeSpan CurrentGameTime { get; private set; }
        public int CurrentFrameIntervalIndex { get; private set; }
        public T CurrentFrame { get { return Frames[FrameIntervals[CurrentFrameIntervalIndex].Item2]; } }

        /// <summary> Creates a new Frame Translation. </summary>
        /// <param name="frames"> The frames to show. </param>
        /// <param name="frameIntervals"> The Frame Intervals to show for the frames. </param>
        public FrameAnimation(T[] frames, Tuple<float, int>[] frameIntervals)
        {
            if (frames.Length == 0 || frameIntervals.Length == 0)
                throw new InvalidOperationException("Frames and Interval arrays must both have at least 1 element.");

            Frames = frames;
            FrameIntervals = frameIntervals;
        }

        public void SetRepeat(float repeatDelay)
        {
            Repeat = true;
            RepeatDelay = repeatDelay;
        }

        public void Start(GameTime startTime)
        {
            StartGameTime = startTime.TotalGameTime;
            IsStarted = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsStarted || IsComplete)
                return;

            LastGameTime = CurrentGameTime;
            CurrentGameTime = gameTime.TotalGameTime;

            Update_Frame();

            if (IsComplete && OnComplete != null)
            {
                OnComplete();
            }
        }

        public void Update_Frame()
        {
            var elapsedSinceStart = ElapsedSinceStart;

            if (elapsedSinceStart < FrameIntervals[0].Item1)
            {
                // Still before the first interval, which should normally be 0.0f, so we'll show the first frame.
                CurrentFrameIntervalIndex = 0;
                return;
            }

            for (var i = CurrentFrameIntervalIndex; i < FrameIntervals.Length; i++)
            {
                if (elapsedSinceStart < FrameIntervals[i].Item1)
                {
                    // The frame we want to show will always be the one immediately after the one the time has just passed over.
                    // However, in situations where the interval is shorter than the Update cycle, we'll enforce that
                    // the frame will show at least briefly by simply incrementing the frame index by 1 ever time.
                    var newIndex = Math.Max(0, i - 1);
                    if (CurrentFrameIntervalIndex != newIndex)
                        CurrentFrameIntervalIndex++;
                    return;
                }
            }

            if (Repeat)
            {
                // If we're on the last frame, then we'll cycle over to the first one once the Repeat Delay elapses.
                // Otherwise, we'll just shift to the next frame because the Update cycle may simply be delayed.
                if (CurrentFrameIntervalIndex == (FrameIntervals.Length - 1))
                {
                    // Repeating, so wait until the last interval + the repeat delay before going back to frame 0 and restarting the Start Time again.
                    if (elapsedSinceStart > FrameIntervals.Last().Item1 + RepeatDelay)
                    {
                        CurrentFrameIntervalIndex = 0;
                        StartGameTime = CurrentGameTime;
                        return;
                    }
                }
                else
                {
                    CurrentFrameIntervalIndex++;
                }
            }
            else
            {
                // No repeat, so always show the last frame forever.
                CurrentFrameIntervalIndex = (FrameIntervals.Length - 1);
                IsComplete = true;
                return;
            }
        }
    }
}
