using UnityEngine;

// (2026-11-21) 직업 (Lv.5 레벨업에서 5종 중 랜덤 3개 제시, 한 판에 하나)
// - 버프 칸은 늘리지 않음 (2026-11-21, 칸 추가는 Lv.10 BuffTileExpandKingAbility). 지금 버프 칸 전체에 직업 버프(tileBuffs)를 더함
// - 킹 스탯 버프(kingBuffs)는 기본 비어 있음
// 종족마다 직업 이름이 다르게 보일 예정 (아직 공통 이름)
public class JobKingAbility : KingAbility
{
    public KingJobType jobType;
    public Buff[] tileBuffs = { new Buff(StatType.AttackPower, 1.1f, StatOpType.Multiply) };
    public Buff[] kingBuffs = new Buff[0];

    KingTileBuffArea area;

    public override void OnEquip()
    {
        foreach (Buff b in kingBuffs)
            king.AddBuff(b);
        area = KingTileBuffArea.GetOrCreate(king);
        area.SetBuffs(this, tileBuffs);
    }

    public override void OnUnequip()
    {
        foreach (Buff b in kingBuffs)
            king.RemoveBuff(b);
        if (area != null)
            area.RemoveBuffs(this);
    }
}

// 직업 (에셋·저장에 번호로 → 뒤에만 추가)
public enum KingJobType
{
    Warrior, // 전사: 공격력
    Archer,  // 궁수: 공속
    Mage,    // 마법사: 스킬 쿨타임 감소
    Priest,  // 사제: 공격력·공속 조금씩
    Rogue,   // 도적: 치명타
}
