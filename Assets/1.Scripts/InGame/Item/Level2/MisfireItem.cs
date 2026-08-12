using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//5초마다 주변 넓은 범위 화염 공격 - 많이 가질 수록 범위 넓어짐
public class MisfireItem : TriggerItem
{
    public float baseCoolTime = 5;
    public float radiuse = 2.5f;
    public float duration = 5;
    public float DPS = 3;
    public Effect effect;

    public override void OnEquip()
    {
        transform.SetParent(Character.Instance.transform);
        transform.localRotation = Quaternion.identity;
        transform.position = Character.Instance.transform.position;

        base.OnEquip();
    }
    public override void UpdateItem()
    {
        base.UpdateItem();
        coolTime = baseCoolTime;
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        effect.Play();
        Collider2D[] hits = Physics2D.OverlapCircleAll(Character.Instance.transform.position, radiuse * count, LayerMask.GetMask("Hittable"));
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].TryGetComponent<IHittable>(out IHittable hittable))
            {
                StatusEffectHandler handler = (hittable as Component)?.GetComponent<StatusEffectHandler>();
                handler?.Apply(new FlameEffect(duration ,DPS));
                hittable.TakeDamage(new DamageData { damage = Character.Instance.statMgr.AttackPower });
            }
        }
    }
}