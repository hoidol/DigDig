using UnityEngine;

public class WackyEnhance8Ability : SlimeEnhanceAbility//, IFireSlimeEnhanceAbility
{
    //[각성] 6번 탄을 발사할때마다 3빙향으로 발사
    public override void Activate(Slime slime, bool a)
    {
        base.Activate(slime,a);
        if((slime as WackySlime).skillBulletCount < 3)
        {
            (slime as WackySlime).skillBulletCount = 3;    
        }   
    }
    

    // public void Fire(Vector2 dir)
    // {
    //     (slime as WackySlime).CountUp(); 
    // }
}