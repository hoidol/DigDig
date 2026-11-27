using UnityEngine;

public class IceBulletSpec : AllyBulletSpec
{
    public float duration;
    public float slowRate = IceEffect.DEFAULT_SLOW_RATE; // 이동속도 배율 (IceMinionData.slowRate)
    // public float chance;

    public IceBulletSpec()
    {
        key = "Ice";
    }


    public override void StartBulletSpec()
    {
        base.StartBulletSpec();
        AddBulletBehaviour(new IceOnHitBehavior(duration, 1, slowRate));
    }


    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {

    //     base.OnBulletFired(bullet);
    //     bullet.AddBehavior(new ChanceBurnBehavior(BURN_CHANCE, BURN_DURATIONS[GetLevel()-1], BURN_DPS[GetLevel()-1]));

    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    //     => $"{BURN_CHANCE * 100:0}% 확률로 화상 ({BURN_DURATIONS[lv-1]}초 {BURN_DPS[lv-1]} DPS)";

}