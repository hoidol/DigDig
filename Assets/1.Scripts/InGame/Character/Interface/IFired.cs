using UnityEngine;

public interface IFired
{
    void OnFired(ref Bullet bullet, ref CharacterBulletObject playerBulletObject, Vector2 dir);
}
