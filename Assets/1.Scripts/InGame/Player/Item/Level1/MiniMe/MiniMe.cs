using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class MiniMe : Ally
{
    public MiniMeMovement miniMeMovement;
    public float attackPower;
    public Transform rootTr;
    public float attackSpeed;
    public float attackTimer;
    public virtual void Awake()
    {

    }
    public virtual void OnEnable()
    {

    }
    public virtual void OnDisable()
    {
    }

    public virtual void Update()
    {
        rootTr.localScale = new Vector3(Player.Instance.MoveDirection.x >= 0 ? 1 : -1, 1, 1);
        attackTimer += Time.deltaTime;
        if (attackTimer > attackSpeed)
        {
            Fire();
        }
    }

    public virtual void Fire()
    {
        AllyBullet allyBullet = AllyBullet.Instantiate();
        allyBullet.transform.position = transform.position;
        allyBullet.Shoot(Player.Instance.weapon.GetAttackDirection());
        allyBullet.damage = attackPower;
        attackTimer = 0;
    }


}