using UnityEngine;


// 폭발 시체 제거 - 내 총알로 처치한 적 위치에서 범위 폭발
public class KillExplosionAbility : Ability, IBulletItem
{
    static readonly float[] radii = { 1.4f, 1.7f, 2f };
    static readonly float[] ratios = { 0.4f, 0.6f, 0.8f }; // 공격력 대비 비율
    public override string GetDescription(int c = -1)
    {
        return $"탄으로 적 처치 시 폭발(강화 시 폭발력 강화)";
    }
    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        float dmg = Player.Instance.statMgr.AttackPower * ratios[count - 1];
        // Debug.Log($"KillExplosionAbility OnBulletFired() {dmg}");
        bullet.AddBehavior(new KillExplosionBehavior(
            radii[count - 1], dmg, LayerMask.GetMask("Hittable")));
    }
}
