using UnityEngine;

using System.Linq;
public class Sword : TriggerItem
{
    //activeTime 없음
    public Transform hitPoint;
    public float[] radiuses = { 1f, 1.5f, 2f };
    public float[] effectSizes = { 4f, 5.5f, 7f };
    public float[] multiDamages = { 0.5f, 0.75f, 1.25f };
    public ParticleEffect effect;
    DamageData damageData = new DamageData();
    public float GetRadius(int c = -1)
    {
        if (c < 0)
            c = count;

        return radiuses[c - 1];
    }
    public float GetMultiDamage(int c = -1)
    {
        if (c < 0)
            c = count;

        return multiDamages[c - 1];
    }
    public float GetDamage(int c = -1)
    {
        if (c < 0)
            c = count;

        return Player.Instance.statMgr.AttackPower * GetMultiDamage(c);
    }
    public override string GetDescription(int c = -1)
    {
        return "전방으로 검을 휘두릅니다.";
    }
    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        coolTime = 5;
        transform.parent = Player.Instance.bodyRootTr;
        transform.position = Player.Instance.bodyCenterTr.position;//위치 맞추기
        effect.Init();
    }
    public override void UpdateItem()
    {
        ParticleSystem.MainModule mainModule = effect.particleSystems[0].main;
        mainModule.startSize = effectSizes[count];

    }
    public override void OnTrigger()
    {
        Debug.Log($"{key} OnActive");
        Collider2D[] cols = Physics2D.OverlapCircleAll(hitPoint.position, GetRadius(count));
        IHittable[] hittables = cols
            .Select(col => col.GetComponent<IHittable>())
            .Where(h => h != null)
            .ToArray();

        float damage = GetDamage(count);
        //이펙트 필요
        ParticleSystem.MainModule mainModule = effect.particleSystems[0].main;
        bool facingLeft = Player.Instance.bodyRootTr.localScale.x < 0;
        mainModule.startRotation = (facingLeft ? 220f : 40f) * Mathf.Deg2Rad;
        effect.Play();
        damageData.damage = damage;
        foreach (var hit in hittables)
        {
            hit.TakeDamage(damageData);
        }
    }
}
