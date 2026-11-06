using UnityEngine;

// 빙석 슬라임의 눈: 적중 처리는 MovingHitObject, 위치는 HailSlime이 경로(PathLoop)로 정해 줌
// 추가 효과: iceChance% 확률로 얼음
public class HailStone : MovingHitObject
{
    public float iceChance;     // %
    public float iceDuration = 1f;

    protected override void OnHit(IHittable hittable)
    {
        if (Random.Range(0f, 100f) >= iceChance)
            return;
        if (hittable is Enemy enemy && enemy.gameObject.activeInHierarchy)
            enemy.GetComponent<StatusEffectHandler>()?.Apply(new IceEffect(iceDuration));
    }
}
