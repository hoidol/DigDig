using UnityEngine;

// 공격 타입 변경: 위에 올라간 유닛의 공격 타입을 바꿔 방어 타입 상성을 맞춘다 (Physic/MagicTileItem)
public abstract class AttackTypeTileItem : TileItem //타일 위에 설치되는 아이템
{
    public abstract AttackType AttackType { get; }

    public override void ModifyDamageData(AllyUnitDamageData data)
    {
        data.attackType = AttackType;
    }
}
