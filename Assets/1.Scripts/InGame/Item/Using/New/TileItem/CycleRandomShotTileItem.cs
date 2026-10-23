using System;
using UnityEngine;

// interval번째 공격마다 랜덤 타겟
public class CycleRandomShotTileItem  : TileItem //타일 위에 설치되는 아이템
{
    public int interval = 3;

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is IShotModifiable m)
            m.ShotModifier.randomShotInterval = interval;
    }

    public override void OnExit(ITileObject tileObject)
    {
        if (tileObject is IShotModifiable m)
            m.ShotModifier.randomShotInterval = 0;
    }
}
