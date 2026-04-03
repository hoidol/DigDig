using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
public class StatusEffectHandler : MonoBehaviour
{
    public bool IsStunned  { get; set; }
    public bool IsShielded { get; set; }

    // 방어막 소모 시도. 막으면 true 반환
    public bool TryBlock()
    {
        if (!IsShielded) return false;

        var shield = effects.FirstOrDefault(e => e is ShieldEffect) as ShieldEffect;
        shield?.Consume(this);
        return true;
    }

    List<StatusEffect> effects = new List<StatusEffect>();
    Dictionary<string, List<StatusEffectView>> activeEffects = new();
    void Awake()
    {
        StatusEffectView[] views = GetComponentsInChildren<StatusEffectView>();
        foreach (StatusEffectView view in views)
        {
            if (!activeEffects.ContainsKey(view.effectKey))
            {
                activeEffects[view.effectKey] = new List<StatusEffectView>();
            }
            activeEffects[view.effectKey].Add(view);
        }
    }

    public void Apply(StatusEffect effect)
    {
        // 같은 타입이 이미 있으면 시간만 갱신
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

    void StopEffect(StatusEffect effect)
    {
        if (effect.EffectKey == null) return;
        if (!activeEffects.TryGetValue(effect.EffectKey, out var views)) return;

        foreach (var view in views) view.Stop();
    }
}
