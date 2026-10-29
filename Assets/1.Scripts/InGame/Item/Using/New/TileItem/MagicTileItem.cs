using UnityEngine;

// 공격 타입을 Magic로 변경
public class MagicTileItem : AttackTypeTileItem //타일 위에 설치되는 아이템
{
    public override AttackType AttackType => AttackType.Magic;
}
