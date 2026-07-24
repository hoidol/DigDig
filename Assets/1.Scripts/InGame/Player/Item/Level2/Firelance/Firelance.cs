using UnityEngine;
//화면 끝까지 감
public class Firelance : MonoBehaviour 
{
    public float moveSpeed = 12f;

    public float damage;
    public float duration;
    public float dps;
    Vector2 dir;
    Camera cam;
    void Awake()
    {
        cam = Camera.main;
    }

    public void Shoot(Vector2 dir)
    {
        this.dir = dir.normalized;


        float angle = Mathf.Atan2(this.dir.y, this.dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }




    void Update()
    {
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        //범위 밖으로 넘어가면 사라짐
        BounceAtScreenEdge();
    }

    void BounceAtScreenEdge()
    {
        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        if (vp.x <= 0f || vp.x >= 1f||vp.y <= 0f || vp.y >= 1f)
        {
            Destroy();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IHittable hittable = other.GetComponent<IHittable>();
        if (hittable == null || !hittable.CanHit()) return;

        hittable.TakeDamage(new DamageData { damage = damage });

        StatusEffectHandler handler = (hittable as Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new FlameEffect(duration, dps));

        // pierceCount--;
        // if (pierceCount <= 0)
        //     Destroy(gameObject);
    }

    void Destroy()
    {
        Destroy(gameObject);
    }
}