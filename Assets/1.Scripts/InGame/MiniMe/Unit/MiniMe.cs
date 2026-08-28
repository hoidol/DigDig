using Cysharp.Threading.Tasks;
using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class MiniMe : MonoBehaviour,IAllyUnit
{
    public string key;
    public int level;
    
    public MiniMeMovement miniMeMovement;
    public abstract float AttackSpeed(); 
    public abstract float AttackPower(); 

    public Transform rootTr;
    public float attackTimer;
    public MiniMeData MiniMeData => MiniMeManager.Instance.GetMiniMeData(key);

    public float AccumulatedDamage { get; set; }

    public void AccumulateDamage(float d)
    {
        AccumulatedDamage+= d;
    }
    public Transform Transform => transform;
    public string Key => key;
    public AllyType AllyType => AllyType.MiniMe;


    public virtual void Awake()
    {

    }
    public virtual void OnEnable()
    {

    }
    public virtual void OnDisable()
    {
    }
    
    public virtual void Spawn(Vector2 pos, int lv)
    {
        transform.position = pos;
        this.level = lv;
    }

    
    public virtual void Update()
    {
        rootTr.localScale = new Vector3(Character.Instance.weapon.GetAttackDirection().x >= 0 ? 1 : -1, 1, 1);
        UpdateAttack();
    }

    public virtual void UpdateAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer > AttackSpeed())
        {
            Fire(AttackDirecton());
        }
    }

    public virtual Vector2 AttackDirecton()
    {
        Transform targetTr = InGameUtil.FindTarget(transform.position, 10, targetLayerMask);

        Vector2 fireDir = Character.Instance.moveJoystick.Direction;
        if (targetTr != null)
        {
            fireDir = (targetTr.position - transform.position).normalized;
        }

        return fireDir;
    }

    public virtual string GetDescription()
    {
        return null;
    }
    public LayerMask targetLayerMask;

    public virtual void Fire(Vector2 dir)
    {
        AllyBulletObject baseBullet = GetBullet();
        baseBullet.transform.position = transform.position;
        baseBullet.Shoot(dir,AttackPower());
        attackTimer = 0;
    }

    public virtual AllyBulletObject GetBullet()
    {
        return null;
    }

    
    public virtual async UniTask<(bool, string, int)>  Merge(MiniMe target)
    {
        if (MiniMeData.growth > 1 || target.MiniMeData.growth > 1)
            return (false, null,0);
        if (MiniMeData.growth != target.MiniMeData.growth)
            return (false, null,0);
        if (level != target.level)
            return (false, null,0);

        string pickedMiniMeKey = null;
        int lv = 0;
        if (MiniMeData.growth == 1 && level == 2)
        {
            pickedMiniMeKey = await SelectMergeMiniMeCanvas.Instance.OpenCanvas(this, target);
            if (pickedMiniMeKey == null)
                return (false, null,0);
        }
         else if (MiniMeData.growth == 1&& level < 2)
        {
            UserMiniMe userMiniMe = UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes[Random.Range(0, UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes.Length)];
            pickedMiniMeKey = userMiniMe.key;
            lv = level+1;
        }
        else if (MiniMeData.growth == 0)
        {
            UserMiniMe userMiniMe = UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes[Random.Range(0, UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes.Length)];
            pickedMiniMeKey = userMiniMe.key;
        }

        return (true, pickedMiniMeKey,lv);
    }

}
