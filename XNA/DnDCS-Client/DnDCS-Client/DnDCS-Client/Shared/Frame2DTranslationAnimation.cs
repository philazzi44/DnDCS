using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DnDCS_Client.Shared
{
    public class Frame2DTranslationAnimation
    {
        public Frame2DAnimation Frame { get; set; }
        public TranslationAnimation Translation { get; set; }

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

            if (updateOrder == null)
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
