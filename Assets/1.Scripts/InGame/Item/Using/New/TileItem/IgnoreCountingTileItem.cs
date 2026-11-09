using UnityEngine;

// 카운팅 제외: 위에 올라간 슬라임은 최대 슬라임 수에 세지 않는다 (SlimeFieldController.ignoreSlimeCount)
public class IgnoreCountingTileItem : TileItem //타일 위에 설치되는 아이템
{
    Slime ignoredSlime;

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is not Slime slime)
            return;

        ignoredSlime = slime;
        SlimeFieldController.Instance.ignoreSlimeCount++;
    }

    public override void OnExit(ITileObject tileObject)
    {
        if (ignoredSlime == null || !ReferenceEquals(ignoredSlime, tileObject))
            return;

        ignoredSlime = null;
        SlimeFieldController.Instance.ignoreSlimeCount--;
    }
}
