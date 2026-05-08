// [숫돌]
// 발사된 총알에 관통(PierceBehavior)을 추가하는 IBulletItem.
// count + basePierceCount만큼 관통 횟수가 설정되어, 총알이 여러 적/광석을 연속으로 통과.
public class HoneItem : Item, IBulletItem
{
    public int basePierceCount = 0;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new PierceBehavior(count + basePierceCount));
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"탄 관통력 +{c + basePierceCount}";
    }
}
