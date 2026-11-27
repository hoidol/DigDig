using UnityEngine;

// [S] 시체수집: 일반 탄 공격 + 공격 사거리 안에서 적이 처치되면(누가 잡든) coinPerKill 코인
// - 경로 끝에 도착해 사라진 적은 처치가 아니므로 제외 (체력 0 이하일 때만)
// - 타일 위에 있을 때만 (드래그 중 제외)
// → 처치가 몰리는 자리(딜러 옆, 경로가 지나는 곳)에 두는 배치 포인트
// 스킬(2026-11-15): 같은 조건의 처치가 SKILL_KILL_COUNT마리 쌓이면 SKILL_BASE_COIN + SKILL_COIN_PER_MERGE × 머지 레벨 코인 (기본 처치 코인은 그대로)
// [각성] (진화) 엘리트/보스 처치 시 +EVOLVE_ELITE_BOSS_BONUS_COIN
public class ScavengerMinion : Minion
{
    public const int EVOLVE_ELITE_BOSS_BONUS_COIN = 30;
    public const int SKILL_KILL_COUNT = 20;
    public const int SKILL_BASE_COIN = 10;
    public const int SKILL_COIN_PER_MERGE = 2;
    int killCharge;

    public override float SkillProgress => Mathf.Clamp01((float)killCharge / SKILL_KILL_COUNT);
    public override void FillSkillForTest() { killCharge = SKILL_KILL_COUNT - 1; }
    // 머지 레벨 0(표시 1단계) = 10, 1 = 12, 2 = 14 …
    public static int GetSkillCoin(int mergeLevel) => SKILL_BASE_COIN + SKILL_COIN_PER_MERGE * mergeLevel;
    ScavengerMinionData scavengerMinionData;
    AllyBulletSpec allyBulletSpec;

    bool isDragging;

    public float CoinRange => AttackRange;

    public override void Spawn(Vector2 pos, int lv)
    {
        scavengerMinionData = MinionManager.Instance.GetMinionData(key) as ScavengerMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        isDragging = false;
        killCharge = 0;
        base.InitMinion();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        if (scavengerMinionData == null || isDragging || e.enemy == null)
            return;
        if (e.enemy.CurHp > 0) // 경로 끝 도착 등 처치가 아닌 경우
            return;
        if (TileManager.Instance == null || TileManager.Instance.GetTile(this) == null)
            return;
        if (((Vector2)transform.position - (Vector2)e.position).sqrMagnitude > CoinRange * CoinRange)
            return;

        int coin = scavengerMinionData.coinPerKill;
        if (isEvolved && e.enemy.IsEliteOrBoss)
            coin += EVOLVE_ELITE_BOSS_BONUS_COIN;

        Slime.Instance.AddCoin(coin);
        CoinText.Instantiate().SetCoinText((Vector3)e.position + Vector3.up * 0.4f, $"+{coin}");

        killCharge++;
        if (killCharge >= SKILL_KILL_COUNT)
        {
            killCharge = 0;
            int skillCoin = GetSkillCoin(mergeLevel);
            Slime.Instance.AddCoin(skillCoin);
            CoinText.Instantiate().SetCoinText(transform.position + Vector3.up * 0.8f, $"+{skillCoin}");
            PlaySkillAnim();
        }
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    public override void StartDrag()
    {
        base.StartDrag();
        isDragging = true;
    }

    public override void EndDrag(Tile t)
    {
        base.EndDrag(t);
        isDragging = false;
    }
}
