using System;
using UnityEngine;

public class AttackPowerTileItem  : StatBuffTileItem //타일 위에 설치되는 아이템
{
    protected override StatType StatType => StatType.AttackPower;

    // 프리팹 생성(AddComponent) 시 기본 수치: 공격력 50% 증가
    void Reset()
    {
        opType = StatOpType.Multiply;
        value = 1.5f;
    }
}
