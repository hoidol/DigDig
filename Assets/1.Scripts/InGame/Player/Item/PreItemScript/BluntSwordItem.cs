using UnityEngine;
using System.Linq;

public class BluntSwordItem : TriggerItem
{
    const float RADIUS = 1.5f;
    const float MULTI_DAMAGE = 0.5f;

    public Transform hitPoint;
    public ParticleEffect effect;
    DamageData damageData = new DamageData();

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return "전방으로 검을 휘두릅니다.";
    }

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        coolTime = 5;
        transform.parent = Player.Instance.bodyRootTr;
        transform.position = Player.Instance.bodyCenterTr.position;
        transform.localScale = Vector3.one;
        effect.Init();
    }

    public override void OnTrigger()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(hitPoint.position, RADIUS, LayerMask.GetMask("Hittable"));
        IHittable[] hittables = cols
            .Select(col => col.GetComponent<IHittable>())
            .Where(h => h != null)
            .ToArray();

        float damage = Player.Instance.statMgr.AttackPower * MULTI_DAMAGE;
        bool facingLeft = Player.Instance.bodyRootTr.localScale.x < 0;
        ParticleSystem.MainModule mainModule = effect.particleSystems[0].main;
        mainModule.startRotation = (facingLeft ? 220f : 40f) * Mathf.Deg2Rad;
        effect.Play();
        damageData.damage = damage;
        foreach (var hit in hittables)
        {
            hit.TakeDamage(damageData);
        }
    }
}
