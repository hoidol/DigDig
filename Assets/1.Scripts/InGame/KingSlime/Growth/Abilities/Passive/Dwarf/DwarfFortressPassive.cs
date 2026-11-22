using System.Collections.Generic;

// 드워프 · 버프형 · 요새: 킹이 같은 칸에 머문 채 wave가 시작될 때마다 버프 칸 미니언 공격력 +perWave (최대 maxStacks번)
// 킹을 다른 칸으로 옮기면 바로 초기화 → 상성에 맞춰 옮길지, 버텨서 누적을 지킬지 고민
public class DwarfFortressPassive : PassiveKingAbility
{
    public float[] perWaves = { 0.05f, 0.075f, 0.1f };
    public int maxStacks = 5;

    int stack;
    Tile anchor;

    public override object[] GetDescArgs(int level) =>
        new object[] { Percent(LV(perWaves, level)), Percent(LV(perWaves, level) * maxStacks) };

    protected override void Apply()
    {
        Area.TilesRefreshed -= OnTilesRefreshed;
        Area.TilesRefreshed += OnTilesRefreshed;
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStart);
        if (anchor == null && TileManager.Instance != null)
            anchor = TileManager.Instance.GetTile(king);
        ApplyStack();
    }

    protected override void Clear()
    {
        Area.TilesRefreshed -= OnTilesRefreshed;
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);
        Area.RemoveBuffs(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);
    }

    void OnWaveStart(WaveStartEvent e)
    {
        Tile cur = TileManager.Instance != null ? TileManager.Instance.GetTile(king) : null;
        if (cur == null || cur != anchor)
        {
            anchor = cur;
            stack = 0;
        }
        if (stack < maxStacks)
            stack++;
        ApplyStack();
    }

    // 킹이 옮겨지면 초기화
    void OnTilesRefreshed(KingTileBuffArea area, List<Tile> tiles)
    {
        Tile cur = TileManager.Instance != null ? TileManager.Instance.GetTile(king) : null;
        if (cur == anchor || stack == 0)
        {
            anchor = cur;
            return;
        }
        anchor = cur;
        stack = 0;
        ApplyStack();
    }

    void ApplyStack()
    {
        if (stack <= 0)
        {
            Area.RemoveBuffs(this);
            return;
        }
        Area.SetBuffs(this, new[] { new Buff(StatType.AttackPower, 1f + LV(perWaves) * stack, StatOpType.Multiply) });
    }
}
