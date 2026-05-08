// [비등가교환]
// GoldChangedEvent를 구독해 골드가 증가할 때마다 count에 따라 25%/45%/70% 확률로
// 총알 1발을 충전(curBulletCount++). 최대 탄약 수 초과 및 재장전 중에는 충전 불가.
public class UnequalExchangeAbility : Ability
{
    static readonly float[] chances = { 25f, 45f, 70f };

    int prevGold;

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        if (c < 1) c = 1;
        return $"골드를 획득할 때마다 {chances[c - 1]}% 확률로 총알 1발을 충전합니다.";
    }

    public override void OnEquip(Player player)
    {
        prevGold = player.gold;
        GameEventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
    }

    void OnGoldChanged(GoldChangedEvent e)
    {
        if (e.addGold < 0) { return; }

        Player player = Player.Instance;
        if (player.isReloading) return;

        int max = (int)player.statMgr.BulletCount;
        if (player.curBulletCount >= max) return;

        if (UnityEngine.Random.Range(0f, 100f) > chances[count - 1]) return;

        player.curBulletCount++;
        GameEventBus.Publish(new BulletChargedEvent(player.curBulletCount, max));
    }
}
