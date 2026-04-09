using UnityEngine;

public class MultiShotItem : Item, IAttackItem
{
    public float spacing = 0.4f; // 총알 간격

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnAttack(Player player, Vector2 dir)
    {
        // 발사 방향의 수직 벡터
        Vector2 perp = new(-dir.y, dir.x);

        // count개를 중앙 기준으로 균등 배치
        // count=1 → offset 0
        // count=2 → -0.5, +0.5
        // count=3 → -1, 0, +1
        float start = -(count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = perp * (spacing * (start + i));

            Player.Instance.Shoot(dir, (Vector2)player.attackPoint.position + offset);
        }
    }
}
