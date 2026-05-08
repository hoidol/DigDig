using UnityEngine;
// [손거울]
// 발사된 총알에 튕김(BounceBehavior)을 추가하는 IBulletItem.
// count만큼 튕김 횟수가 증가하며, 총알이 벽이나 적에 맞으면 반사 방향으로 계속 진행.
public class HandMirrorItem : Item, IBulletItem
{
    public int bounceCount = 1;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        Debug.Log("HandMirrorItem OnBulletFired");
        bullet.AddBehavior(new BounceBehavior(count));
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"탄 튕김 +{c}";
    }
}
