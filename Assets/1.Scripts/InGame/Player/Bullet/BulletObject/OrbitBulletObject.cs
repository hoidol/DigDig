using UnityEngine;

public class OrbitBulletObject : MonoBehaviour
{
    const float HIT_RADIUS = 0.5f;
    const float HIT_COOLDOWN = 0.5f;

    readonly PlayerDamageData damageData = new();
    float timer;

    public LayerMask hittableLayer;
    int hitCount;

    public void SetOrbitBullet(OrbitBullet orbitBullet)
    {
        //orbitBullet.
        hitCount = 1 + Player.Instance.bounce;

        damageData.Init();
        damageData.cause = transform;
        damageData.Calculate();
        timer = 5;
    }

    void Update()
    {
        if (timer <= 0)
        {

            GetComponentInParent<Orbit>().RemoveOrbitBullet(this);
            return;
        }

        timer -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((hittableLayer & (1 << collision.gameObject.layer)) == 0) return;

        if (collision.TryGetComponent(out IHittable hit))
        {

            hit.TakeDamage(damageData);
            hitCount--;
            if (hitCount <= 0)
            {
                GetComponentInParent<Orbit>().RemoveOrbitBullet(this);
                return;
            }

            damageData.damage *= Player.Instance.statMgr.AmmoEfficiency;
            if (damageData.damage <= 1)
                damageData.damage = 1;

        }

    }

}
