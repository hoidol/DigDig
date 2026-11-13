using UnityEngine;

// 슬라임 수 증가: 타일에 설치하지 않고, 획득하면 최대 슬라임 수(MinionFieldController.minionMaxCount)를 늘린다
// 뽑기 흐름(SelectItemCanvas)을 같이 쓰려고 TileItem을 상속하지만 NeedTile = false → 타일 선택 없이 바로 적용
public class AddMinionSpawnCountItem : TileItem
{
    public const int ADD_COUNT = 1;

    public override bool NeedTile => false;

    public override void OnEquip()
    {
        UpdateItem();
        MinionFieldController.Instance.minionMaxCount += ADD_COUNT;
        tile = null; // 설치하지 않으므로 타일과 연결하지 않음
    }

    public override void OnUnequip()
    {
    }
}
