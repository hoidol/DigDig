using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public abstract class BulletObject : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float bulletRadius = 0.1f;
    public Vector3 direction;
    public float damage
    {
        get;
        set;
    }

    public LayerMask hitLayerMask;

    protected IHittable preTarget;
    const float LIFETIME = 15f;
    protected float lifetimeTimer;
    public DamageData damageData;

    public virtual void Shoot(Vector2 dir,float damage)
    {
        direction = dir;
        damageData.damage = damage;
        preTarget = null;
        lifetimeTimer = LIFETIME;
    }

    public virtual void Update()
    {
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0)
        {
            Release();
            return;
        }

        Move();
        CheckHit();
    }
    public virtual void Move()
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    public virtual void CheckHit()
    {
        RaycastHit2D hit2d = GetRaycastHit2D();
        if (hit2d)
        {
            Hit(hit2d);
        }
    }

    public virtual RaycastHit2D GetRaycastHit2D()
    {
        return Physics2D.CircleCast(transform.position, bulletRadius, direction, moveSpeed * Time.deltaTime, hitLayerMask);
    }


    public abstract IHittable Hit(RaycastHit2D hit2D);
    public virtual void Release()
    {
        gameObject.SetActive(false);
    }

    public virtual void Bounce(RaycastHit2D hit2D)
    {
        //
    }
}
