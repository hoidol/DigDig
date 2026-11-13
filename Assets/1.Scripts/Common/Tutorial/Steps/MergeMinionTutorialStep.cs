using UnityEngine;

// 합성 블록: 플레이어가 fromTile의 미니언을 toTile의 미니언 위로 드래그해 합성하면 다음 단계
// 두 칸의 미니언은 같은 종·같은 머지 레벨이어야 한다 (소환 블록에서 같은 key로 지정)
// 합성할 때까지 손가락 드래그 연출이 계속 나온다 (DragTutorialStep)
public class MergeMinionTutorialStep : DragTutorialStep
{
    [Header("끌어올 미니언이 있는 칸. 전체 6*6 기준 (x: 왼쪽 0 → 오른쪽 5, y: 맨 위 0 → 맨 아래 5)")]
    public Vector2Int fromTileIndex = new Vector2Int(2, 2);

    [Header("합성 대상 미니언이 있는 칸")]
    public Vector2Int toTileIndex = new Vector2Int(1, 2);

    protected override void OnEnter()
    {
        Tile fromTile = TileManager.Instance.GetTile(fromTileIndex);
        Tile toTile = TileManager.Instance.GetTile(toTileIndex);
        Minion from = fromTile != null ? fromTile.tileObject as Minion : null;
        Minion to = toTile != null ? toTile.tileObject as Minion : null;
        if (from == null || to == null
            || !Minion.CanMerge(from.key, from.mergeLevel, from.MinionData.growth, to.key, to.mergeLevel, to.MinionData.growth))
        {
            Debug.LogError($"MergeMinionTutorialStep 합성할 수 없는 칸 {fromTileIndex} → {toTileIndex} (두 칸에 같은 종·같은 레벨 미니언이 있어야 함)");
            Complete();
            return;
        }

        GameEventBus.Subscribe<MergedMinionEvent>(OnMerged);
        BeginGuide(from, toTile);
    }

    protected override void OnExit()
    {
        GameEventBus.Unsubscribe<MergedMinionEvent>(OnMerged);
        base.OnExit();
    }

    // 합성 결과는 드롭 뒤에 나오므로 MergedMinionEvent로 완료
    protected override bool IsDropDone() => false;

    void OnMerged(MergedMinionEvent e)
    {
        CompleteAfterDrop();
    }
}
