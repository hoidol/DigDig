using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
public class StatusEffectHandler : MonoBehaviour
{
    [field: SerializeField] public bool IsStunned { get; set; }
    [field: SerializeField] public bool IsShielded { get; set; }
    public const int MAX_SLOW_STACK = 3;
    const float SLOW_RATE_PER_STACK = 0.7f;
    [field: SerializeField] public int SlowStack { get; set; } // 슬로우 + 빙결 중첩 합계
    [field: SerializeField] public float MudSlow { get; set; } // 진흙 감속 비율 (MudEffect, 0~0.8)
    // 이동속도 배율 = 슬로우·빙결 중첩 × 진흙 감속
    public float SlowRate => Mathf.Pow(SLOW_RATE_PER_STACK, Mathf.Min(SlowStack, MAX_SLOW_STACK)) * (1f - MudSlow);
    [field: SerializeField] public int StigmaStack { get; set; }               // 낙인 중첩 (StigmaEffect)
    [field: SerializeField] public float StigmaDamageMul { get; set; } = 1f;   // 낙인으로 받는 피해 배율 (Enemy.TakeDamage)

    // 방어막 소모 시도. 막으면 true 반환
    public bool TryBlock()
    {
        if (!IsShielded) return false;

        var shield = effects.FirstOrDefault(e => e is ShieldEffect) as ShieldEffect;
        shield?.Consume(this);
        return true;
    }
    public bool OnDebuff() //디버프(화염 독 스턴 등) 적용 중이면
    {
        return effects.Count > 0;
    }

    public bool HasEffect(string effectKey) // 특정 이펙트 보유 여부
    {
        for (int i = 0; i < effects.Count; i++)
            if (effects[i].EffectKey == effectKey) return true;
        return false;
    }
    [SerializeField] List<StatusEffect> effects = new List<StatusEffect>();
    public IReadOnlyList<StatusEffect> Effects => effects; // 권능 전염의 성배
    Dictionary<string, List<StatusEffectView>> activeEffects = new();
    public void Init()
    {
        // Debug.Log("StatusEffectHandler Init()1");
        if (activeEffects.Count <= 0)
        {
            StatusEffectView[] views = GetComponentsInChildren<StatusEffectView>();

            // Debug.Log($"StatusEffectHandler Init() {views.Length}");
            foreach (StatusEffectView view in views)
            {
                if (!activeEffects.ContainsKey(view.effectKey))
                {
                    activeEffects[view.effectKey] = new List<StatusEffectView>();
                }
                activeEffects[view.effectKey].Add(view);
                view.gameObject.SetActive(false);
            }
        }
        Reset();
    }

    // 적용 중인 상태이상 전부 해제 (풀링 재소환 시 이전 상태이상이 남지 않도록)
    public void Reset()
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            effects[i].OnRemove(this);
            StopEffect(effects[i]);   // 파티클 해제
        }
        effects.Clear();

        IsStunned = false;
        IsShielded = false;
        SlowStack = 0;
        MudSlow = 0f;
        StigmaStack = 0;
        StigmaDamageMul = 1f;
    }

    // 엘리트/보스는 스턴 시간 절반 (모든 스턴 공통)
    public const float BIG_ENEMY_STUN_DURATION_MUL = 0.5f;

    public void Apply(StatusEffect effect)
    {
        if (effect is StunEffect && TryGetComponent(out Enemy enemy) && enemy.IsEliteOrBoss)
        {
            effect.duration *= BIG_ENEMY_STUN_DURATION_MUL;
            effect.remainingTimer = effect.duration;
        }

        // 같은 타입이 이미 있으면 시간만 갱신
        // Debug.Log($"{effect.EffectKey} 적용 대상{gameObject.name}");
        var existing = effects.FirstOrDefault(e => e.GetType() == effect.GetType());
        if (existing != null)
        {
            existing.OnApply(effect, this);
            return;
        }


        effects.Add(effect);
        effect.OnApply(effect, this);

        PlayEffect(effect);      // 파티클 시작
    }

    void PlayEffect(StatusEffect effect)
    {
        if (effect.EffectKey == null) return;

        // EffectManager에서 파티클 가져와서 대상에 붙이기
        if (!activeEffects.TryGetValue(effect.EffectKey, out var views)) return;

        foreach (var view in views) view.Play();
    }

    void Update()
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            effects[i].remainingTimer -= Time.deltaTime;
            effects[i].OnUpdate(this);

            if (effects[i].IsExpired)
            {
                effects[i].OnRemove(this);
                StopEffect(effects[i]);   // 파티클 해제
                effects.RemoveAt(i);
            }
        }
    }

    public void StopEffect(StatusEffect effect)
    {
        if (effect.EffectKey == null) return;
        if (!activeEffects.TryGetValue(effect.EffectKey, out var views)) return;

        foreach (var view in views) view.Stop();
    }
}
