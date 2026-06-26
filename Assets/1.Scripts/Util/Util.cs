using UnityEngine;
public class Util
{
    public static float GetAngleFromVector(Vector3 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

}

