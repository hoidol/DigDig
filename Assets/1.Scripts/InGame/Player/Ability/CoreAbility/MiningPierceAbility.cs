// 광석 채굴 후 관통탄 - 광석 파괴 후 다음 총알 count번 관통
using UnityEngine;

public class MiningPierceAbility : Ability, IBulletItem
{
    bool applyNext;
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;
        return $"광석 제거 후 다음 탄 관통력 +{c}";
    }
    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
        applyNext = false;
    }

    void OnOreDestroyed(OreStoneDestroyedEvent e) => applyNext = true;

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (!applyNext) return;
        applyNext = false;
        Debug.Log("MiningPierceAbility Added");
        bullet.AddBehavior(new PierceBehavior(count + 1));

    }
}
