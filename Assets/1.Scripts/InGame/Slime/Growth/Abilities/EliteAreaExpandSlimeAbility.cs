using System.Collections.Generic;
using UnityEngine;

// (2026-11-21) 버프 칸 확장: 버프 칸 1개 추가
// (2026-11-23) Lv.2·Lv.5 두 번 (같은 카드 레벨 +1마다 칸 1개 추가). Lv.5(두 번째)엔 필드 최대 미니언 수 +1 (maxMinionCountAdds)
// - 추가 칸(extraOffset)은 카드를 보여줄 때 지금 버프 칸을 뺀 나머지 칸 중 랜덤 (카드 3장이 서로 다른 칸, LevelUpChoiceRule.PickExpandTiles)
// (2026-11-25) 용사 강화 카드 "정예 칸 추가" (최대 3, SlimeGrowthSetting.heroBoostAbilities). 최대 미니언 수 증가 없음 (프리팹 maxMinionCountAdds 비움)
// - 추가 칸에도 종족·직업 버프가 모두 걸림 (EliteArea = 모든 출처 버프가 합친 모양 전체에)
public class EliteAreaExpandSlimeAbility : SlimeAbility
{
    // 고를 때 정해지는 추가 버프 칸 (슬라임 기준, Tile.index 단위: x 오른쪽 +, y 아래쪽 +). OnEquip·OnLevelChanged 전에 설정
    [HideInInspector] public Vector2Int extraOffset;
    [Tooltip("레벨별 필드 최대 미니언 수 증가 (index = 레벨-1). 기본 Lv.2(=두 번째 확장, 슬라임 Lv.5) +1")]
    public int[] maxMinionCountAdds = { 0, 1 };

    readonly List<Vector2Int> offsets = new List<Vector2Int>();
    public IReadOnlyList<Vector2Int> Offsets => offsets; // 테스트 복구(WaveSnapshotSystem)용
    int addedMinionCount;
    EliteArea area;

    public override void OnEquip()
    {
        area = EliteArea.GetOrCreate(slime);
        AddExtra();
    }

    public override void OnLevelChanged() => AddExtra();

    void AddExtra()
    {
        if (!offsets.Contains(extraOffset))
            offsets.Add(extraOffset);
        area.SetShape(this, offsets.ToArray());
        int add = GetMinionCountAdd(level);
        if (add != 0 && MinionFieldController.Instance != null)
        {
            MinionFieldController.Instance.minionMaxCount += add;
            addedMinionCount += add;
        }
    }

    int GetMinionCountAdd(int lv) => maxMinionCountAdds != null && lv - 1 < maxMinionCountAdds.Length && lv >= 1 ? maxMinionCountAdds[lv - 1] : 0;

    // 설명 {0} = 그 레벨에서 더 받는 것 (최대 미니언 수)
    public override string GetValueText(int level)
    {
        int add = GetMinionCountAdd(level);
        return add > 0 ? $"\n필드 최대 미니언 수 +{add}" : "";
    }

    public override void OnUnequip()
    {
        if (area != null)
            area.RemoveShape(this);
        if (addedMinionCount != 0 && MinionFieldController.Instance != null)
            MinionFieldController.Instance.minionMaxCount -= addedMinionCount;
        addedMinionCount = 0;
    }
}
