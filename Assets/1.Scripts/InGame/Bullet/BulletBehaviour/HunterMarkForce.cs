using UnityEngine;

// 표식이 있는 적에게 추가 피해 + 표식 적용
public class HunterMarkForce : IBulletForce
{
    float bonusRatio;

    public HunterMarkForce(float bonusRatio)
    {
        this.bonusRatio = bonusRatio;
    }

    // IBulletForce: 표식 있으면 추가 피해
    public float GetMultiDamage(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();

        if (handler != null && handler.OnDebuff())
            return bullet.damage * bonusRatio;
        return 0;
    }

}
