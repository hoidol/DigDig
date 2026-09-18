using UnityEngine;

public class FlameEnhance8Ability : SlimeEnhanceAbility, ISkillSlimeEnhanceAbility
{
    // [각성] 능력 개방 - 가까운 적에게 화염 
     //최대 6기 
    public float cooltime;

    public Effect effect;
    public float radius = 2.5f;
    
    public void Execute()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(Character.Instance.transform.position, radius, LayerMask.GetMask("Hittable"));
        int hitCount=0;
        FlameSlime flameSlime = slime as FlameSlime;
        for (int i = 0; i < hits.Length; i++)
        {
            if(hitCount > 6)
                break;
            if (hits[i].TryGetComponent<IHittable>(out IHittable hittable))
            {
                
                StatusEffectHandler handler = (hittable as Component)?.GetComponent<StatusEffectHandler>();
                handler?.Apply(new FlameEffect(flameSlime.burnDuration, flameSlime.burnDPS));
                hittable.TakeDamage(new DamageData { damage = Character.Instance.statMgr.AttackPower });
                hitCount ++;
            }
        }
    }
}
