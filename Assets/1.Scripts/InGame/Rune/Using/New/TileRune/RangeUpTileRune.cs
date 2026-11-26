using System;
using UnityEngine;

public class RangeUpTileRune  : StatBuffTileRune //타일 위에 설치되는 아이템
{
    protected override StatType StatType => StatType.AttackRange;

    // 프리팹 생성(AddComponent) 시 기본 수치: 사거리 +2
    void Reset()
    {
        opType = StatOpType.Add;
        value = 3f;
    }
}
