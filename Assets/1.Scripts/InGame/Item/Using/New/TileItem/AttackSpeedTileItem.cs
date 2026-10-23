using System;
using UnityEngine;

public class AttackSpeedTileItem  : StatBuffTileItem //타일 위에 설치되는 아이템
{
    protected override StatType StatType => StatType.AttackSpeed;

    // 프리팹 생성(AddComponent) 시 기본 수치: 공격속도 30% 증가
    void Reset()
    {
        opType = StatOpType.Multiply;
        value = 1.3f;
    }
}
