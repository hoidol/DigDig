using UnityEngine;

public class RandomEnhance5Ability : SlimeEnhanceAbility, IFireSlimeEnhanceAbility
{
    //[각성] 6번 탄을 발사할때마다 1빙향으로 발사
    public override void Activate(Slime slime, bool a)
    {
        base.Activate(slime,a);
        if((slime as RandomSlime).skillBulletCount < 3)
        {
            (slime as RandomSlime).skillBulletCount = 3;    
        }   
    }
    

    public void Fire(Vector2 dir)
    {
        (slime as RandomSlime).CountUp(); 
    }
}