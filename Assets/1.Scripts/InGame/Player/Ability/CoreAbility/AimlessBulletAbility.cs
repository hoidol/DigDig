using UnityEngine;

// 오발탄 - 40% 확률로 랜덤 방향 추가 총알 발사
public class AimlessBulletAbility : Ability, IBulletItem
{
    const float PROB = 0.40f;

    bool isFiring;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public override string GetDescription(bool detail = false)
    {
        return $"{PROB * 100:0}% 확률로 오발탄 추가 발사";
    }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (isFiring) return;
        if (Random.value >= PROB) return;

        isFiring = true;
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Player.Instance.Shoot(randomDir, Player.Instance.attackPoint.position);
        isFiring = false;
    }
}
