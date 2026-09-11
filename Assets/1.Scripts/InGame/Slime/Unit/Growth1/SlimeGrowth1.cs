using System.Collections.Generic;
using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class SlimeGrowth1 : Slime
{
    // [SerializeField] List<SlimeEnhanceAbility> curEnhanceAbilities;
    [SerializeField] List<IFireSlimeEnhanceAbility> fireAbilities = new List<IFireSlimeEnhanceAbility>();

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);
        for(int i = 0; i < slimeEnhanceAbilities.Length; i++)
        {
            if(userSlime.enhanceLevel >= slimeEnhanceAbilities[i].level)
            {
                slimeEnhanceAbilities[i].Spawn(this);
                if(slimeEnhanceAbilities[i] is IForceSlimeEnhanceAbility)
                {
                    (slimeEnhanceAbilities[i] as IForceSlimeEnhanceAbility).Force(this);
                }

                if(slimeEnhanceAbilities[i] is IFireSlimeEnhanceAbility)
                {
                    fireAbilities.Add(slimeEnhanceAbilities[i] as IFireSlimeEnhanceAbility);
                }
                
            }
        }        
    }

    public override void Fire(Vector2 dir)
    {
        base.Fire(dir);
        for(int i = 0; i < fireAbilities.Count; i++)
        {
            fireAbilities[i].Fire(dir);
        }   
    }
    
}
