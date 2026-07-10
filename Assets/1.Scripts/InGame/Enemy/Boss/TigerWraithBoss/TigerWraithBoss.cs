using Cysharp.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

//두려움 상징용 Stage1 보스 - 창귀
public class TigerWraithBoss : Boss
{
    public SkeletonAnimation skeletonAnimation;
    public override void Apear()
    {
        base.Apear();
        skeletonAnimation.state.SetAnimation(0, "idle", true);

    }

    public override async UniTask StartBehaviour()
    {
        while (true)
        {
            //1번 공격 후 Sweep
            BossBehaviour bossBehaviour = GetBossBehaviour("Approching");
            await bossBehaviour.StartBehaviour();

            ExcutePatternBossBehaviour excutePattern = GetBossBehaviour("ExcutePattern") as ExcutePatternBossBehaviour;
            excutePattern.patternName = "Sweep";
            await excutePattern.StartBehaviour();

            StopBossBehaviour stop = GetBossBehaviour("Stop") as StopBossBehaviour;
            stop.stopTimeRange = new Vector2(1f,2f);
            await stop.StartBehaviour();

            bossBehaviour = GetBossBehaviour("Approching");
            await bossBehaviour.StartBehaviour();

            excutePattern = GetBossBehaviour("ExcutePattern") as ExcutePatternBossBehaviour;
            excutePattern.patternName = "Sweep";
            await excutePattern.StartBehaviour();

            stop = GetBossBehaviour("Stop") as StopBossBehaviour;
            stop.stopTimeRange = new Vector2(5f,6f);
            await stop.StartBehaviour();
        }
    }
    public override float PlayAnim(string anim)
    {
        var entry = skeletonAnimation.state.SetAnimation(0, anim, false);
        return entry.Animation.Duration;
    }


}
