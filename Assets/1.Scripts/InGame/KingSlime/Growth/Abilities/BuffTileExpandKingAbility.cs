using UnityEngine;

// (2026-11-21) Lv.5 종족 강화(버프 칸 확장): 버프 칸 1개 추가
// (2026-11-22) 필드 최대 미니언 수 +1은 삭제
// - 추가 칸(extraOffset)은 카드를 보여줄 때 지금 버프 칸(종족 칸)을 뺀 나머지 칸 중 랜덤 (카드 3장이 서로 다른 칸, LevelUpChoiceRule.PickExpandTiles)
// - 추가 칸에도 종족·직업 버프가 모두 걸림 (KingTileBuffArea = 모든 출처 버프가 합친 모양 전체에)
public class BuffTileExpandKingAbility : KingAbility
{
    // 고를 때 정해지는 추가 버프 칸 (킹 기준, Tile.index 단위: x 오른쪽 +, y 아래쪽 +). OnEquip 전에 설정
    [HideInInspector] public Vector2Int extraOffset;

    KingTileBuffArea area;

    public override void OnEquip()
    {
        area = KingTileBuffArea.GetOrCreate(king);
        area.SetShape(this, new[] { extraOffset });
    }

    public override void OnUnequip()
    {
        if (area != null)
            area.RemoveShape(this);
    }
}
