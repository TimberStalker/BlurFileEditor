using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;

namespace BlurFileEditor.Utils.Animations
{
    public class CornerRadiusAnimation : CornerRadiusAnimationBase
    {


        public CornerRadius? By
        {
            get { return (CornerRadius?)GetValue(ByProperty); }
            set { SetValue(ByProperty, value); }
        }

        public IEasingFunction EasingFunction
        {
            get { return (IEasingFunction)GetValue(EasingFunctionProperty); }
            set { SetValue(EasingFunctionProperty, value); }
        }

        public CornerRadius? From
        {
            get { return (CornerRadius?)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }


        public CornerRadius? To
        {
            get { return (CornerRadius?)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        // Using a DependencyProperty as the backing store for To.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(CornerRadius?), typeof(CornerRadiusAnimation), new FrameworkPropertyMetadata(null));



        public static readonly DependencyProperty ByProperty =
            DependencyProperty.Register("By", typeof(CornerRadius?), typeof(CornerRadiusAnimation), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(CornerRadiusAnimation), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(CornerRadius?), typeof(CornerRadiusAnimation), new FrameworkPropertyMetadata(null));


        public CornerRadiusAnimation()
        {

        }
        public CornerRadiusAnimation(CornerRadius toValue, Duration duration)
        {
            To = toValue;
            Duration = duration;
        }
        public CornerRadiusAnimation(CornerRadius toValue, Duration duration, FillBehavior fillBehavior) 
        {
            To = toValue;
            Duration = duration;
            FillBehavior = fillBehavior;
        }
        public CornerRadiusAnimation(CornerRadius fromValue, CornerRadius toValue, Duration duration)
        {
            From = fromValue;
            To = toValue;
            Duration = duration;
        }
        public CornerRadiusAnimation(CornerRadius fromValue, CornerRadius toValue, Duration duration, FillBehavior fillBehavior)
        {
            From = fromValue;
            To = toValue;
            Duration = duration;
            FillBehavior = fillBehavior;
        }

        public bool IsAdditive { get; set; }
        public bool IsCumulative { get; set; }
        public override bool IsDestinationDefault => true;

        public CornerRadiusAnimation Clone() => new CornerRadiusAnimation(From ?? new CornerRadius(), To ?? new CornerRadius(), Duration, FillBehavior) { IsAdditive = IsAdditive, IsCumulative = IsCumulative };
        protected override Freezable CreateInstanceCore() => new CornerRadiusAnimation();
        protected override CornerRadius GetCurrentValueCore(CornerRadius defaultOriginValue, CornerRadius defaultDestinationValue, AnimationClock animationClock)
        {
            double progress = EasingFunction?.Ease(animationClock.CurrentProgress ?? 0) ?? animationClock.CurrentProgress ?? 0;
            var from = From ?? defaultOriginValue;
            var to = To ?? defaultDestinationValue;
            var by = By ?? new CornerRadius();
            return new CornerRadius(
            (to.TopLeft - from.TopLeft + by.TopLeft) * progress + from.TopLeft,
            (to.TopRight - from.TopRight + by.TopRight) * progress + from.TopRight,
            (to.BottomLeft - from.BottomLeft + by.BottomLeft) * progress + from.BottomLeft,
            (to.BottomRight - from.BottomRight + by.BottomRight) * progress + from.BottomRight);
        }
    }
}
