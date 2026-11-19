// 빙결 (2026-11-19 슬로우 중첩과 분리): 이동속도 × slowRate (0.3 = 70% 감속), 중첩 없음
// 이미 걸려 있을 때 다시 걸리면: 배율은 더 강한 쪽(작은 값) 유지, 시간은 남은 시간보다 새 시간이 길면 새 시간으로
// 예) 0.3 3초 → 2초 후 0.7 3초 = 0.3 유지 + 남은 시간 3초로 갱신
public class IceEffect : StatusEffect
{
    public const float DEFAULT_SLOW_RATE = 0.7f; // 배율을 따로 안 주는 빙결 (빙석·얼음 구슬)

    public override string EffectKey => "Ice";
    public float slowRate;

    public IceEffect(float duration, float slowRate = DEFAULT_SLOW_RATE)
    {
        this.duration = duration;
        this.remainingTimer = duration;
        this.slowRate = slowRate;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect != this && effect is IceEffect ice)
        {
            slowRate = Merge(slowRate, ice.slowRate);
            remainingTimer = MergeRemain(remainingTimer, ice.duration);
            duration = UnityEngine.Mathf.Max(duration, ice.duration);
        }
        handler.IceSlowRate = slowRate;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.IceSlowRate = 1f;
    }

    // 규칙 (테스트용 static)
    public static float Merge(float curRate, float newRate) => UnityEngine.Mathf.Min(curRate, newRate);
    public static float MergeRemain(float curRemain, float newDuration) => UnityEngine.Mathf.Max(curRemain, newDuration);
}
