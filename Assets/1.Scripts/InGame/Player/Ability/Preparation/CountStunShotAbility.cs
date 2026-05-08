// 5번째 발사 총알은 스턴 (보스 제외)
using UnityEngine;

public class CountStunShotAbility : Ability, IAttackItem, IBulletItem
{
    int shotCount;
    bool applyNext;
    int TriggerCount(int c) => 7 - c;
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;

        return $"{TriggerCount(c)}번 탄 발사 시 다음 공격 스턴";
    }

    public override void OnUnequip(Player player)
    {
        shotCount = 0;
        applyNext = false;
    }

    public void OnAttack(Player player, Vector2 dir)
    {
        shotCount++;
        if (shotCount < TriggerCount(count)) return; //7 - count
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
