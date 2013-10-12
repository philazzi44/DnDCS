using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DnDCS_Client.Shared
{
    public abstract class BaseAnimation
    {
        public const int REPEAT_FOREVER = -1;

        public string LogName { get; set; }

        public bool IsRunning { get; protected set; }
        public TimeSpan StartGameTime { get; protected set; }
        public TimeSpan LastGameTime { get; protected set; }
        public TimeSpan CurrentGameTime { get; protected set; }

        public bool Repeat { get; protected set; }
        protected bool isRepeating;
        public float RepeatDelay { get; protected set; }
        public int RepeatCount { get; protected set; }
        public int CurrentRepeatCount { get; protected set; }

        public virtual bool IsComplete { get; protected set; }
        public Action OnComplete { get; set; }

        public BaseAnimation()
        {
            LogName = "Base Animation";
        }

        public virtual void Start(GameTime startTime)
        {
            if (IsRunning)
                throw new InvalidOperationException("Can only start a non-running animation.");

            Reset();

            StartGameTime = startTime.TotalGameTime;
            CurrentGameTime = startTime.TotalGameTime;
            LastGameTime = startTime.TotalGameTime;
            IsRunning = true;
        }

        public virtual void Stop(bool reset = false)
        {
            IsRunning = false;
            if (reset)
                Reset();
        }

        /// <summary> Resets any values that would allow the animation to start again. Can only be called when IsRunning is false. </summary>
        public virtual void Reset()
        {
            if (IsRunning)
                throw new InvalidOperationException("Can only reset a non-running animation.");

            IsComplete = false;
            isRepeating = false;
            CurrentRepeatCount = 0;
        }

        public virtual void SetRepeat(float repeatDelay, int repeatToIndex = 0, int repeatCount = REPEAT_FOREVER)
        {
            if (IsRunning)
                throw new InvalidOperationException("Cannot change repeat values for a running animation.");

            Repeat = true;
            RepeatDelay = repeatDelay;
            RepeatCount = repeatCount;
        }

        protected abstract void Update(GameTime gameTime);

        public void Update(GameTime gameTime, bool startIfNeeded = true)
        {
            if (IsComplete)
                return;
            else if (!IsRunning)
            {
                if (startIfNeeded)
                    Start(gameTime);
                else
                    return;
            }

            LastGameTime = CurrentGameTime;
            CurrentGameTime = gameTime.TotalGameTime;

            Debug.Add(LogName);
            Update(gameTime);

            if (IsComplete && OnComplete != null)
            {
                IsRunning = false;
                OnComplete();
            }
        }
    }
}
