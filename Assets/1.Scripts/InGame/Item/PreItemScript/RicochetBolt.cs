using UnityEngine;

// 방랑탄 투사체: 화면 끝에서 반사, 적/광석 관통 피해
public class RicochetBolt : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lifeTime = 5f;

    float damage;
    Vector2 dir;
    Camera cam;
    float elapsed;
    float damageRate;

    public void Init(float damage, Vector2 dir)
    {
        this.damage = damage;
        this.dir = dir;
        cam = Camera.main;
    }
    public void SetDamageRate(float dRate)
    {
        damageRate = dRate;
    }

    void Update()
    {
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        transform.right = dir;

        elapsed += Time.deltaTime;
        if (elapsed >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        BounceAtScreenEdge();
    }

    void BounceAtScreenEdge()
    {
        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        if (vp.x <= 0f || vp.x >= 1f) dir.x = -dir.x;
        if (vp.y <= 0f || vp.y >= 1f) dir.y = -dir.y;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IHittable hittable = other.GetComponent<IHittable>();
        if (hittable == null || !hittable.CanHit()) return;
        hittable.TakeDamage(new DamageData { damage = damage * damageRate });
        // 관통: 파괴되지 않음
    }
}
