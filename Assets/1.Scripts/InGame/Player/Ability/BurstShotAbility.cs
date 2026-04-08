using UnityEngine;

// 연속샷 - 6번마다 count발 연속 추가 발사
public class BurstShotAbility : Ability
{
    int shotCount;
    const int TRIGGER_COUNT = 6;
    public override string GetDescription(int c = -1)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;

        return $"{TRIGGER_COUNT}번 탄 발사 시 다음 공격 {count}번 연속 발사";
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
        if (e.fromPlayer)
        {
            shotCount++;
            if (shotCount < TRIGGER_COUNT - (count - 1)) return;
            shotCount = 0;
            Player.Instance.QueueExtraShot(1);
        }

    }
}
