using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public abstract class BulletBase : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    protected Vector3 direction;
    public float damage
    {
        get;
        protected set;
    }

    public LayerMask hitLayerMask;

    protected IHittable preTarget;
    protected List<IBulletBehavior> behaviors = new List<IBulletBehavior>();
    protected List<IBulletForce> forces = new List<IBulletForce>();
    public virtual void Shoot(Vector2 dir, float damage)
    {
        direction = dir;
        transform.right = dir;

        this.damage = damage;


        preTarget = null;

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
            Hit(hit2d);
        }
    }

    public virtual RaycastHit2D GetRaycastHit2D()
    {
        return Physics2D.Raycast(transform.position, direction, moveSpeed * Time.deltaTime, hitLayerMask);
    }


    public void AddBehavior(IBulletBehavior b)
    {
        var existing = behaviors.Find(x => x.GetType() == b.GetType());
        if (existing != null)
            existing.Merge(b);
        else
            behaviors.Add(b);
    }
    public void ClearBehaviors() => behaviors.Clear();
    public void AddBulletForce(IBulletForce b)
    {
        forces.Add(b);

    }
    public void ClearBulletForce() => forces.Clear();

    public abstract void Hit(RaycastHit2D hit2D);
    public virtual void Release()
    {
        gameObject.SetActive(false);
    }

    public virtual void Bounce(RaycastHit2D hit2D)
    {
        //
    }
}
