using UnityEngine;

// [S] 시체수집: 일반 탄 공격 + 공격 사거리 안에서 적이 처치되면(누가 잡든) coinPerKill 코인
// - 경로 끝에 도착해 사라진 적은 처치가 아니므로 제외 (체력 0 이하일 때만)
// - 타일 위에 있을 때만 (드래그 중 제외)
// - 강화: 5 = 획득 범위 ×1.15, 8 = [각성] 엘리트/보스 처치 시 +5, 11 = 20% 확률로 2배
// → 처치가 몰리는 자리(딜러 옆, 경로가 지나는 곳)에 두는 배치 포인트
public class ScavengerSlime : SlimeGrowth1
{
    ScavengerSlimeData scavengerSlimeData;
    AllyBulletSpec allyBulletSpec;

    // 강화 능력이 바꿀 수 있음 (InitSlime에서 base 호출 전에 기본값 설정)
    public float coinRangeMul;
    public int eliteBossBonusCoin;
    public float doubleChance; // 0~1

    bool isDragging;

    public float CoinRange => AttackRange * coinRangeMul;

    public override void Spawn(Vector2 pos, int lv)
    {
        scavengerSlimeData = SlimeManager.Instance.GetSlimeData(key) as ScavengerSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        coinRangeMul = 1f;
        eliteBossBonusCoin = 0;
        doubleChance = 0f;
        isDragging = false;
        base.InitSlime();
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
        if (scavengerSlimeData == null || isDragging || e.enemy == null)
            return;
        if (e.enemy.CurHp > 0) // 경로 끝 도착 등 처치가 아닌 경우
            return;
        if (TileManager.Instance == null || TileManager.Instance.GetTile(this) == null)
            return;
        if (Vector2.Distance(transform.position, e.position) > CoinRange)
            return;

        int coin = scavengerSlimeData.coinPerKill;
        if (e.enemy.IsEliteOrBoss)
            coin += eliteBossBonusCoin;
        if (doubleChance > 0f && Random.value < doubleChance)
            coin *= 2;

        KingSlime.Instance.AddCoin(coin);
        CoinText.Instantiate().SetCoinText((Vector3)e.position + Vector3.up * 0.4f, $"+{coin}");
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
