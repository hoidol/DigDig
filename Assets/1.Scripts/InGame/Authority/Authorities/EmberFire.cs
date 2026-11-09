using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 잔불의 화로: 장판 피해 지점에 남는 불길. TICK마다 범위 안 적에게 피해, 수명이 끝나면 사라짐
public class EmberFire : MonoBehaviour
{
    const float TICK = 0.5f;
    const int MAX_ACTIVE = 20; // 동시에 남는 불길 상한 (지옥불처럼 자주 터지는 장판 대비)
    static readonly List<EmberFire> actives = new List<EmberFire>();

    Vector2 pos;
    float radius;
    float damagePerTick;
    LayerMask mask;
    float lifeTimer;
    float tickTimer;
    readonly DamageData damageData = new DamageData { attackType = AttackType.Magic };

    public static int ActiveCount => actives.Count(e => e != null);
    public static int CreatedCount { get; private set; } // 이번 판에 생긴 불길 수 (테스트 확인용)

    public static EmberFire Spawn(Vector2 pos, float radius, float dps, float lifetime, LayerMask mask)
    {
        actives.RemoveAll(e => e == null);
        if (actives.Count >= MAX_ACTIVE)
        {
            Destroy(actives[0].gameObject); // 가장 오래된 불길부터 정리
            actives.RemoveAt(0);
        }
        var go = new GameObject("EmberFire");
        go.transform.position = pos;
        var fire = go.AddComponent<EmberFire>();
        fire.pos = pos;
        fire.radius = radius;
        fire.damagePerTick = dps * TICK;
        fire.mask = mask;
        fire.lifeTimer = lifetime;
        actives.Add(fire);
        CreatedCount++;
        return fire;
    }

    public static void ClearAll()
    {
        foreach (var e in actives.Where(e => e != null))
            Destroy(e.gameObject);
        actives.Clear();
        CreatedCount = 0;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying)
            return;
        lifeTimer -= Time.deltaTime;
        tickTimer += Time.deltaTime;
        if (tickTimer >= TICK)
        {
            tickTimer -= TICK;
            Tick();
        }
        if (lifeTimer <= 0f)
        {
            actives.Remove(this);
            Destroy(gameObject);
        }
    }

    void Tick()
    {
        EffectManager.Instance.Play(EffectType.Spark, pos);
        foreach (Collider2D col in Physics2D.OverlapCircleAll(pos, radius, mask))
        {
            if (col.TryGetComponent(out IHittable hit))
            {
                damageData.damage = Mathf.Max(1f, damagePerTick);
                hit.TakeDamage(damageData);
            }
        }
    }
}
