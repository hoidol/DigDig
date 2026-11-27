using UnityEngine;

// 용암탄: 마지막 튕김 후 4타일 범위 용암 지대 생성
public class LavaShellBulletSpec : BulletSpec
{
    const float LAVA_RADIUS    = 4f;
    static readonly float[] LAVA_DAMAGE_RATES = { 0.4f, 0.5f, 0.6f };
    static readonly float[] LAVA_DURATIONS    = { 4f,   5f,   6f   };

    public LavaShellBulletSpec() { key = "LavaShell"; }

    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     int lv = GetLevel();
    //     bullet.AddBehavior(new LavaShellBehavior(
    //         Player.Instance.bounce,
    //         LAVA_RADIUS,
    //         LAVA_DAMAGE_RATES[lv - 1],
    //         LAVA_DURATIONS[lv - 1]
    //     ));
    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    // {
    //     return $"마지막 튕김 지점에 용암 지대 생성\n범위 {LAVA_RADIUS}타일, {LAVA_DURATIONS[lv - 1]}초간 공격력 {LAVA_DAMAGE_RATES[lv - 1] * 100:0}% 피해/0.5초";
    // }
}
