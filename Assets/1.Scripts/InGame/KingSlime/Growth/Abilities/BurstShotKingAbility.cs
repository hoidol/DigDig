using Cysharp.Threading.Tasks;
using UnityEngine;

// 연속탄: 킹이 탄을 쏠 때마다 같은 탄을 같은 위치·방향으로 (countPerLevel × level)발 더 (간격 KingSlime.COMBO_ATTACK_INTERVAL_MS)
// 더 쏜 탄은 OnKingFired를 거치지 않음 → 연속탄이 다시 연속탄을 만들지 않음 (룬 연발 BurstShotTileItem과 같은 방식)
public class BurstShotKingAbility : KingAbility
{
    public int countPerLevel = 1;

    int BurstCount => countPerLevel * level;

    public override string GetValueText(int level) => (countPerLevel * level).ToString();

    public override void OnKingFired(AllyBulletObject bullet)
    {
        // 탄 객체는 풀링되어 재사용되므로 지연 전에 발사 정보를 복사해 둠
        RunBurst(bullet.spec, bullet.allyUnit, bullet.shootPos, bullet.shootDir, bullet.shootDamage, BurstCount).Forget();
    }

    async UniTaskVoid RunBurst(BulletSpec spec, IAllyUnit allyUnit, Vector2 pos, Vector2 dir, float damage, int count)
    {
        var token = this.GetCancellationTokenOnDestroy();
        for (int i = 0; i < count; i++)
        {
            if (await UniTask.Delay(KingSlime.COMBO_ATTACK_INTERVAL_MS, cancellationToken: token).SuppressCancellationThrow())
                return;
            AllyBulletObject burst = spec.Instantiate(allyUnit);
            burst.transform.position = pos;
            burst.Shoot(dir, damage);
        }
    }
}
