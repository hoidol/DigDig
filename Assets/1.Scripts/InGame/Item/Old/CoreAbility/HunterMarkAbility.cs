// 사냥꾼의 표식 - 명중 시 표식 부여, 표식 대상에게 추가 피해
public class HunterMarkAbility : Ability, IBullet
{
    static readonly float bonusRatio = 0.3f;

    public override void OnEquip(Character player) { }
    public override void OnUnequip(Character player) { }

    public void OnBulletFired(CharacterBulletObject bullet)
    {
        var force = new HunterMarkForce(bonusRatio);
        bullet.AddBulletForce(force);
    }
}
