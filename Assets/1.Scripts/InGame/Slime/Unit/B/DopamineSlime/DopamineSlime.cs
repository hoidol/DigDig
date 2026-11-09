using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// [B] 광란: 기본 탄 발사 + 주변(detectRange)에서 적/돌이 죽으면 추가 탄 extraShotCount발 발사
// 추가 탄 공격력 = AttackPower × extraShotDamageMul. 추가 발사는 일반 공격 타이머를 건드리지 않음 (Fire 대신 FireBullet)
// 추가 발사 후 coolTime 동안은 다시 발동 안 함
// [각성] (진화) 추가 탄 EVOLVE_EXTRA_SHOT_COUNT발
public class DopamineSlime : SlimeGrowth1
{
    public const int EVOLVE_EXTRA_SHOT_COUNT = 2;
    DopamineSlimeData dopamineSlimeData;
    AllyBulletSpec allyBulletSpec;

    public Transform dopamineRange; // 처치 감지 범위 표시 (선택(StartTarget) 중에만 보임, 크기 = detectRange. 공격 사거리 rangeTr와 같은 규칙)
    public float detectRange;
    public float extraShotDamageMul;
    public int extraShotCount;
    public float coolTime;

    public float coolTimer;
    float nextDamageMul = 1f; // GetBullet에서 쓸 공격력 배율 (추가 탄일 때만 extraShotDamageMul)
    CancellationTokenSource cts;

    public override void Spawn(Vector2 pos, int lv)
    {
        dopamineSlimeData = SlimeManager.Instance.GetSlimeData(key) as DopamineSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        detectRange = dopamineSlimeData.detectRanges.GetByMergeLevel(mergeLevel);
        extraShotDamageMul = dopamineSlimeData.extraShotDamageMuls.GetByMergeLevel(mergeLevel);
        extraShotCount = dopamineSlimeData.extraShotCount;
        coolTime = dopamineSlimeData.extraShotCoolTime;
        coolTimer = 0;
        base.InitSlime();

        if (dopamineRange != null)
        {
            dopamineRange.localScale = Vector3.one * detectRange;
            dopamineRange.gameObject.SetActive(false);
        }
    }

    protected override void OnEvolve()
    {
        extraShotCount = Mathf.Max(extraShotCount, EVOLVE_EXTRA_SHOT_COUNT);
    }

    public override void StartTarget()
    {
        base.StartTarget();
        if (dopamineRange != null)
            dopamineRange.gameObject.SetActive(true);
    }

    public override void EndTarget()
    {
        base.EndTarget();
        if (dopamineRange != null)
            dopamineRange.gameObject.SetActive(false);
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower * nextDamageMul;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        cts = new CancellationTokenSource();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e) => TryExtraShot(e.position);

    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        if (e.stone != null)
            TryExtraShot(e.stone.transform.position);
    }

    void TryExtraShot(Vector2 deadPos)
    {
        if (dopamineSlimeData == null || coolTimer > 0) // Spawn 전 / 쿨타임 중
            return;
        if (!RelayCrystalAuthority.InDetectRange(transform.position, detectRange, deadPos))
            return;

        coolTimer = coolTime; // 같은 프레임에 여러 마리가 죽어도 한 번만 발동
        ExtraShot().Forget();
    }

    async UniTaskVoid ExtraShot()
    {
        CancellationToken token = cts.Token;
        for (int i = 0; i < extraShotCount; i++)
        {
            if (i > 0 && await UniTask.Delay(KingSlime.COMBO_ATTACK_INTERVAL_MS, cancellationToken: token).SuppressCancellationThrow())
                return;

            Vector2 dir = AttackDirecton(); // 공격 사거리 안 타겟 (없으면 발사 안 함)
            if (dir == Vector2.zero)
                return;

            nextDamageMul = extraShotDamageMul;
            FireBullet(dir);
            nextDamageMul = 1f;
        }
    }

    public override void Update()
    {
        base.Update();
        if (coolTimer > 0)
            coolTimer -= Time.deltaTime;
    }
}
