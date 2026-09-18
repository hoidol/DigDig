using UnityEngine;

public class WackyEnhance8Ability : SlimeEnhanceAbility, IFireSlimeEnhanceAbility
{
    //[각성] 6번 탄을 발사할때마다 4빙향으로 발사

    public override void Activate(Slime slime, bool a)
    {
        base.Activate(slime,a);
        if((slime as WackySlime).skillBulletCount < 4)
        {
            (slime as WackySlime).skillBulletCount = 4;    
        }   
    }
    

    public void Fire(Vector2 dir)
    {
        (slime as WackySlime).CountUp();
        //dir 기준 360/6해서 일정하게 발사하기     
    }
}