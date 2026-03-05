using UnityEngine;
using System.Linq;
public class BulletBase : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    Vector3 direction;
    float damage;

    public LayerMask hitLayerMask;
    public virtual void Shoot(Vector2 dir, float damage)
    {
        direction = dir;
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

    public void Hit(IHittable hit)
    {
        hit.TakeDamage(damage);

        Release();

    }
    public virtual void Release()
    {
        gameObject.SetActive(false);
    }
}
