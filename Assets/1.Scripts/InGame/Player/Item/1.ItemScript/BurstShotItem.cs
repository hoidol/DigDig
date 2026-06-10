using UnityEngine;

// 연속샷 - 6번마다 1발 연속 추가 발사
public class BurstShotItem : Item, IBullet
{
    int shotCount;
    const int TRIGGER_COUNT = 10;

    public override string GetDescription(bool detail = false)
    {
        return $"{TRIGGER_COUNT}번 탄 발사 시 다음 공격 1번 연속 발사";
    }

      public void OnBulletFired(PlayerBulletObject bullet)
    {
        //if (!e.fromPlayer) return;
        shotCount++;
        if (shotCount < TRIGGER_COUNT) return;
        shotCount = 0;
        // Player.Instance.QueueExtraShot(1);
    }
}
