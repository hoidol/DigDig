using System.Collections.Generic;
using UnityEngine;

// 캐릭터 주변 칸(AreaOffsets 모양)에 있는 슬라임에 버프가 적용
// 캐릭터나 주변 슬라임의 배치가 바뀔 때마다 대상을 다시 계산 → 범위에 들어온 슬라임은 버프, 나간 슬라임은 해제
public abstract class GrantItem : Item
{
    public GrantAreaType grantAreaType;

    public StatType statType = StatType.AttackPower;
    public StatOpType opType = StatOpType.Multiply;
    public float value = 1.2f;

    // 캐릭터 칸 기준 범위 (칸 단위, x=가로 y=세로)
    protected abstract Vector2Int[] AreaOffsets { get; }

    // 버프를 준 슬라임 → 걸어준 Buff (RemoveBuff는 참조로 지우므로 인스턴스를 들고 있음)
    readonly Dictionary<Slime, Buff> appliedBuffs = new Dictionary<Slime, Buff>();

    public override void OnEquip()
    {
        base.OnEquip();
        GameEventBus.Subscribe<TileObjectChangedEvent>(OnTileObjectChanged);
        Refresh();
    }

    public override void OnUnequip()
    {
        GameEventBus.Unsubscribe<TileObjectChangedEvent>(OnTileObjectChanged);
        ClearAll();
    }

    void OnTileObjectChanged(TileObjectChangedEvent e)
    {
        Refresh();
    }

    // 현재 캐릭터 위치 기준 범위 안의 슬라임 목록 (캐릭터와 같은 TileGroup 안에서만)
    public List<Slime> GetTargets()
    {
        List<Slime> targets = new List<Slime>();
        Tile center = TileManager.Instance.GetTile(Character.Instance);
        foreach (Tile tile in TileManager.Instance.GetTilesAround(center, AreaOffsets))
        {
            if (tile.tileGroup != center.tileGroup) // 바로 옆 칸이라도 그룹이 다르면 영향 없음
                continue;
            if (tile.tileObject is Slime slime)
                targets.Add(slime);
        }
        return targets;
    }

    void Refresh()
    {
        List<Slime> targets = GetTargets();

        // 범위에서 빠진 슬라임 버프 해제
        List<Slime> removed = new List<Slime>();
        foreach (var pair in appliedBuffs)
        {
            if (!targets.Contains(pair.Key))
                removed.Add(pair.Key);
        }
        foreach (Slime slime in removed)
        {
            if (slime != null) // 이미 Destroy된 슬라임은 해제 생략
                slime.RemoveBuff(appliedBuffs[slime]);
            appliedBuffs.Remove(slime);
        }

        // 새로 들어온 슬라임 버프 적용
        foreach (Slime slime in targets)
        {
            if (appliedBuffs.ContainsKey(slime))
                continue;
            Buff buff = new Buff(statType, value, opType);
            appliedBuffs.Add(slime, buff);
            slime.AddBuff(buff);
        }
    }

    void ClearAll()
    {
        foreach (var pair in appliedBuffs)
        {
            if (pair.Key != null)
                pair.Key.RemoveBuff(pair.Value);
        }
        appliedBuffs.Clear();
    }
}

public enum GrantAreaType
{
    AroundFour,
    AroundFight,
    Diagonal,
    TileGrant
}
