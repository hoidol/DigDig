using Spine.Unity;
using UnityEngine;

//두려움 상징용 Stage1 보스 - 창귀
public class TigerWraithBoss : Boss
{
    public SkeletonAnimation skeletonAnimation;


    public override float PlayAnim(string anim)
    {
        var entry = skeletonAnimation.state.SetAnimation(0, anim, false);
        return entry.Animation.Duration;
    }

}
