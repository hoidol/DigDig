using UnityEngine;
public interface IBulletForce
{
    //조건
    public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir);
}