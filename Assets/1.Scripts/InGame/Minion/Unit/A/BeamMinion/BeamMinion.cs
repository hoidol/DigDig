using UnityEngine;

// [A] 광선 링크: 필드(타일 위)에 광선 슬라임이 2마리 이상이면 모두 서로 레이저로 연결 (BeamLinkNetwork)
// 레이저에 닿은 적에게 0.25초마다 피해. 연결된 슬라임은 기본 공격을 하지 않고, 혼자면 기본 탄 공격
// [각성] (진화) 레이저에 닿은 적 피해 틱마다 EVOLVE_SLOW_DURATION초 감속
public class BeamMinion : MinionGrowth1
{
    public const float EVOLVE_SLOW_DURATION = 0.5f;
    BeamMinionData beamMinionData;
    AllyBulletSpec allyBulletSpec;

    public Material lineMaterial; // 레이저 머티리얼 (비어 있으면 Sprites/Default)
    public Color lineColor = new Color(1f, 0.3f, 0.3f, 0.9f);

    public float damageReductionPerLine;

    public float LineWidth => beamMinionData.lineWidths.GetByMergeLevel(mergeLevel);
    public float MinLineDamageMul => beamMinionData.minLineDamageMul;

    // 이번 프레임 레이저 연결 수 (BeamLinkNetwork가 매 프레임 갱신)
    public int linkCount;
    public bool IsLinked => linkCount > 0;

    public override void Spawn(Vector2 pos, int lv)
    {
        beamMinionData = MinionManager.Instance.GetMinionData(key) as BeamMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        damageReductionPerLine = beamMinionData.damageReductionPerLine;
        isDragging = false;
        base.InitMinion();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        BeamLinkNetwork.Register(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        BeamLinkNetwork.Unregister(this);
        linkCount = 0;
    }

    public override void Update()
    {
        BeamLinkNetwork.Tick(); // 프레임당 한 번만 실행됨 (여러 광선 슬라임이 불러도)
        base.Update();
    }

    // 레이저로 연결돼 있으면 기본 공격 안 함 (공격 타이머는 유지 → 연결이 끊기면 바로 발사)
    public override void Fire(Vector2 dir)
    {
        if (IsLinked)
            return;
        base.Fire(dir);
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    // 드래그 중엔 링크 해제, 드랍하면 다시 링크 (링크는 BeamLinkNetwork가 매 프레임 다시 계산)
    bool isDragging;

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

    // 레이저 대상이 될 수 있는지 (Spawn 끝났고 타일 위에 있고 드래그 중이 아님)
    public bool CanLink => beamMinionData != null && !isDragging && TileManager.Instance != null && TileManager.Instance.GetTile(this) != null;
}
