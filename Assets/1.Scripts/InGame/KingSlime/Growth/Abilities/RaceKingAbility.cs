using System.Text;
using UnityEngine;

// (2026-11-20) 종족 (Lv.1 첫 레벨업에서 5종 중 랜덤 3개 제시, 한 판에 하나)
// - 버프 칸 모양(킹 기준 8칸 중 3칸, tileOffsets) → KingTileBuffArea
// - (2026-11-21) 종족 버프(buffs, 공격력·공속·치명타 등) 하나의 목록을 버프 칸 미니언과 킹 둘 다에 적용 (이전: 칸용·킹용 분리, 방어 타입 상대 피해)
// 직업 이름·스킬 이름은 종족마다 다르게 보일 예정 (raceType 기준). 외형 변화는 아직 없음(appearanceSprite만 준비)
public class RaceKingAbility : KingAbility
{
    public KingRaceType raceType;

    [Tooltip("버프 칸 (Tile.index 기준: x 오른쪽 +, y 아래쪽 +). 예: 오른쪽 = (1,0), 위 = (0,-1)")]
    public Vector2Int[] tileOffsets = { new Vector2Int(1, -1), new Vector2Int(1, 0), new Vector2Int(1, 1) };
    [Tooltip("버프 칸 미니언 + 킹 둘 다 받음")]
    public Buff[] buffs = { new Buff(StatType.AttackPower, 1.05f, StatOpType.Multiply) };
    public Color tileColor = new Color(1f, 0.85f, 0.3f, 0.35f);

    [Tooltip("나중에 종족별 킹 외형 변화용 (아직 사용 안 함)")]
    public Sprite appearanceSprite;
    public RuntimeAnimatorController appearanceAnimator;

    KingTileBuffArea area;

    public override void OnEquip()
    {
        foreach (Buff b in buffs)
            king.AddBuff(b);
        area = KingTileBuffArea.GetOrCreate(king);
        area.SetShape(this, tileOffsets, tileColor);
        area.SetBuffs(this, buffs); // 칸 미니언에겐 대상마다 새 Buff로 복사해서 걸림
    }

    public override void OnUnequip()
    {
        foreach (Buff b in buffs)
            king.RemoveBuff(b);
        if (area != null)
        {
            area.RemoveBuffs(this);
            area.RemoveShape(this);
        }
    }

    // 설명 {0} = 버프 칸 모양 (3×3, K = 킹)
    public override string GetValueText(int level) => GetShapeText(tileOffsets);

    public static string GetShapeText(Vector2Int[] offsets)
    {
        var sb = new StringBuilder();
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0)
                    sb.Append("K");
                else
                    sb.Append(System.Array.IndexOf(offsets, new Vector2Int(x, y)) >= 0 ? "■" : "□");
            }
            if (y < 1)
                sb.Append('\n');
        }
        return sb.ToString();
    }
}

// 종족 (에셋·저장에 번호로 → 뒤에만 추가)
public enum KingRaceType
{
    Human,
    Orc,
    Elf,
    Dwarf,
    Undead,
}
