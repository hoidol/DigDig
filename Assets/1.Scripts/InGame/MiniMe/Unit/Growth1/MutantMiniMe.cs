using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class MutantMiniMe : MiniMeGrowth1
{
    public float healRange;
    public float healChance;
    public override void OnEnable()
    {
        GameEventBus.Subscribe<BulletFiredEvent>(OnBulletFiredEvent);
    }
    public override void OnDisable()
    {
        GameEventBus.Unsubscribe<BulletFiredEvent>(OnBulletFiredEvent);
    }
    public void OnBulletFiredEvent(BulletFiredEvent e)
    {
        float dis = Vector2.Distance(Character.Instance.transform.position, transform.position);
        if (dis <= healRange)
        {
            if (Random.value <= healChance)
            {
                Character.Instance.AddHp(1);
            }
        }
    }

    public override AllyBulletObject GetBullet()
    {
        throw new System.NotImplementedException();
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }
}