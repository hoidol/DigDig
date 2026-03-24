using UnityEngine;

public class MultiShot : Item, IAttackItem
{
    public int extraCount = 2;
    public float spreadAngle = 15f;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnAttack(Player player, Vector2 dir)
    {
        for (int i = 1; i <= extraCount; i++)
        {
            float angle = spreadAngle * i;
            FireAt(player, Rotate(dir, angle));
            FireAt(player, Rotate(dir, -angle));
        }
    }
    void FireAt(Player player, Vector2 dir)
    {
        var bullet = PlayerBullet.Instantiate();
        bullet.transform.position = player.attackPoint.position;
        bullet.Shoot(dir, player.playerStatMgr.AttackPower);
    }
    Vector2 Rotate(Vector2 v, float deg) =>
        Quaternion.Euler(0, 0, deg) * v;
}
