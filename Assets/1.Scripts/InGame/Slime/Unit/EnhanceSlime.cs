using System.Collections.Generic;
using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class EnhanceSlime : Slime
{
    [SerializeField] List<IFireSlimeEnhanceAbility> fireAbilities = new List<IFireSlimeEnhanceAbility>();

    public override void InitSlime()
    {
        ApplyEnhanceAbility();
    }

    public void ApplyEnhanceAbility()
    {
        for(int i = 0; i < slimeEnhanceAbilities.Length; i++)
        {
            bool activate = userSlime.enhanceLevel >= slimeEnhanceAbilities[i].level;
            slimeEnhanceAbilities[i].Activate(this,activate);
            if(activate)
            {                
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
