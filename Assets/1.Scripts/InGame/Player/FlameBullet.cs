using UnityEngine;

public class FlameBullet : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    Vector3 direction;
    float damage;
    public void Shoot(Vector2 dir, float damage)
    {
        direction = dir;
        this.damage = damage;
    }

    void Update()
    {
        transform.position += direction * moveSpeed * Time.deltaTime;

        RaycastHit2D hit2d = Physics2D.Raycast(transform.position, direction, moveSpeed * Time.deltaTime, LayerMask.GetMask("Hittable"));

        if (hit2d)
        {
            IHittable hit = hit2d.collider.GetComponent<IHittable>();
            if (hit != null)
            {
                Hit(hit);
            }
        }
    }

    void Hit(IHittable hit)
    {
        hit.OnHit(damage);
        gameObject.SetActive(false);

    }
}
