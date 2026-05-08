// 사냥꾼의 표식 - 명중 시 표식 부여, 표식 대상에게 추가 피해
public class HunterMarkAbility : Ability, IBulletItem
{
    static readonly float[] bonusRatios = { 0.2f, 0.3f, 0.4f };

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        var force = new HunterMarkForce(bonusRatios[count - 1]);
        bullet.AddBulletForce(force);
    }
}
