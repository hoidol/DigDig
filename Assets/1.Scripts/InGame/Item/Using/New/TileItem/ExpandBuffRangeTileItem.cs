using UnityEngine;

// 버프 범위 확장: 위에 올라간 십자 버프 슬라임(CrossBuffSlime: Breeze/Rage/Keen)의 버프 범위가 십자 4칸 → 8방향
// 같은 TileGroup 안 규칙은 그대로. 다른 유닛이 올라가면 효과 없음
public class ExpandBuffRangeTileItem : TileItem //타일 위에 설치되는 아이템
{
    CrossBuffSlime target;

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is not CrossBuffSlime slime)
            return;

        target = slime;
        target.AddRangeExpand();
    }

    public override void OnExit(ITileObject tileObject)
    {
        if (target == null || !ReferenceEquals(target, tileObject))
            return;

        target.RemoveRangeExpand();
        target = null;
    }

    // 장착한 슬라임 중 십자 버프 슬라임이 없으면 뽑기 후보에서 제외 (쓸모없는 선택지 방지)
    public override bool CanDraw()
    {
        var equipped = UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes;
        if (equipped == null)
            return false;
        foreach (UserSlime userSlime in equipped)
        {
            if (userSlime != null && SlimeManager.Instance.GetSlimeData(userSlime.key) is CrossBuffSlimeData)
                return true;
        }
        return false;
    }
}
