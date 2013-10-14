using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnDCS_Client.Shared.Animations
{
    public class ConditionalAnimation<T> where T : BaseAnimation
    {
        public T TrueAnimation { get; set; }
        public T FalseAnimation { get; set; }

        public Func<bool> Condition { get; set; }

        public T Animation
        {
            get
            {
                if (TrueAnimation == null)
                    throw new InvalidOperationException("True Animation has not been set.");
                if (FalseAnimation == null)
                    throw new InvalidOperationException("False Animation has not been set.");
                if (Condition == null)
                    throw new InvalidOperationException("Condition has not been set.");

                return (Condition()) ? TrueAnimation : FalseAnimation;
            }
        }

        public ConditionalAnimation()
        {
        }

        public ConditionalAnimation(T trueAnimation, T falseAnimation, Func<bool> condition)
        {
            if (trueAnimation == null)
                throw new ArgumentNullException("trueAnimation");
            if (falseAnimation == null)
                throw new ArgumentNullException("falseAnimation");
            if (condition == null)
                throw new ArgumentNullException("condition");

            TrueAnimation = trueAnimation;
            FalseAnimation = falseAnimation;
            Condition = condition;
        }

    }
}
