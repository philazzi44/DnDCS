using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DnDCS_Client.Shared
{
    public class TranslationAnimation
    {
        public TimeSpan StartGameTime { get; private set; }
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
        public bool IsCompleteX { get; private set; }
        public bool IsCompleteY { get; private set; }
        public bool IsComplete { get { return (IsCompleteX && IsCompleteY); } }

        private readonly Action OnComplete;

        public static TranslationAnimation CreateXTranslation(float startX, float startY, float endY, float yPerSecond, GameTime gameTime, Action onComplete)
        {
            return new TranslationAnimation(startX, startY, startX, endY, 0, yPerSecond, gameTime, onComplete);
        }

        public static TranslationAnimation CreateYTranslation(float startX, float startY, float endX, float xPerSecond, GameTime gameTime, Action onComplete)
        {
            return new TranslationAnimation(startX, startY, startX, startY, xPerSecond, 0, gameTime, onComplete);
        }

        /// <summary> Creates an X/Y translation. Static mehtods exist to create individual translations. </summary>
        /// <param name="startX"> The starting X coordinate. </param>
        /// <param name="startY">The starting Y coordinate. </param>
        /// <param name="endX"> The ending X coordinate. If no X translation required, set to same as startX. </param>
        /// <param name="endY"> The ending Y coordinate. If no Y translation required, set to same as startY. </param>
        /// <param name="xPerSecond"> The X position to move per second. May be set to zero. </param>
        /// <param name="yPerSecond"> The Y position to move per second. May be set to zero. </param>
        /// <param name="gameTime"> The starting Game Time recorded. </param>
        /// <param name="onComplete"> Callback to raise once both X and Y translations are completed. </param>
        public TranslationAnimation(float startX, float startY, float endX, float endY, float xPerSecond, float yPerSecond, GameTime gameTime, Action onComplete)
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
            OnComplete = onComplete;

            CurrentX = StartX;
            CurrentY = StartY;
            CurrentGameTime = gameTime;
            IsCompleteX = (xPerSecond == 0);
            IsCompleteY = (yPerSecond == 0);
        }


        public void Update(GameTime gameTime)
        {
            if (IsComplete)
                return;

            CurrentGameTime = gameTime;
            this.currentDelta = (float)(CurrentGameTime.TotalGameTime.TotalSeconds - StartGameTime.TotalSeconds);

            Update_X(gameTime);
            Update_Y(gameTime);
            
            if (IsComplete && OnComplete != null)
            {
                OnComplete();
            }
        }

        public void Update_X(GameTime gameTime)
        {
            if (!IsCompleteX)
            {
                var newCurrentX = StartX + (float)(currentDelta * XPerSecond);
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
                    // Ignored
                }

                IsCompleteX = (CurrentX == EndX);
            }
        }

        public void Update_Y(GameTime gameTime)
        {
            if (!IsCompleteY)
            {
                var newCurrentY = StartY + (float)(currentDelta * YPerSecond);
                if (YPerSecond > 0)
                {
                    CurrentY = Math.Min(newCurrentY, EndY);
                }
                else if (YPerSecond < 0)
                {
                    CurrentY = Math.Max(newCurrentY, EndY);
                }
                else // if (YPerSecond == 0)
                {
                    // Ignored
                }
                IsCompleteY = (CurrentY == EndY);
            }
        }
    }
}
