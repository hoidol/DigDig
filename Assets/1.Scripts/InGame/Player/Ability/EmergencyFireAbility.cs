using UnityEngine;

// 긴급 호출 - 장전 직전(마지막 탄) 4방향으로 탄 발사
public class EmergencyFireAbility : Ability, IAttackItem
{
    public override string GetDescription(int c = -1)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;

        return $"마지막 탄 발사 시 주변으로 탄 발사";
    }
    public void OnAttack(Player player, Vector2 dir)
    {
        if (player.curBulletCount != 1) return;

        Vector2 left = new(-dir.y, dir.x);   // dir 기준 왼쪽 (90° CCW)
        Vector2 right = new(dir.y, -dir.x);   // dir 기준 오른쪽 (90° CW)
        Vector2 back = -dir;                   // dir 기준 뒤쪽
        Vector2 frontLeft = ((Vector2)(Quaternion.Euler(0, 0, 45) * dir)).normalized; // 왼쪽앞 대각선
        Vector2 frontRight = ((Vector2)(Quaternion.Euler(0, 0, -45) * dir)).normalized; // 오른쪽앞 대각선

        Vector2[] dirs;
        if (count == 1)
            dirs = new[] { left, right };
        else if (count == 2)
            dirs = new[] { left, right, back };
        else
            dirs = new[] { left, right, back, frontLeft, frontRight };

        foreach (var d in dirs)
        {
            Player.Instance.Shoot(d, Vector2.zero);
        }
    }

    public override void OnUnequip(Player player) { }
}
