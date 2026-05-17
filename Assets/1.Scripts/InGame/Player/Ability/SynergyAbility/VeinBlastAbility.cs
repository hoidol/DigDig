using UnityEngine;

// 광맥 폭발 - 광석 파괴 시 해당 위치 주변 범위 피해
public class VeinBlastAbility : SynergyAbility
{
    static readonly float radii = 1.5f;

    LayerMask enemyLayer;

    public override void OnEquip(Player player)
    {
        enemyLayer = LayerMask.GetMask("Hittable");
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnOreDestroyed);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnOreDestroyed);
    }

    void OnOreDestroyed(DestroyedStoneEvent e)
    {
        if (e.lastDamage.cause == null)
            return;

        if (e.lastDamage.cause.TryGetComponent<PlayerBullet>(out var bullet))
        {
            // Debug.Log("VeinBlastAbility OnOreDestroyed");
            float dmg = Player.Instance.statMgr.AttackPower; //* ratios[count - 1];
            AOEUtil.DamageEnemies(e.oreStone.transform.position, radii, dmg, enemyLayer);
            EffectManager.Instance.Play(EffectType.SmallExplosion, e.oreStone.transform.position);
        }


    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"광석 파괴 시 반경 {radii}m 폭발, 공격력 {100}% 피해";
    }
}
