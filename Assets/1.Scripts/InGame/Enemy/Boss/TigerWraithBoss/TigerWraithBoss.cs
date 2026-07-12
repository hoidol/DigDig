using Cysharp.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

//두려움 상징용 Stage1 보스 - 창귀
public class TigerWraithBoss : Boss
{
    public SkeletonAnimation skeletonAnimation;
    string[] rangeAttackPatternNames = { "RandomDrop" };//, "BulletSpray"/
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
            ApprochingBossBehaviour approching = GetBossBehaviour("Approching") as ApprochingBossBehaviour;
            approching.distance = 6;
            await approching.StartBehaviour();

            ExcutePatternBossBehaviour excutePattern = GetBossBehaviour("ExcutePattern") as ExcutePatternBossBehaviour;
            excutePattern.patternName = "Sweep";
            await excutePattern.StartBehaviour();

            StopBossBehaviour stop = GetBossBehaviour("Stop") as StopBossBehaviour;
            stop.stopTimeRange = new Vector2(1f, 2f);
            await stop.StartBehaviour();

            if (Random.Range(0, 3) == 0)
            {
                approching = GetBossBehaviour("Approching") as ApprochingBossBehaviour;
                approching.distance = 6;
                await approching.StartBehaviour();

                excutePattern = GetBossBehaviour("ExcutePattern") as ExcutePatternBossBehaviour;
                excutePattern.patternName = "Sweep";
                await excutePattern.StartBehaviour();

                stop = GetBossBehaviour("Stop") as StopBossBehaviour;
                stop.stopTimeRange = new Vector2(5f, 6f);
                await stop.StartBehaviour();
            }
            else
            {
                approching = GetBossBehaviour("Approching") as ApprochingBossBehaviour;
                approching.distance = 10;
                await approching.StartBehaviour();

                excutePattern = GetBossBehaviour("ExcutePattern") as ExcutePatternBossBehaviour;
                excutePattern.patternName = rangeAttackPatternNames[Random.Range(0, rangeAttackPatternNames.Length)];
                await excutePattern.StartBehaviour();

                stop = GetBossBehaviour("Stop") as StopBossBehaviour;
                stop.stopTimeRange = new Vector2(5f, 6f);
                await stop.StartBehaviour();
            }
        }
    }
    public override float PlayAnim(string anim)
    {
        var entry = skeletonAnimation.state.SetAnimation(0, anim, false);
        return entry.Animation.Duration;
    }


}
