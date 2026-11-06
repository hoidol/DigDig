using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

// 지뢰 슬라임이 던지는 지뢰 (탄 아님: BulletSpec/타일 OnFired를 거치지 않음)
// MineSlime 위치에서 포물선으로 날아가 착지 → 적이 TRIGGER_RANGE 안에 들어오면 폭발
// 폭발 범위 / 최대 피해 적 수 / 유지 시간은 MineSlime이 던질 때 넘김. 트리거 범위는 고정
public class Mine : MonoBehaviour
{
    public const float TRIGGER_RANGE = 0.4f;
    const float THROW_DURATION = 0.4f; // 던지는 시간
    const float THROW_HEIGHT = 1f;     // 포물선 최고 높이

    // 지뢰는 던진 슬라임이 사라져도 남으므로 슬라임이 아니라 여기서 풀링
    static readonly Stack<Mine> pool = new Stack<Mine>();

    public IAllyUnit Owner { get; private set; }
    public bool IsArmed { get; private set; }

    float explodeRadius;
    int maxHitCount;
    float lifeTimer;
    LayerMask hitLayerMask;
    Vector2 landPos;
    readonly AllyUnitDamageData damageData = new AllyUnitDamageData();

    public static Mine Spawn(Mine prefab)
    {
        while (pool.Count > 0)
        {
            Mine pooled = pool.Pop();
            if (pooled != null) // 씬 전환으로 파괴된 것은 건너뜀
            {
                pooled.gameObject.SetActive(true);
                return pooled;
            }
        }
        return Instantiate(prefab);
    }

    // 피해량·치명타 등은 던지는 순간의 슬라임 스탯으로 고정
    public void Throw(IAllyUnit owner, Vector2 from, Vector2 to, float damage, float explodeRadius, int maxHitCount, float lifetime, LayerMask hitLayerMask)
    {
        Owner = owner;
        this.explodeRadius = explodeRadius;
        this.maxHitCount = maxHitCount;
        this.lifeTimer = lifetime;
        this.hitLayerMask = hitLayerMask;
        landPos = to;

        damageData.Init(owner);
        damageData.damage = damage;
        damageData.attackType = owner.AttackType;

        IsArmed = false;
        transform.position = from;
        transform.DOKill();
        transform.DOJump(to, THROW_HEIGHT, 1, THROW_DURATION)
            .SetEase(Ease.Linear)
            .OnComplete(() => IsArmed = true);
    }

    void Update()
    {
        if (!IsArmed)
            return;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Explode();
            return;
        }

        if (Physics2D.OverlapCircle(landPos, TRIGGER_RANGE, hitLayerMask) != null)
            Explode();
    }

    void Explode()
    {
        IsArmed = false;
        EffectManager.Instance.Play(EffectType.SmallExplosion, landPos);

        IHittable[] targets = Physics2D.OverlapCircleAll(landPos, explodeRadius, hitLayerMask)
            .Select(c => c.GetComponent<IHittable>())
            .Where(h => h != null)
            .OrderBy(h => Vector2.Distance(landPos, h.Transform.position))
            .Take(maxHitCount)
            .ToArray();

        float damage = damageData.damage;
        foreach (IHittable hit in targets)
        {
            damageData.damage = damage; // ApplyDamage 쪽에서 바뀌지 않도록 매번 원래 값
            Owner.AccumulateDamage(damage);
            hit.TakeDamage(damageData);
        }

        Release();
    }

    void Release()
    {
        transform.DOKill();
        IsArmed = false;
        Owner = null;
        gameObject.SetActive(false);
        pool.Push(this);
    }
}
