using UnityEngine;

// 적중한 적에게 피뢰침 표식 부여, 표식은 duration 후 자동 제거
public class LightningRodBehavior : IBulletBehavior
{
    readonly float duration;

    public LightningRodBehavior(float duration)
    {
        this.duration = duration;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        Component comp = hit as Component;
        if (comp != null && comp.GetComponent<LightningRodMark>() == null)
        {
            LightningRodMark mark = comp.gameObject.AddComponent<LightningRodMark>();
            Object.Destroy(mark, duration);
        }
        return true;
    }
}
