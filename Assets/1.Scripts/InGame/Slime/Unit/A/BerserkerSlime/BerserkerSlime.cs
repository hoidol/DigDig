using UnityEngine;

// [A] 버서커: 기본 탄 발사 + 주변(detectRange)에서 적/돌이 죽을 때마다 공속 중첩 (최대 maxStack)
// 공속 = 기본(버프 포함) × (1 + stack × attackSpeedPerStack). 마지막 처치 후 stackDuration 지나면 중첩 초기화
// 구조는 광란(DopamineSlime)과 같음: 처치 이벤트 구독 + 감지 범위 표시(berserkerRange, 선택 중에만)
// [각성] (진화) 최대 중첩일 때 공격력 ×EVOLVE_MAX_STACK_ATTACK_POWER_MUL
public class BerserkerSlime : SlimeGrowth1
{
    public const float EVOLVE_MAX_STACK_ATTACK_POWER_MUL = 1.2f;
    BerserkerSlimeData berserkerSlimeData;
    AllyBulletSpec allyBulletSpec;

    public Transform berserkerRange; // 처치 감지 범위 표시 (선택(StartTarget) 중에만 보임, 크기 = detectRange)
    public float detectRange;
    public float attackSpeedPerStack;
    public int maxStack;
    public float stackDuration;

    public int stack;
    public float stackTimer;

    public override void Spawn(Vector2 pos, int lv)
    {
        berserkerSlimeData = SlimeManager.Instance.GetSlimeData(key) as BerserkerSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        detectRange = berserkerSlimeData.detectRanges.GetByMergeLevel(mergeLevel);
        attackSpeedPerStack = berserkerSlimeData.attackSpeedPerStacks.GetByMergeLevel(mergeLevel);
        maxStack = berserkerSlimeData.maxStack;
        stackDuration = berserkerSlimeData.stackDuration;
        stack = 0;
        stackTimer = 0;
        base.InitSlime();

        if (berserkerRange != null)
        {
            berserkerRange.localScale = Vector3.one * detectRange;
            berserkerRange.gameObject.SetActive(false);
        }
    }

    // 중첩은 버프까지 반영된 값에 곱함 (중첩이 바뀔 때마다 UpdateSlime으로 다시 계산)
    public override void UpdateSlime()
    {
        base.UpdateSlime();
        statDic[StatType.AttackSpeed] *= 1f + stack * attackSpeedPerStack;
        if (isEvolved && stack >= maxStack)
            statDic[StatType.AttackPower] *= EVOLVE_MAX_STACK_ATTACK_POWER_MUL;
    }

    public override void StartTarget()
    {
        base.StartTarget();
        if (berserkerRange != null)
            berserkerRange.gameObject.SetActive(true);
    }

    public override void EndTarget()
    {
        base.EndTarget();
        if (berserkerRange != null)
            berserkerRange.gameObject.SetActive(false);
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e) => TryAddStack(e.position);

    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        if (e.stone != null)
            TryAddStack(e.stone.transform.position);
    }

    void TryAddStack(Vector2 deadPos)
    {
        if (berserkerSlimeData == null) // Spawn 전
            return;
        if (!RelayCrystalAuthority.InDetectRange(transform.position, detectRange, deadPos))
            return;

        stackTimer = stackDuration; // 최대 중첩이어도 유지 시간은 갱신
        if (stack >= maxStack)
            return;

        stack++;
        UpdateSlime();
    }

    public override void Update()
    {
        base.Update();
        if (stack > 0)
        {
            stackTimer -= Time.deltaTime;
            if (stackTimer <= 0)
            {
                stack = 0;
                UpdateSlime();
            }
        }
    }
}
