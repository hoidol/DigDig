// 도탄 - 광석 또는 적이 죽으면 다음 발사 총알에 count번 튕김 부여
public class BoundShootAbility : Ability, IBulletItem
{
    bool applyNext;
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;

        return $"광석 제거 시 다음 탄이 {c}번 튕김";
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
        bullet.AddBehavior(new BounceBehavior(count));
    }
}
