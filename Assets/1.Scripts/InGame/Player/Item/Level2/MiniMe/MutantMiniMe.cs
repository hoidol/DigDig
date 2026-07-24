using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class MutantMiniMe : MiniMe
{
    public float healRange;
    public float healChance;
    public override void OnBulletFiredEvent(BulletFiredEvent e)
    {
        base.OnBulletFiredEvent(e);
        float dis = Vector2.Distance(Player.Instance.transform.position,transform.position);
        if(dis <= healRange)
        {
            if(Random.value <= healChance)
            {
                Player.Instance.AddHp(1);
            }
        } 
    }
}