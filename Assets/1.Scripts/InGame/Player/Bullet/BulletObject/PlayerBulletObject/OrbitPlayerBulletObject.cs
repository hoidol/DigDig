using UnityEngine;

using System.Collections.Generic;

public class OrbitPlayerBulletObject : PlayerBulletObject
{
    public override void Shoot(Vector2 dir)
    {
        Release();
    }


}

