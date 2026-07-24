using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class MiniMe : Ally
{
    public MiniMeMovement miniMeMovement;
    public float attackPower;
    
    public virtual void Awake()
    {
        
    }
   public virtual void OnEnable()
    {
        GameEventBus.Subscribe<BulletFiredEvent>(OnBulletFiredEvent);
        
    }
    public virtual void OnDisable()
    {
        GameEventBus.Unsubscribe<BulletFiredEvent>(OnBulletFiredEvent);
    }

    //플레이어가 공격하면 같이 쏨
    public virtual void OnBulletFiredEvent(BulletFiredEvent e)
    {
     Fire();         
    }
    public virtual void Fire()
    {
        AllyBullet allyBullet = AllyBullet.Instantiate();
        allyBullet.transform.position = transform.position;
        allyBullet.Shoot(Player.Instance.weapon.GetAttackDirection());
        allyBullet.damage = attackPower;  
    }


}