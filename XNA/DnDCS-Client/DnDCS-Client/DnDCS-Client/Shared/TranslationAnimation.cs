using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DnDCS_Client.Shared
{
    public class TranslationAnimation
    {
        public GameTime StartGameTime { get; private set; }
        public float StartX { get; private set; }
        public float StartY { get; private set; }

        public float EndX { get; private set; }
        public float EndY { get; private set; }

        public float XPerSecond { get; private set; }
        public float YPerSecond { get; private set; }

        private float currentDelta;
        public GameTime CurrentGameTime { get; private set; }
        public float CurrentX { get; private set; }
        public float CurrentY { get; private set; }
        public bool IsComplete { get; private set; }

        private readonly Action OnComplete;

        public TranslationAnimation(float startX, float startY, float endX, float endY, float xPerSecond, float yPerSecond, GameTime gameTime, Action onComplete)
        {
            if (xPerSecond == 0 && yPerSecond == 0)
                throw new InvalidOperationException("No translation specified.");
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            XPerSecond = xPerSecond;
            YPerSecond = yPerSecond;
            StartGameTime = gameTime;
            OnComplete = onComplete;

            Update(gameTime);
        }

        public void Update(GameTime gameTime)
        {
            CurrentGameTime = gameTime;
            this.currentDelta = (float)((CurrentGameTime.TotalGameTime.TotalSeconds - StartGameTime.TotalGameTime.TotalSeconds) / 1000d);

            var newCurrentX = (float)(currentDelta * XPerSecond);
            if (XPerSecond > 0)
            {
                CurrentX = Math.Min(newCurrentX, EndX);
            }
            else if (XPerSecond < 0)
            {
                CurrentX = Math.Max(newCurrentX, EndX);
            }
            else // if (XPerSecond == 0)
            {
                // TODO: This IF statement isn't done. We need to watch for COmpleteX and CompleteY, then AllComplete is actually true.
                // avoid checkin these IFs every single Update, if the CompleteX or Y is already true.
            }
            CurrentY = (float)(currentDelta * YPerSecond);
        }
    }
}
