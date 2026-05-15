using UnityEngine;

// 오발탄 - 확률로 랜덤 방향 추가 총알 발사
public class AimlessBulletAbility : Ability, IBulletItem
{
    static readonly float[] probs = { 0.30f, 0.35f, 0.45f, 0.50f, 0.60f };

    bool isFiring;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (isFiring) return;
        if (Random.value >= probs[count - 1]) return;

        isFiring = true;
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Player.Instance.Shoot(randomDir, Player.Instance.attackPoint.position);
        isFiring = false;
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"{probs[c - 1] * 100:0}% 확률로 랜덤 방향 오발탄 발사";
    }
}
