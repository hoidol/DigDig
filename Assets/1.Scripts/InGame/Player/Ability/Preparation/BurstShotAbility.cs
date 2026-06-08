using UnityEngine;

// 연속샷 - 6번마다 1발 연속 추가 발사
public class BurstShotAbility : Ability
{
    int shotCount;
    const int TRIGGER_COUNT = 6;

    public override string GetDescription(bool detail = false)
    {
        return $"{TRIGGER_COUNT}번 탄 발사 시 다음 공격 1번 연속 발사";
    }

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<BulletFiredEvent>(OnBulletFired);
    }

    public override void OnUnequip(Player player)
    {
        shotCount = 0;
        GameEventBus.Unsubscribe<BulletFiredEvent>(OnBulletFired);
    }

    void OnBulletFired(BulletFiredEvent e)
    {
        //if (!e.fromPlayer) return;
        shotCount++;
        if (shotCount < TRIGGER_COUNT) return;
        shotCount = 0;
        // Player.Instance.QueueExtraShot(1);
    }
}
