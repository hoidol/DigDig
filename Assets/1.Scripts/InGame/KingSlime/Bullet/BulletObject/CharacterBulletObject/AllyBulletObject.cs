using UnityEngine;

using System.Collections.Generic;
using System.Linq;


public class AllyBulletObject : BulletObject
{
    public string key;

    protected List<IBulletBehavior> behaviors = new List<IBulletBehavior>();
    protected List<IBulletForce> forces = new List<IBulletForce>();
    [SerializeField] public AllyUnitDamageData allyUnitDamageData;
    public IAllyUnit allyUnit;
    public AllyUnitDamageData DamageData => allyUnitDamageData;

    // 발사 정보 (연발 등에서 같은 탄을 다시 쏠 때 사용)
    public BulletSpec spec;
    public Vector2 shootPos;
    public Vector2 shootDir;
    public float shootDamage;
    public bool isSplitShard; // 분열의 파편으로 쪼개진 탄 (다시 쪼개지지 않음)

    // 바운드 연쇄 (BounceBehavior): 맞은 적 목록·연쇄 횟수·쫓아가는 다음 적
    public readonly HashSet<IHittable> chainHitSet = new HashSet<IHittable>();
    public int chainCount;
    IHittable chainTarget;

    // (2026-11-26) 화면 끝 반사 (뾰족이 진화 PierceEvolution): 남은 횟수만큼 화면 끝에서 튕김, 관통 횟수 소모 옵션
    public int screenBounceLeft;
    public bool screenBounceConsumesPierce;

    public virtual void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        this.allyUnit = allyUnit;
        spec = bullet;
        isSplitShard = false; // 풀링 재사용
        chainHitSet.Clear();
        chainCount = 0;
        chainTarget = null;
        screenBounceLeft = 0;
        screenBounceConsumesPierce = false;
        AllyBulletSpec allyBulletSpec = bullet as AllyBulletSpec;
        transform.localScale = Vector3.one;

        if (allyUnitDamageData == null)
        {
            allyUnitDamageData = new AllyUnitDamageData();
        }
        allyUnitDamageData.Init(allyUnit); // 풀링 재사용 시 이전 타일 효과 초기화

        damage = allyBulletSpec.damage;
        allyUnitDamageData.damage = damage;
        allyUnitDamageData.attackType = allyUnit.AttackType;
        if (bullet.mustCrit)
            allyUnitDamageData.critChance = 100f; // 확정 치명타 (사냥꾼 저격탄 등)

        ClearBehaviors();
        ClearBulletForce();


        for (int i = 0; i < bullet.bulletBehaviors.Count; i++)
        {
            AddBehavior(bullet.bulletBehaviors[i]);
        }

        for (int i = 0; i < bullet.bulletForces.Count; i++)
        {
            AddBulletForce(bullet.bulletForces[i]);
        }
    }

    public override void Shoot(Vector2 dir, float damage)
    {
        base.Shoot(dir, damage);
        shootPos = transform.position;
        shootDir = dir;
        shootDamage = damage;
    }

    public override IHittable Hit(RaycastHit2D hit2D)
    {
        IHittable hit = hit2D.collider.GetComponent<IHittable>();
        if (hit == null)
            return null;

        if (preTarget == hit)
            return null;
        if (chainHitSet.Contains(hit)) // 연쇄 중 이미 맞은 적은 지나감
            return null;

        preTarget = hit;

        float finalDamage = damage * damageMultiplier;

        for (int i = 0; i < forces.Count; i++)
        {
            finalDamage += forces[i].GetMultiDamage(this, hit, hit2D, direction);
        }
        if (finalDamage < 1f)
            finalDamage = 1f;

        allyUnit.AccumulateDamage(finalDamage);
        allyUnitDamageData.damage = finalDamage;
        hit.TakeDamage(allyUnitDamageData);

        bool shouldRelease = true;
        foreach (var b in behaviors)
        {
            shouldRelease = b.OnHit(this, hit, hit2D, direction); //입사 벡터, 법선 벡터, 전달 필요 
            if (!shouldRelease)
                break;
        }

        if (shouldRelease)
        {
            Release();
        }
        return hit;
    }


    public void SetChainTarget(IHittable target)
    {
        chainTarget = target;
        direction = ((Vector2)(target.Transform.position - transform.position)).normalized;
        transform.right = direction;
    }

    // 연쇄 대상이 있으면 매 프레임 그 적 쪽으로 방향을 맞춤 (대상이 죽거나 사라지면 그대로 직진)
    public override void Move()
    {
        if (chainTarget != null)
        {
            if (chainTarget.CurHp <= 0 || !chainTarget.Transform.gameObject.activeInHierarchy)
            {
                chainTarget = null;
            }
            else
            {
                Vector2 toTarget = chainTarget.Transform.position - transform.position;
                if (toTarget != Vector2.zero)
                {
                    direction = toTarget.normalized;
                    transform.right = direction;
                }
            }
        }
        base.Move();
        if (screenBounceLeft > 0)
            TryScreenBounce();
    }

    // 화면 밖으로 나가려 하면 방향을 뒤집어 화면 안으로 (관통탄이면 관통 횟수 1 소모 → 다 쓰면 사라짐)
    void TryScreenBounce()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
            return;
        Vector3 c = cam.transform.position;
        float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
        Vector3 p = transform.position;
        bool bounced = false;
        if ((p.x < c.x - halfW && direction.x < 0f) || (p.x > c.x + halfW && direction.x > 0f))
        {
            direction.x = -direction.x;
            bounced = true;
        }
        if ((p.y < c.y - halfH && direction.y < 0f) || (p.y > c.y + halfH && direction.y > 0f))
        {
            direction.y = -direction.y;
            bounced = true;
        }
        if (!bounced)
            return;
        screenBounceLeft--;
        transform.right = direction;
        preTarget = null; // 돌아오면서 같은 적도 다시 맞을 수 있게
        if (screenBounceConsumesPierce)
            foreach (var b in behaviors)
                if (b is PierceBehavior pierce && pierce.ConsumeOne())
                {
                    Release();
                    return;
                }
    }

    // 연쇄 중엔 이미 맞은 적에 가려 다음 적을 못 찾지 않도록, 맞은 적을 건너뛰고 첫 적중 대상
    public override RaycastHit2D GetRaycastHit2D()
    {
        if (chainHitSet.Count == 0)
            return base.GetRaycastHit2D();

        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, bulletRadius, direction, moveSpeed * Time.deltaTime, hitLayerMask);
        foreach (RaycastHit2D h in hits)
        {
            IHittable hittable = h.collider.GetComponent<IHittable>();
            if (hittable != null && !chainHitSet.Contains(hittable))
                return h;
        }
        return default;
    }

    public void AddBehavior(IBulletBehavior b)
    {
        behaviors.Add(b);
        behaviors = behaviors.OrderBy(e => e.ApplyOrder).ToList();
    }
    public void ClearBehaviors() => behaviors.Clear();
    public void AddBulletForce(IBulletForce b)
    {
        forces.Add(b);

    }
    public void RemoveBulletForce(IBulletForce b)
    {
        forces.Remove(b);
    }
    public void ClearBulletForce() => forces.Clear();



    public override void Bounce(RaycastHit2D hit2D)
    {
        Vector2 dir = Vector2.Reflect(direction, hit2D.normal);
        if (dir != Vector2.zero)
            direction = dir;

        transform.right = direction;
    }
    public override void Release()
    {
        base.Release();
        BulletSpawner.Instance.ReturnPlayerBulletObject(key, this);
    }
}

