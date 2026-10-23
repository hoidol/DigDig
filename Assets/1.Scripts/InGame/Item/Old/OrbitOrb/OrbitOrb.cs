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
        Vector2 dir = transform.position - orbitMachine.transform.position;
        bulletBehaviors.ForEach(e=>e.OnHit(null,hittable,new RaycastHit2D(),dir));

        // 탄을 쏘지 않으므로 타일 아이템 효과를 적중마다 직접 적용
        TileItem tileItem = orbitMachine.owner is IShotModifiable m ? m.ShotModifier.tileItem : null;
        AllyUnitDamageData damageData = new AllyUnitDamageData { damage = damage, attackType = orbitMachine.attackType, allyUnit = orbitMachine.owner };
        if (tileItem != null)
            tileItem.ModifyDamageData(damageData);
        hittable.TakeDamage(damageData);
        IBulletBehavior hitBehavior = tileItem != null ? tileItem.HitBehavior : null; // 확률형은 읽을 때 판정하므로 한 번만 읽음
        if (hitBehavior != null)
            hitBehavior.OnHit(null, hittable, new RaycastHit2D(), dir);
    }
}
