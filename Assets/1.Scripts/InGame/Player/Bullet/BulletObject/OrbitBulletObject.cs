using UnityEngine;

public class OrbitBulletObject : MonoBehaviour
{

    readonly DamageData damageData = new();
    float timer;

    public LayerMask hittableLayer;
    int hitCount;
    float damage;
    public void SetOrbitBullet(OrbitBullet orbitBullet)
    {
        //orbitBullet.
        hitCount = 1 + Player.Instance.bounce;

        // damage = orbitBullet.GetDamage();
        timer = 5;
    }

    void Update()
    {
        if (timer <= 0)
        {

            // GetComponentInParent<Orbit>().RemoveOrbitBullet(this);
            return;
        }

        timer -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((hittableLayer & (1 << collision.gameObject.layer)) == 0) return;

        if (collision.TryGetComponent(out IHittable hit))
        {
            damageData.damage = damage;

            hit.TakeDamage(damageData);
            hitCount--;
            if (hitCount <= 0)
            {
                // GetComponentInParent<Orbit>().RemoveOrbitBullet(this);
                return;
            }

            damageData.damage *= Player.Instance.statMgr.AmmoEfficiency;
            if (damageData.damage <= 1)
                damageData.damage = 1;

        }

    }

}
