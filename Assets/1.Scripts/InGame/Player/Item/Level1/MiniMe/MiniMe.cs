using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class MiniMe : Ally
{
    public Rigidbody2D rg2D;
    public float moveSpeed;
    public float inRange= 5;
    public float[] attackPowers = {3,6,9};
    void Awake()
    {
        rg2D = GetComponent<Rigidbody2D>();
    }
    void OnEnable()
    {
        GameEventBus.Subscribe<BulletFiredEvent>(OnBulletFiredEvent);
        
    }
    void OnDisable()
    {
        GameEventBus.Unsubscribe<BulletFiredEvent>(OnBulletFiredEvent);
    }

    //플레이어가 공격하면 같이 쏨
    void OnBulletFiredEvent(BulletFiredEvent e)
    {
        AllyBullet allyBullet = AllyBullet.Instantiate();
        allyBullet.transform.position = transform.position;
        allyBullet.Shoot(e.dir);
        allyBullet.damage = attackPowers[level-1];        
    }

    private void FixedUpdate() 
    {
        Vector2 vec = Player.Instance.transform.position -transform.position;
        if(vec.magnitude < inRange)
        {
            rg2D.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir =vec.normalized;
        rg2D.linearVelocity = dir*moveSpeed;
    }

}