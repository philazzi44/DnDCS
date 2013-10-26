using System;
using Microsoft.Xna.Framework;

namespace DnDCS.XNA.Client.Shared.Animations
{
    public class Frame2DTranslationAnimation
    {
        private Frame2DAnimation frame;
        public Frame2DAnimation Frame
        {
            get { return frame; }
            set
            {
                if (updateOrder != null)
                {
                    if (updateOrder[0] == frame)
                        updateOrder[0] = value;
                    else
                        updateOrder[1] = value;
                }
                frame = value;
            }
        }

        private TranslationAnimation translation;
        public TranslationAnimation Translation
        {
            get { return translation; }
            set
            {
                if (updateOrder != null)
                {
                    if (updateOrder[0] == translation)
                        updateOrder[0] = value;
                    else
                        updateOrder[1] = value;
                }
                translation = value;
            }
        }

        private BaseAnimation[] updateOrder;

        /// <summary> Creates a Frame and Translation Animation where the two animations will be set manually as available, and the Frame will be updated before the Translation. Note that each animation can be updated separately if needed. </summary>
        public Frame2DTranslationAnimation()
        { 
        }

        public Frame2DTranslationAnimation(Frame2DAnimation frame, TranslationAnimation translation)
        {
            Frame = frame;
            Translation = translation;
            updateOrder = new BaseAnimation[] { Frame, Translation };
        }

        /// <summary> Creates a Translation and Frame Animation, with the Translation being updated before the Frame. Note that each animation can be updated separately if needed. </summary>
        public Frame2DTranslationAnimation(TranslationAnimation translation, Frame2DAnimation frame)
        {
            Frame = frame;
            Translation = translation;
            updateOrder = new BaseAnimation[] { Translation, Frame };
        }

        private void AssertValues()
        {
            if (Frame == null || Translation == null)
                throw new InvalidOperationException("FrameAnimation is null.");
            if (Translation == null)
                throw new InvalidOperationException("TranslationAnimation is null.");

            if (updateOrder == null || updateOrder[0] == null || updateOrder[1] == null)
                updateOrder = new BaseAnimation[] { Frame, Translation };
        }

        public void Start(GameTime startTime)
        {
            AssertValues();

            foreach (var baseAnimation in updateOrder)
            {
                baseAnimation.Start(startTime);
            }
        }

        public void Update(GameTime gameTime, bool startIfNeeded = true)
        {
            AssertValues();

            foreach (var baseAnimation in updateOrder)
            {
                baseAnimation.Update(gameTime, startIfNeeded);
            }
        }

        public void Stop(bool reset = false)
        {
            AssertValues();

            foreach (var baseAnimation in updateOrder)
            {
                baseAnimation.Stop(reset);
            }
        }
    }
}
