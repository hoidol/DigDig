using UnityEngine;
using System.Collections.Generic;

public class OrbitOrb : MonoBehaviour
{
    public float damage = 10f;
    public float orbitSpeed = 90f; // 초당 회전 각도

    // 같은 적 연속 히트 방지
    readonly Dictionary<IHittable, float> hitCooldowns = new();
    const float HIT_COOLDOWN = 1f;

    void Update()
    {
        transform.RotateAround(
            Player.Instance.transform.position,
            Vector3.forward,
            orbitSpeed * Time.deltaTime
        );

        // 쿨다운 감소
        var keys = new List<IHittable>(hitCooldowns.Keys);
        foreach (var key in keys)
        {
            hitCooldowns[key] -= Time.deltaTime;
            if (hitCooldowns[key] <= 0)
                hitCooldowns.Remove(key);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out IHittable hittable)) return;
        if (hitCooldowns.ContainsKey(hittable)) return;

        hitCooldowns[hittable] = HIT_COOLDOWN;
        OnHit(hittable);
    }

    protected virtual void OnHit(IHittable hittable)
    {
        hittable.TakeDamage(new DamageData() { damage = damage });
    }
}
