using UnityEngine;

// 산탄 연사 - 일반 공격 시 확률로 방사형(360도) 탄 발사
public class ScatterVolleyAbility : SynergyAbility, IAttackItem
{
    static readonly float prob = 0.3f;
    static readonly int bulletCount = 5;
    static readonly float ratio = 0.3f;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnAttack(Player player, Vector2 dir)
    {
        if (Random.value > prob) return;

        float dmg = player.statMgr.AttackPower * ratio;
        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float rad = i * angleStep * Mathf.Deg2Rad;
            Vector2 bulletDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            var bullet = Player.Instance.Shoot(bulletDir, Vector2.zero);
            bullet.damageData.damage = dmg;
        }
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"공격 시 {prob * 100:0}% 확률로 {bulletCount}방향 방사형 탄 발사 (공격력 {ratio * 100:0}%)";
    }
}
