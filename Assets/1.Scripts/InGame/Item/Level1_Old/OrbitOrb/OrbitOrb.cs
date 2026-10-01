using UnityEngine;
using System.Collections.Generic;

// 회전 오브젝트 베이스: 회전은 OrbitItemBase의 컨테이너가 담당, 여기선 피해 처리만
public class OrbitOrb : MonoBehaviour
{
    public float damage;

    public List<HitCooldown> hitCooldowns = new();
    protected const float HIT_COOLDOWN = 0.5f;
    public LayerMask layerMask;
    public List<IBulletBehavior> bulletBehaviors = new List<IBulletBehavior>();

    public class HitCooldown
    {
        public IHittable hittable;
        public float cooltime;
    }
    OrbitMachine orbitMachine;
    public void Spawn(OrbitMachine orbitMachine)
    {
        this.orbitMachine = orbitMachine;
        bulletBehaviors.Clear();
    }
    public void AddBulletBehaviour(IBulletBehavior bulletBehavior)
    {
        bulletBehaviors.Add(bulletBehavior);
    }
    public virtual void Update()
    {
        for (int i = hitCooldowns.Count - 1; i >= 0; i--)
        {
            hitCooldowns[i].cooltime -= Time.deltaTime;
            if (hitCooldowns[i].cooltime <= 0)
                hitCooldowns.RemoveAt(i);
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & layerMask.value) == 0) return;
        if (!other.TryGetComponent(out IHittable hittable)) return;
        if (hitCooldowns.Exists(h => h.hittable == hittable)) return;

        hitCooldowns.Add(new HitCooldown { hittable = hittable, cooltime = HIT_COOLDOWN });
        OnHit(other, hittable);
    }

    public virtual void OnHit(Collider2D other, IHittable hittable)
    {
        bulletBehaviors.ForEach(e=>e.OnHit(null,hittable,new RaycastHit2D(),transform.position- orbitMachine.transform.position));
        hittable.TakeDamage(new DamageData { damage = damage });
    }
}
