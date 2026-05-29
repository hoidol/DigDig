// 6번째 발사 총알은 스턴 (보스 제외)
using UnityEngine;

public class CountStunShotAbility : Ability, IAttackItem, IBulletItem
{
    int shotCount;
    bool applyNext;
    const int TRIGGER_COUNT = 6;

    public override string GetDescription(bool detail = false)
    {
        return $"{TRIGGER_COUNT}번 탄 발사 시 다음 공격 스턴";
    }

    public override void OnUnequip(Player player)
    {
        shotCount = 0;
        applyNext = false;
    }

    public void OnAttack(Player player, Vector2 dir)
    {
        shotCount++;
        if (shotCount < TRIGGER_COUNT) return;
        shotCount = 0;
        applyNext = true;
    }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (!applyNext) return;
        applyNext = false;
        bullet.AddBehavior(new StunOnHitBehavior(2));
    }
}
