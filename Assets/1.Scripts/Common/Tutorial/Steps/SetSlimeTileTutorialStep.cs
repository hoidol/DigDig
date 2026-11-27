using UnityEngine;

// 슬라임 위치 지정 블록: 슬라임을 지정한 칸으로 바로 옮기고 다음 단계로 (플레이어 조작 없음)
public class SetSlimeTileTutorialStep : TutorialStep
{
    [Header("전체 6*6 기준 칸 (x: 왼쪽 0 → 오른쪽 5, y: 맨 위 0 → 맨 아래 5)")]
    public Vector2Int tileIndex = new Vector2Int(4, 4);

    public override void Prepare()
    {
        Tutorial.ReserveTile(tileIndex);
    }

    protected override void OnEnter()
    {
        Tile tile = TileManager.Instance.GetTile(tileIndex);
        Slime slime = Slime.Instance;
        if (tile == null)
        {
            Debug.LogError($"SetSlimeTileTutorialStep 없는 칸 {tileIndex}");
        }
        else if (tile.tileObject != (ITileObject)slime)
        {
            // 그 칸에 슬라임이 있으면 자리 교체, 비어 있으면 이동
            if (tile.tileObject == null || !MinionFieldController.SwapTileObjects(slime, tile.tileObject))
            {
                TileManager.Instance.RemoveTileObject(slime);
                tile.SetTileObject(slime);
            }
        }
        Complete();
    }
}
