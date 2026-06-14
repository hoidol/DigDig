// [흡혈탄]
// 적 처치 시 최대 체력의 0.5% 회복
public class VampireBullet : Bullet
{
    const float HEAL_RATE = 0.005f;
    public VampireBullet()
    {
        key = "Vampire";
    }

    public override void OnBulletFired(PlayerBulletObject bullet)
    {
        base.OnBulletFired(bullet);
        bullet.AddBehavior(new VampireOnKillBehavior(HEAL_RATE));
    }

    public override string GetDescription(bool detail = false)
        => $"처치 시 최대 체력의 {HEAL_RATE * 100:0.#}% 회복";

}
