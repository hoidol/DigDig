using UnityEngine;

// 암천창 투사체: 직선 이동, pierceLeft 횟수만큼 관통 후 파괴
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Spear : MonoBehaviour
{
    public float moveSpeed = 12f;

    float damage;
    int pierceLeft;
    Vector2 dir;

    public void Init(Vector2 dir, float damage, int pierceCount)
    {
        this.dir = dir.normalized;
        this.damage = damage;
        pierceLeft = pierceCount;

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        float angle = Mathf.Atan2(this.dir.y, this.dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IHittable hittable = other.GetComponent<IHittable>();
        if (hittable == null || !hittable.CanHit()) return;

        hittable.TakeDamage(new DamageData { damage = damage });
        pierceLeft--;
        if (pierceLeft <= 0)
            Destroy(gameObject);
    }
}
