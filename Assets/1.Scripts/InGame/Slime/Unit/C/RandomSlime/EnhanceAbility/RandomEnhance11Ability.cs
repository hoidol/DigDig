using UnityEngine;

public class RandomEnhance11Ability : SlimeEnhanceAbility
{
     //[각성] 6번 탄을 발사할때마다 5빙향으로 발사
    public override void Activate(Slime slime, bool a)
    {
        base.Activate(slime, a);
        if((slime as RandomSlime).skillBulletCount < 7)
        {
            (slime as RandomSlime).skillBulletCount = 7;    
        }
        
    }
    
}