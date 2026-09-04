using UnityEngine;

public class IceBulletSpec : BulletSpec
{
    public float duration;

    public IceBulletSpec()
    {
        key = "Ice";
    }

    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {

    //     base.OnBulletFired(bullet);
    //     bullet.AddBehavior(new ChanceBurnBehavior(BURN_CHANCE, BURN_DURATIONS[GetLevel()-1], BURN_DPS[GetLevel()-1]));

    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    //     => $"{BURN_CHANCE * 100:0}% 확률로 화상 ({BURN_DURATIONS[lv-1]}초 {BURN_DPS[lv-1]} DPS)";

}