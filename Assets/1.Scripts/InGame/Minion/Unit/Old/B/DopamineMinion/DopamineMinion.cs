using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// [B] 광란: 기본 탄 발사 + 주변(detectRange)에서 적이 죽으면 추가 탄 extraShotCount발 발사
// 추가 탄 공격력 = AttackPower × extraShotDamageMul. 추가 발사는 일반 공격 타이머를 건드리지 않음 (Fire 대신 FireBullet)
// 추가 발사 후 coolTime 동안은 다시 발동 안 함
// 스킬(2026-11-15): 감지 범위 안 처치(경로 끝 도착 제외) SKILL_KILL_COUNT번 → SKILL_DURATION초 동안 공속 × SKILL_ATTACK_SPEED_MUL
//   버프 중 처치는 세지 않음. 추가 탄(기본 효과)은 그대로
public class DopamineMinion : Minion
{
    public const int SKILL_KILL_COUNT = 20;
    public const float SKILL_DURATION = 8f;
    public const float SKILL_ATTACK_SPEED_MUL = 3f;
    int killCharge;
    // 스킬 공속 버프 (AddBuff로 걸고 skillBuffTimer가 끝나면 RemoveBuff)
    Buff skillBuff;
    float skillBuffTimer;
    bool IsSkillBuffActive => skillBuffTimer > 0f;

    public override float SkillProgress => IsSkillBuffActive ? 0f : Mathf.Clamp01((float)killCharge / SKILL_KILL_COUNT);
    public override void FillSkillForTest() { killCharge = SKILL_KILL_COUNT - 1; }
    DopamineMinionData dopamineMinionData;
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
        dopamineMinionData = MinionManager.Instance.GetMinionData(key) as DopamineMinionData;
        if (skillBuff != null) // 풀링 재사용: 이전 스킬 버프 제거
            activeBuffs.Remove(skillBuff);
        skillBuff = null;
        skillBuffTimer = 0f;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        detectRange = dopamineMinionData.detectRanges.GetByMergeLevel(mergeLevel);
        extraShotDamageMul = dopamineMinionData.extraShotDamageMuls.GetByMergeLevel(mergeLevel);
        extraShotCount = dopamineMinionData.extraShotCount;
        coolTime = dopamineMinionData.extraShotCoolTime;
        coolTimer = 0;
        killCharge = 0;
        base.InitMinion();

        if (dopamineRange != null)
        {
            dopamineRange.localScale = Vector3.one * detectRange;
            dopamineRange.gameObject.SetActive(false);
        }
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
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        cts = new CancellationTokenSource();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if (IsBeingDragged) // 드래그 중엔 처치 충전·추가 탄 없음
            return;
        if (e.enemy != null && e.enemy.CurHp <= 0) // 경로 끝 도착은 스킬 충전 제외
            ChargeSkill(e.position);
        TryExtraShot(e.position);
    }

    void ChargeSkill(Vector2 deadPos)
    {
        if (dopamineMinionData == null || IsSkillBuffActive)
            return;
        if (TileManager.Instance == null || TileManager.Instance.GetTile(this) == null) // 타일 위에 있을 때만
            return;
        if (((Vector2)transform.position - deadPos).sqrMagnitude > detectRange * detectRange)
            return;

        killCharge++;
        if (killCharge < SKILL_KILL_COUNT)
            return;
        killCharge = 0;
        skillBuff = new Buff(StatType.AttackSpeed, SKILL_ATTACK_SPEED_MUL, StatOpType.Multiply);
        skillBuffTimer = SKILL_DURATION;
        AddBuff(skillBuff);
        PlayPowerUpEffect(SKILL_DURATION); // 강화 이펙트 (버프 동안)
        PlaySkillAnim();
    }

    void TryExtraShot(Vector2 deadPos)
    {
        if (dopamineMinionData == null || coolTimer > 0) // Spawn 전 / 쿨타임 중
            return;
        if (((Vector2)transform.position - deadPos).sqrMagnitude > detectRange * detectRange)
            return;

        coolTimer = coolTime; // 같은 프레임에 여러 마리가 죽어도 한 번만 발동
        ExtraShot().Forget();
    }

    async UniTaskVoid ExtraShot()
    {
        CancellationToken token = cts.Token;
        for (int i = 0; i < extraShotCount; i++)
        {
            Vector2 dir = AttackDirecton(); // 공격 사거리 안 타겟 (없으면 발사 안 함)
            if (dir == Vector2.zero)
                return;

            nextDamageMul = extraShotDamageMul;
            FireBullet(dir);
            nextDamageMul = 1f;

            if (i > 0 && await UniTask.Delay(Slime.COMBO_ATTACK_INTERVAL_MS, cancellationToken: token).SuppressCancellationThrow())
                return;

        }
    }

    // 스킬 버프 시간 처리 (끝나면 RemoveBuff). 버프 중이면 true
    bool UpdateSkillBuff()
    {
        if (skillBuffTimer <= 0f)
            return false;
        skillBuffTimer -= Time.deltaTime;
        if (skillBuffTimer > 0f)
            return true;
        skillBuffTimer = 0f;
        RemoveBuff(skillBuff);
        StopPowerUpEffect();
        skillBuff = null;
        return false;
    }

    public override void Update()
    {
        base.Update();
        UpdateSkillBuff();
        if (coolTimer > 0)
            coolTimer -= Time.deltaTime;
    }
}
