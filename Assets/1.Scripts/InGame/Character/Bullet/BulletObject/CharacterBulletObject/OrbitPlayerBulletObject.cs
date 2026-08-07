using UnityEngine;

using System.Collections.Generic;

public class OrbitPlayerBulletObject : CharacterBulletObject
{
    public override void Shoot(Vector2 dir)
    {
        Release();
    }


}

