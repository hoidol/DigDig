using UnityEngine;
// [손거울]
// 발사된 총알에 튕김(BounceBehavior)을 추가하는 IBullet.
// count만큼 튕김 횟수가 증가하며, 총알이 벽이나 적에 맞으면 반사 방향으로 계속 진행.
public class HandMirrorItem : Item, IBullet
{
    int bounceCount = 3;

    public override void OnEquip(Player player)
    {
        Player.Instance.AddBounce(bounceCount);
    }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBulletObject bullet)
    {
        Debug.Log("HandMirrorItem OnBulletFired");
        
    }

    public override string GetDescription(bool detail = false)
    {
        return $"탄 튕김 +";
    }
}
