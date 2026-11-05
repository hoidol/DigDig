// [흡혈탄]
// 적 처치 시 최대 체력의 0.5% 회복
public class VampireBulletSpec : BulletSpec
{
    float[] HEAL_RATES = {0.005f,0.01f,0.015f};
    public VampireBulletSpec()
    {
        key = "Vampire";
    }

    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     base.OnBulletFired(bullet);
    //     bullet.AddBehavior(new VampireOnKillBehavior(HEAL_RATES[GetLevel()-1]));
    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    //     => $"처치 시 최대 체력의 {HEAL_RATES[GetLevel()-1] * 100:0.#}% 회복";

}
