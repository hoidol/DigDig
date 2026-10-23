// 교묘한 교환 - 골드를 얻을 때 30% 확률로 총알 1발 충전
public class UnequalExchangeAbility : Ability
{
    const float CHANCE = 30f;

    public override string GetDescription(bool detail = false)
    {
        return $"골드를 획득할 때마다 {CHANCE}% 확률로 총알 1발 충전";
    }

    public override void OnEquip(Character player)
    {
        // GameEventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
    }

    public override void OnUnequip(Character player)
    {
        // GameEventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
    }

    // void OnGoldChanged(GoldChangedEvent e)
    // {
    //     if (e.addGold < 0) return;

    //     Player player = Player.Instance;
    //     // if (player.isReloading) return;

    //     // int max = (int)player.statMgr.BulletCount;
    //     // if (player.curBulletCount >= max) return;

    //     if (UnityEngine.Random.Range(0f, 100f) > CHANCE) return;

    //     // player.weapon.AddBullet();
    //     // GameEventBus.Publish(new BulletChargedEvent(player.curBulletCount, max));
    // }
}
