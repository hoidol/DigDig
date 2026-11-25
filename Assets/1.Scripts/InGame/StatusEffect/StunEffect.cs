// StunEffect.cs
// (2026-11-25) 이미 스턴 중에 다시 걸리면 남은 시간은 긴 쪽으로 (이전엔 처음 시간 그대로라 강한 스턴이 묻혔음)
//              fullOnBigEnemy = 엘리트·보스도 시간 절반 없이 (사령술사 저주 스킬)
[System.Serializable]
public class StunEffect : StatusEffect
{
    public override string EffectKey => "Stun";
    public bool fullOnBigEnemy;

    public StunEffect(float duration, bool fullOnBigEnemy = false)
    {
        this.duration = duration;
        this.remainingTimer = duration;
        this.fullOnBigEnemy = fullOnBigEnemy;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect != this && effect.remainingTimer > remainingTimer)
        {
            duration = effect.duration;
            remainingTimer = effect.remainingTimer;
        }
        handler.IsStunned = true;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.IsStunned = false;
    }
}
