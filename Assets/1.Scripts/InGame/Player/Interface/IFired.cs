using UnityEngine;

public interface IFired
{
    void OnFired(ref Bullet bullet, ref PlayerBulletObject playerBulletObject, Vector2 dir);
}
