using UnityEngine;

// 빙석 슬라임의 눈: 적중 처리는 MovingHitObject, 위치는 HailMinion이 경로(PathLoop)로 정해 줌
// 추가 효과: iceChance% 확률로 얼음
public class HailStone : MovingHitObject
{
    public float iceChance;     // %
    public float iceDuration = 1f;
    // (2026-11-26) 눈송이 진화(IceEvolution)용: 빙결 감속률·확률 스턴 (빙석 미니언은 기본값 = 기본 감속률, 스턴 없음)
    [System.NonSerialized] public float iceSlowRate = IceEffect.DEFAULT_SLOW_RATE;
    [System.NonSerialized] public float stunChance;   // 0~1
    [System.NonSerialized] public float stunDuration;
    public Transform spriteTr;
    [Tooltip("스프라이트 회전 속도(도/초). 회전 방향은 HailMinion 경로 순환 방향(시계/반시계)을 따름")]
    public float spinSpeed = 360f;

    // 경로 순환 방향으로 spriteTr 회전 (HailMinion.UpdateHails에서 게임 진행 중에만 호출)
    public void Spin(RotationDirection direction, float deltaTime)
    {
        if (spriteTr == null)
            return;
        float sign = direction == RotationDirection.Clockwise ? -1f : 1f; // z+ = 반시계
        spriteTr.Rotate(0f, 0f, sign * spinSpeed * deltaTime);
    }

    protected override void OnHit(IHittable hittable)
    {
        if (!(hittable is Enemy enemy) || !enemy.gameObject.activeInHierarchy)
            return;
        StatusEffectHandler handler = enemy.GetComponent<StatusEffectHandler>();
        if (handler == null)
            return;
        if (Random.Range(0f, 100f) < iceChance)
            handler.Apply(new IceEffect(iceDuration, iceSlowRate));
        if (stunChance > 0f && stunDuration > 0f && Random.value < stunChance)
            handler.Apply(new StunEffect(stunDuration));
    }
}
