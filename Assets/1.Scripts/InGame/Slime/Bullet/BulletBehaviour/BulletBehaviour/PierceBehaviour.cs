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

    // (2026-11-26) 화면 끝 반사로 관통 1 소모 (뾰족이 진화). 다 썼으면 true
    public bool ConsumeOne() => --remaining <= 0;

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
