using Cysharp.Threading.Tasks;
using UnityEngine;

// 연속샷 - 10번마다 1발 연속 추가 발사
public class BurstShotItem : Item, IBullet
{
    int shotCount;
    int[] TRIGGER_COUNTS = {10,9,8};

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{TRIGGER_COUNTS[lv-1]}번 탄 발사 시 다음 공격 1번 연속 발사";
    }
    string bulletKey;
    public void OnBulletFired(CharacterBulletObject bullet)
    {
        //if (!e.fromPlayer) return;
        shotCount++;
        if (shotCount < TRIGGER_COUNTS[count-1]) return;
        shotCount = 0;
        bulletKey = bullet.key;
        // Player.Instance.QueueExtraShot(1);
        //bullet.key 발사 더 발사
        //BaseGun.COMBO_ATTACK_INTERVAL_MS 초 후 
        Shoot().Forget();
    }
    async UniTaskVoid Shoot()
    {
        await UniTask.Delay(BaseGun.COMBO_ATTACK_INTERVAL_MS);
        // Player.Instance.weapon.Shoot(BulletManager.bullets[bulletKey], Vector2.zero, Vector2.zero);

    }


}
