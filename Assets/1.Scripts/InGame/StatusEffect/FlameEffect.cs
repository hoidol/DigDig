using System.Collections.Generic;
using UnityEngine;
// FlameEffect.cs
// (2026-11-26) 화상 최대 MAX_STACK(3)중첩. 걸릴 때마다 중첩 하나 (피해·남은 시간은 중첩마다 따로 흐름)
// - 중첩이 3개 미만이면 추가
// - 3개 다 찼으면 새 화상 피해가 가장 약한 중첩보다 세면 그 중첩과 교체(시간도 새로), 같거나 약하면 무시
// - 피해 = 중첩 피해 합 (0.5초마다), 중첩은 각자 시간이 끝나면 빠지고 다 빠지면 화상 해제
// 예) 10초 화상 피해 2,3,4,1,5가 1초 간격 → 3초까지 2+3+4 = 9 / 4초의 1은 무시 / 5초의 5는 2와 교체 → 3+4+5 = 12
// (이전: 하나만, 다시 걸리면 시간 갱신 + 센 피해로 교체)
public class FlameEffect : StatusEffect
{
    public const int MAX_STACK = 3;

    class Stack
    {
        public float dps;
        public float remaining;
        public IAllyUnit source;
    }

    readonly List<Stack> stacks = new List<Stack>();
    float damageTimer;
    IHittable hittable;
    public override string EffectKey => "Flame";
    readonly DamageData damageData = new DamageData();

    readonly float initDps;
    readonly IAllyUnit initSource;

    public int StackCount => stacks.Count;
    // 지금 들어가는 피해 (중첩 합)
    public float DamagePerSecond
    {
        get
        {
            float sum = 0f;
            foreach (Stack s in stacks)
                sum += s.dps;
            return sum;
        }
    }
    // 가장 센 중첩을 건 아군 (없으면 null)
    public IAllyUnit Source
    {
        get
        {
            Stack best = null;
            foreach (Stack s in stacks)
                if (best == null || s.dps > best.dps)
                    best = s;
            return best?.source;
        }
    }

    // source = 화상을 건 아군 (피해 집계용, 없으면 null)
    public FlameEffect(float duration, float dps, IAllyUnit source = null)
    {
        this.duration = duration;
        this.remainingTimer = duration;
        initDps = dps;
        initSource = source;
        damageData.attackType = AttackType.Magic;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        hittable = handler.GetComponent<IHittable>();
        if (effect is FlameEffect f)
            AddStack(f.initDps, f.duration, f.initSource);
    }

    void AddStack(float dps, float time, IAllyUnit source)
    {
        if (stacks.Count < MAX_STACK)
        {
            stacks.Add(new Stack { dps = dps, remaining = time, source = source });
        }
        else
        {
            Stack weakest = stacks[0];
            foreach (Stack s in stacks)
                if (s.dps < weakest.dps)
                    weakest = s;
            if (dps <= weakest.dps)
                return; // 가장 약한 중첩보다 약하거나 같으면 무시
            weakest.dps = dps;
            weakest.remaining = time;
            weakest.source = source;
        }
        RefreshRemaining();
    }

    // 화상 전체 남은 시간 = 가장 오래 남은 중첩 (StatusEffectHandler가 0이 되면 해제)
    void RefreshRemaining()
    {
        float max = 0f;
        foreach (Stack s in stacks)
            max = Mathf.Max(max, s.remaining);
        remainingTimer = max;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        stacks.Clear();
    }

    public override void OnUpdate(StatusEffectHandler handler)
    {
        float dt = Time.deltaTime;
        for (int i = stacks.Count - 1; i >= 0; i--)
        {
            stacks[i].remaining -= dt;
            if (stacks[i].remaining <= 0f)
                stacks.RemoveAt(i);
        }
        RefreshRemaining();
        if (stacks.Count == 0)
            return;

        if (damageTimer >= 0.5f)
        {
            damageTimer = 0f;
            DealDamage();
        }
        damageTimer += dt;
    }

    // 피해 집계가 맞게 건 아군마다 한 번씩 (같은 미니언이 건 중첩은 합쳐서 한 번)
    void DealDamage()
    {
        if (hittable == null)
            return;
        for (int i = 0; i < stacks.Count; i++)
        {
            IAllyUnit src = stacks[i].source;
            bool done = false;
            for (int j = 0; j < i; j++)
                if (stacks[j].source == src) { done = true; break; }
            if (done)
                continue;
            float sum = 0f;
            for (int j = i; j < stacks.Count; j++)
                if (stacks[j].source == src)
                    sum += stacks[j].dps;
            damageData.damage = sum;
            damageData.source = src;
            hittable.TakeDamage(damageData);
            if (hittable is MonoBehaviour mb && (mb == null || !mb.isActiveAndEnabled))
                return; // 죽었으면 그만
        }
    }

    // 테스트용 규칙 계산 (씬 없이): 순서대로 걸린 피해 → 남는 중첩 피해 목록 (시간 경과 없이)
    public static List<float> SimulateStacks(IEnumerable<float> dpsList)
    {
        var f = new FlameEffect(10f, 0f);
        foreach (float d in dpsList)
            f.AddStack(d, 10f, null);
        var result = new List<float>();
        foreach (Stack s in f.stacks)
            result.Add(s.dps);
        return result;
    }
}
