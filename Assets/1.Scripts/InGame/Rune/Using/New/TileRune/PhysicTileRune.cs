using UnityEngine;

// 공격 타입을 Physic로 변경
public class PhysicTileRune : AttackTypeTileRune //타일 위에 설치되는 아이템
{
    public override AttackType AttackType => AttackType.Physic;
}
