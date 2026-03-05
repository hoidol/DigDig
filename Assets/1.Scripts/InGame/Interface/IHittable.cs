using UnityEngine;

public interface IHittable
{
    Transform Transform
    {
        get;
    }
    void TakeDamage(float damage);
}