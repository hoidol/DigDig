using UnityEngine;
public class PierceBehavior : IBulletBehavior
{
    int remaining;
    float multi;
    public PierceBehavior(int count, float mul) { 
        remaining = count; 
        multi = mul;
        }

    public int ApplyOrder => 5;

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        bullet.damageMultiplier *= multi;
        // Debug.Log($"PierceBehavior OnHit {remaining}");
        if (--remaining <= 0)
        {
            return true;
        }

        return false;
    }
}
