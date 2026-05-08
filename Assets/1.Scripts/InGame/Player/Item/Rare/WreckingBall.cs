using System.Collections.Generic;
using UnityEngine;

// 도탄 볼: Raycast로 벽 반사, OnTriggerEnter2D로 피해 처리
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class WreckingBall : MonoBehaviour
{
    public float moveSpeed = 6f;
    public LayerMask wallMask;
    public Transform spriteTr;

    float damage;
    float damageRate;
    Vector2 dir;

    readonly List<HitCooldown> hitCooldowns = new();
    const float HIT_COOLDOWN = 0.5f;

    class HitCooldown
    {
        public IHittable hittable;
        public float cooltime;
    }

    public void Init(float damage, float damageRate)
    {
        this.damage = damage;
        this.damageRate = damageRate;

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        if (spriteTr != null) spriteTr.up = dir;
    }

    void Update()
    {
        // 쿨다운 감소
        for (int i = hitCooldowns.Count - 1; i >= 0; i--)
        {
            hitCooldowns[i].cooltime -= Time.deltaTime;
            if (hitCooldowns[i].cooltime <= 0)
                hitCooldowns.RemoveAt(i);
        }

        // 벽 Raycast → 반사
        float step = moveSpeed * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, step, wallMask);
        if (hit)
        {
            dir = Vector2.Reflect(dir, hit.normal);
            transform.position = (Vector3)(hit.point + hit.normal * 0.01f);
            OnHit(hit);
        }
        else
        {
            transform.position += (Vector3)(dir * step);
        }

        if (spriteTr != null) spriteTr.up = dir;
    }

    void OnHit(RaycastHit2D hit2d)
    {
        IHittable hittable = hit2d.collider.GetComponent<IHittable>();
        if (hittable == null || !hittable.CanHit()) return;
        if (hitCooldowns.Exists(h => h.hittable == hittable)) return;

        hitCooldowns.Add(new HitCooldown { hittable = hittable, cooltime = HIT_COOLDOWN });
        hittable.TakeDamage(new DamageData { damage = damage * damageRate });
    }
}
