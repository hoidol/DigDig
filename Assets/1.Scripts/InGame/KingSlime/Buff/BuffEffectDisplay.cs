using System.Collections.Generic;
using UnityEngine;

// 받은 버프의 이펙트(Buff.effectType)를 대상에 따라다니게 표시 (Minion·KingSlime 공용)
// 버프 목록 기준으로 켜고 끔 → 버프가 빠지면 이펙트도 같이 꺼짐. 같은 종류는 하나만
public class BuffEffectDisplay
{
    readonly Dictionary<EffectType, ParticleEffect> effects = new Dictionary<EffectType, ParticleEffect>();
    readonly HashSet<EffectType> types = new HashSet<EffectType>();
    readonly List<EffectType> removed = new List<EffectType>();

    // show = false면 전부 끔 (대상이 꺼져 있을 때 등)
    public void Refresh(Transform target, List<Buff> buffs, bool show = true)
    {
        types.Clear();
        if (show && buffs != null)
            foreach (Buff buff in buffs)
                if (buff.effectType.HasValue)
                    types.Add(buff.effectType.Value);

        removed.Clear();
        foreach (var pair in effects)
            if (!types.Contains(pair.Key))
                removed.Add(pair.Key);
        foreach (EffectType type in removed)
        {
            End(target, effects[type]);
            effects.Remove(type);
        }

        if (EffectManager.Instance == null)
            return;
        foreach (EffectType type in types)
        {
            // 이미 재생 중이면 그대로 (끝나서 다른 대상이 재사용 중이면 새로)
            if (effects.TryGetValue(type, out ParticleEffect effect) && effect != null && effect.FollowTarget == target)
                continue;
            effect = EffectManager.Instance.GetEffect(type) as ParticleEffect;
            if (effect == null)
                continue;
            effect.PlayFollow(target, float.MaxValue);
            effects[type] = effect;
        }
    }

    public void Clear(Transform target)
    {
        foreach (var pair in effects)
            End(target, pair.Value);
        effects.Clear();
    }

    static void End(Transform target, ParticleEffect effect)
    {
        if (effect != null && effect.FollowTarget == target)
            effect.EndEffect();
    }
}
