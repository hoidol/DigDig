using Cysharp.Threading.Tasks;
using UnityEngine;

// 연발: 탄을 쏠 때마다 같은 탄을 같은 위치/방향으로 burstCount번 더 발사 (간격 COMBO_ATTACK_INTERVAL_MS)
public class BurstShotTileItem : TileItem //타일 위에 설치되는 아이템
{
    public int burstCount = 1;

    public override void OnFired(AllyBulletObject bullet)
    {
        base.OnFired(bullet);
        // 탄 객체는 풀링되어 재사용되므로 지연 전에 발사 정보를 복사해 둠
        RunBurst(bullet.spec, bullet.allyUnit, bullet.shootPos, bullet.shootDir, bullet.shootDamage).Forget();
    }

    // 추가 탄은 OnFired를 거치지 않으므로 연발이 다시 연발을 만들지 않음
    async UniTaskVoid RunBurst(BulletSpec spec, IAllyUnit allyUnit, Vector2 pos, Vector2 dir, float damage)
    {
        var token = this.GetCancellationTokenOnDestroy();
        for (int i = 0; i < burstCount; i++)
        {
            if (await UniTask.Delay(Character.COMBO_ATTACK_INTERVAL_MS, cancellationToken: token).SuppressCancellationThrow())
                return;
            if (allyUnit is Object unit && unit == null) // 쏜 유닛이 합성 등으로 사라짐
                return;

            AllyBulletObject burst = spec.Instantiate(allyUnit);
            burst.transform.position = pos;
            burst.Shoot(dir, damage);
        }
    }
}
