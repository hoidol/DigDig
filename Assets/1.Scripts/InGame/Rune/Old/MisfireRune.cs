using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//5초마다 주변 넓은 범위 화염 공격 - 많이 가질 수록 범위 넓어짐
public class MisfireRune : TriggerRune
{
    public float baseCoolTime = 4;
    public float radiuse = 2.5f;
    public float duration = 5;
    public float DPS = 3;
    public Effect effect;

    public override void OnEquip()
    {
        transform.SetParent(Slime.Instance.transform);
        transform.localRotation = Quaternion.identity;
        transform.position = Slime.Instance.transform.position;

        base.OnEquip();
    }
    public override void UpdateRune()
    {
        base.UpdateRune();
        coolTime = baseCoolTime;
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        effect.Play();
        Collider2D[] hits = Physics2D.OverlapCircleAll(Slime.Instance.transform.position, radiuse, LayerMask.GetMask("Hittable"));
        int hitCount=0;
        for (int i = 0; i < hits.Length; i++)
        {
            if(hitCount > 6)
                break;
            if (hits[i].TryGetComponent<IHittable>(out IHittable hittable))
            {
                
                StatusEffectHandler handler = (hittable as Component)?.GetComponent<StatusEffectHandler>();
                handler?.Apply(new FlameEffect(duration ,DPS));
                hittable.TakeDamage(new DamageData { damage = Slime.Instance.statMgr.AttackPower });
                hitCount ++;
            }
        }
    }
}