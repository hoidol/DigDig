using UnityEngine;

public interface IHittable
{
    Transform Transform
    {
        get;
    }
    void OnHit(float damage);
}