using UnityEngine;

public class StatusEffectView : MonoBehaviour
{
    public string effectKey;

    // 적 종류별 크기 배율 (프리팹 크기 = 일반 적 기준). 엘리트(보너스 엘리트 포함) / 보스는 이 배율만큼 키움
    [Header("적 종류별 크기 배율 (일반 = 1)")]
    public float eliteScale = 1f;
    public float bossScale = 1f;

    Vector3 baseScale;
    bool baseScaleSaved;

    public virtual void Play()
    {
        ApplyEnemyTypeScale();
        gameObject.SetActive(true);
    }
    public virtual void Stop()
    {
        gameObject.SetActive(false);
    }

    void ApplyEnemyTypeScale()
    {
        if (!baseScaleSaved)
        {
            baseScale = transform.localScale;
            baseScaleSaved = true;
        }
        Enemy enemy = GetComponentInParent<Enemy>(true);
        float mul = 1f;
        if (enemy != null)
        {
            switch (enemy.enemyType)
            {
                case EnemyType.Elite:
                case EnemyType.BonusElite:
                    mul = eliteScale;
                    break;
                case EnemyType.Boss:
                    mul = bossScale;
                    break;
            }
        }
        transform.localScale = baseScale * mul;
    }
}
