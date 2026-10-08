using UnityEngine;

public class WackyEnhance11Ability : SlimeEnhanceAbility
{
     //[각성] 6번 탄을 발사할때마다 5빙향으로 발사
    public override void Activate(Slime slime, bool a)
    {
        base.Activate(slime, a);
        if((slime as WackySlime).skillBulletCount < 5)
        {
            (slime as WackySlime).skillBulletCount = 5;    
        }
        
    }
    
}