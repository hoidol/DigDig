// 사냥꾼의 표식 - 명중 시 표식 부여, 표식 대상에게 추가 피해
public class HunterMarkAbility : Ability, IBullet
{
    static readonly float bonusRatio = 0.3f;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBulletObject bullet)
    {
        var force = new HunterMarkForce(bonusRatio);
        bullet.AddBulletForce(force);
    }
}
