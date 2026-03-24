using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BulletBase : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    protected Vector3 direction;
    float damage;

    public LayerMask hitLayerMask;
    public virtual void Shoot(Vector2 dir, float damage)
    {
        direction = dir;
        transform.right = dir;
        this.damage = damage;
    }

    public virtual void Update()
    {

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
            IHittable hit = hit2d.collider.GetComponent<IHittable>();
            if (hit != null)
            {
                Hit(hit);
            }
        }
    }

    public virtual RaycastHit2D GetRaycastHit2D()
    {
        return Physics2D.Raycast(transform.position, direction, moveSpeed * Time.deltaTime, hitLayerMask);
    }
    List<IBulletBehavior> behaviors = new List<IBulletBehavior>();

    public void AddBehavior(IBulletBehavior b)
    {
        var existing = behaviors.Find(x => x.GetType() == b.GetType());
        if (existing != null)
            existing.Merge(b);
        else
            behaviors.Add(b);
    }
    public void ClearBehaviors() => behaviors.Clear();

    public virtual void Hit(IHittable hit)
    {
        hit.TakeDamage(damage);
        bool shouldRelease = true;
        foreach (var b in behaviors)
        {
            if (b.OnHit(this, hit))
                break;
        }

        if (shouldRelease) Release();
    }
    public virtual void Release()
    {
        gameObject.SetActive(false);
    }

    public virtual void Bounce()
    {

    }
}
