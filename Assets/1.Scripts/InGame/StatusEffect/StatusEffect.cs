[System.Serializable]
public abstract class StatusEffect
{
    public float duration;        // 지속 시간
    public float remainingTimer;   // 남은 시간

    public virtual string EffectKey => null; // 파티클 키, 없으면 null
    public abstract void OnApply(StatusEffect effect, StatusEffectHandler handler);
    public abstract void OnRemove(StatusEffectHandler handler);
    public virtual void OnUpdate(StatusEffectHandler handler) { }

    public bool IsExpired => remainingTimer <= 0;
}