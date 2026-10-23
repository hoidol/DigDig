using System;
using UnityEngine;

// 다중 타겟: 한 번 공격에 탄 수 +addShootCount (Slime.ShootCount에 더해짐, 탄마다 서로 다른 타겟)
public class MultiTargetTileItem  : TileItem //타일 위에 설치되는 아이템
{
    public int addShootCount = 1;

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is IShotModifiable m)
            m.ShotModifier.bonusShootCount += addShootCount;
    }

    public override void OnExit(ITileObject tileObject)
    {
        if (tileObject is IShotModifiable m)
            m.ShotModifier.bonusShootCount -= addShootCount;
    }
}
