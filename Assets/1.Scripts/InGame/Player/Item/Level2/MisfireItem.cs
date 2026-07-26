using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//5초마다 주변 넓은 범위 화염 공격
public class MisfireItem : TriggerItem
{
    public float[] coolTimes = { 5, 4, 3 };
    public float[] radiuses = { 2, 2.2f, 2.5f };
    public float[] durations = { 5, 5, 5 };
    public float[] DPSs = { 3f, 4f, 5f };
    public Effect effect;

    public override void OnEquip()
    {
        transform.SetParent(Player.Instance.transform);
        transform.localRotation = Quaternion.identity;
        transform.position = Player.Instance.transform.position;

        base.OnEquip();
    }
    public override void UpdateItem()
    {
        base.UpdateItem();
        coolTime = coolTimes[count - 1];
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        effect.Play();
        Collider2D[] hits = Physics2D.OverlapCircleAll(Player.Instance.transform.position, radiuses[count - 1], LayerMask.GetMask("Hittable"));
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].TryGetComponent<IHittable>(out IHittable hittable))
            {
                StatusEffectHandler handler = (hittable as Component)?.GetComponent<StatusEffectHandler>();
                handler?.Apply(new FlameEffect(durations[count - 1], DPSs[count - 1]));
                hittable.TakeDamage(new DamageData { damage = Player.Instance.statMgr.AttackPower });
            }
        }
    }
}