using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;

namespace BlurFileEditor.Utils.Animations;

public abstract class CornerRadiusAnimationBase : AnimationTimeline
{
    public sealed override Type TargetPropertyType => typeof(CornerRadius);

    protected CornerRadiusAnimationBase()
    {

    }


    public ThicknessAnimationBase Clone() => Clone();
    public sealed override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock) => GetCurrentValue((CornerRadius)defaultOriginValue, (CornerRadius)defaultDestinationValue, animationClock);
    public CornerRadius GetCurrentValue(CornerRadius defaultOriginValue, CornerRadius defaultDestinationValue, AnimationClock animationClock) => GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
    protected abstract CornerRadius GetCurrentValueCore(CornerRadius defaultOriginValue, CornerRadius defaultDestinationValue, AnimationClock animationClock);
}
