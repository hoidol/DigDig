using UnityEngine;

public class WackyEnhance11Ability : SlimeEnhanceAbility
{
     //[각성] 스킬 발동 시 7방향으로 발사
    public override void Activate(Slime slime, bool a)
    {
        base.Activate(slime, a);
        if((slime as WackySlime).skillBulletCount < 7)
        {
            (slime as WackySlime).skillBulletCount = 7;    
        }
        
    }
    
}