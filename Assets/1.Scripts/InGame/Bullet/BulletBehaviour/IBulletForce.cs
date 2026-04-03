using UnityEngine;
public interface IBulletForce
{
    //조건
    public float GetMultiDamage(BulletBase bullet, IHittable hit, RaycastHit2D hit2D);
}