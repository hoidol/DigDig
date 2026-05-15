using UnityEngine;
using Cysharp.Threading.Tasks;

// 긴급 호출 - 마지막 탄 발사 시 360도 방향으로 방사형 탄 발사
public class EmergencyFireAbility : Ability, IAttackItem
{
    int[] bulletCouns = { 3, 4, 5, 6, 7 };

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1) c = count;
        if (c <= 0) c = 1;
        int idx = Mathf.Clamp(c - 1, 0, bulletCouns.Length - 1);
        return $"마지막 탄 발사 시 주변으로 {bulletCouns[idx]}탄 발사";
    }

    public void OnAttack(Player player, Vector2 dir)
    {
        if (player.isReloading) return;
        if (player.curBulletCount != 1) return;

        FireRadial(player, dir).Forget();
    }

    async UniTaskVoid FireRadial(Player player, Vector2 dir)
    {
        int bulletCount = bulletCouns[Mathf.Clamp(count - 1, 0, bulletCouns.Length - 1)];
        float angleStep = 360f / bulletCount;
        float baseAngle = Vector2.SignedAngle(Vector2.right, dir);

        for (int i = 0; i < bulletCount; i++)
        {
            float rad = (baseAngle + angleStep * i) * Mathf.Deg2Rad;
            Vector2 shootDir = new(Mathf.Cos(rad), Mathf.Sin(rad));
            player.weapon.Shoot(shootDir, Vector2.zero);
            await UniTask.Delay(30);
        }
    }

    public override void OnUnequip(Player player) { }
}
