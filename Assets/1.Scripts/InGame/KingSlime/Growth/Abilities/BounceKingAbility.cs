// 바운드: 킹 탄이 적에게 맞으면 (countPerLevel × level)번 다음 적으로 튕김 (BounceBehavior, 피해 감소 없음)
public class BounceKingAbility : KingAbility
{
    public int countPerLevel = 1;
    public float damageMulPerBounce = 1f;

    public override string GetValueText(int level) => (countPerLevel * level).ToString();

    public override void OnKingFired(AllyBulletObject bullet)
    {
        bullet.AddBehavior(new BounceBehavior(countPerLevel * level, damageMulPerBounce));
    }
}
