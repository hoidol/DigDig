using UnityEngine;

// 버프 범위 확장: 위에 올라간 십자 버프 슬라임(CrossBuffMinion: Breeze/Rage/Keen)의 버프 범위가 십자 4칸 → 8방향
// 같은 TileGroup 안 규칙은 그대로. 다른 유닛이 올라가면 효과 없음
public class ExpandBuffRangeTileItem : TileItem //타일 위에 설치되는 아이템
{
    CrossBuffMinion target;

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is not CrossBuffMinion minion)
            return;

        target = minion;
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
        var equipped = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions;
        if (equipped == null)
            return false;
        foreach (UserMinion userMinion in equipped)
        {
            if (userMinion != null && MinionManager.Instance.GetMinionData(userMinion.key) is CrossBuffMinionData)
                return true;
        }
        return false;
    }
}
