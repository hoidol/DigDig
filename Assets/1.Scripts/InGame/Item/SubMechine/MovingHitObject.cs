using System.Collections.Generic;
using UnityEngine;

// 이동하면서 닿은 적에게 피해를 주는 오브젝트 공용 (빙석 눈 HailStone 등)
// - 위치/이동은 소유자(슬라임)나 하위 클래스가 담당. 여기선 적중 처리만
// - 트리거로 적과 닿으면 피해. 같은 적은 hitCooldown 동안 다시 안 때리고, 계속 닿아 있으면 쿨이 끝날 때 다시 때림
// - 탄이 아니므로 소유자 타일 아이템 효과(치명타·만능·거인 사냥 / 둔화·스턴·처치 코인)를 적중마다 직접 적용 (OrbitOrb와 같은 방식)
// - 하위 클래스는 OnHit을 재정의해 추가 효과(얼음 등)를 넣는다
// 프리팹: Collider2D(isTrigger) + Rigidbody2D(Kinematic) 필요
public abstract class MovingHitObject : MonoBehaviour
{
    public LayerMask layerMask = 1 << 3;

    protected IAllyUnit owner;
    public float damage;          // 소유자가 갱신 (공격력)
    public float hitCooldown = 0.5f;

    readonly Dictionary<IHittable, float> hitCooldowns = new Dictionary<IHittable, float>();
    readonly List<IHittable> expired = new List<IHittable>();

    public virtual void Init(IAllyUnit owner, float hitCooldown)
    {
        this.owner = owner;
        this.hitCooldown = hitCooldown;
        hitCooldowns.Clear();
    }

    protected virtual void Update()
    {
        expired.Clear();
        foreach (var pair in hitCooldowns)
        {
            if (pair.Value <= Time.time)
                expired.Add(pair.Key);
        }
        foreach (IHittable h in expired)
            hitCooldowns.Remove(h);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    protected virtual void OnTriggerStay2D(Collider2D other) => TryHit(other);

    void TryHit(Collider2D other)
    {
        if (owner == null || (owner is Object o && o == null))
            return;
        if (((1 << other.gameObject.layer) & layerMask.value) == 0) return;
        if (!other.TryGetComponent(out IHittable hittable)) return;
        if (hitCooldowns.ContainsKey(hittable)) return;

        hitCooldowns[hittable] = Time.time + hitCooldown;
        Hit(hittable);
    }

    void Hit(IHittable hittable)
    {
        TileItem tileItem = owner is IShotModifiable m ? m.ShotModifier.tileItem : null;
        AllyUnitDamageData damageData = new AllyUnitDamageData();
        damageData.Init(owner);
        damageData.damage = damage;
        damageData.attackType = owner.AttackType;
        if (tileItem != null)
            tileItem.ModifyDamageData(damageData);
        owner.AccumulateDamage(damageData.damage);
        hittable.TakeDamage(damageData);

        Vector2 dir = (Vector2)transform.position - (Vector2)owner.Transform.position;
        IBulletBehavior hitBehavior = tileItem != null ? tileItem.HitBehavior : null; // 확률형은 읽을 때 판정하므로 한 번만 읽음
        hitBehavior?.OnHit(null, hittable, new RaycastHit2D(), dir);

        OnHit(hittable);
    }

    // 피해 적용 후 추가 효과 (하위 클래스)
    protected virtual void OnHit(IHittable hittable) { }
}
