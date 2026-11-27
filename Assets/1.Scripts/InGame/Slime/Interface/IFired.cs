using UnityEngine;

public interface IFired
{
    void OnFired(ref BulletSpec bullet, ref AllyBulletObject bulletObject, Vector2 dir);
}
