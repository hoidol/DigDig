using UnityEngine;

// 소환 블록(1기): 지정한 종을 지정한 칸에 바로 소환하고 다음 단계
// (2026-11-20) 골드 소환 버튼 삭제 → 버튼 없이 바로 소환 (카드 소환 방식 튜토리얼은 아직 미정)
// 여러 기를 뽑게 하려면 이 블록을 여러 개 이어 둔다 (사이에 대화를 끼울 수 있음)
// 합성을 가르치려면 두 블록을 같은 key로 (예: 1번째·3번째 = 같은 key → 합성 블록에서 두 칸 지정)
public class SpawnMinionTutorialStep : TutorialStep
{
    [Header("나올 미니언 key (장착 여부와 무관)")]
    public string minionKey = "Ice";

    [Header("나올 칸. 전체 6*6 기준 (x: 왼쪽 0 → 오른쪽 5, y: 맨 위 0 → 맨 아래 5). 비어 있지 않으면 랜덤 칸")]
    public Vector2Int tileIndex;

    public override void Prepare()
    {
        Tutorial.ReserveTile(tileIndex);
    }

    protected override void OnEnter()
    {
        if (!MinionManager.Instance.minionDataDic.ContainsKey(minionKey))
        {
            Debug.LogError($"SpawnMinionTutorialStep 없는 미니언 key {minionKey}");
            Complete();
            return;
        }

        Tile tile = TileManager.Instance.GetTile(tileIndex);
        if (tile == null || !tile.IsEmpty)
            tile = TileManager.Instance.GetRandomEmptyTile();
        if (tile != null)
        {
            Minion minion = KingSlime.Instance.AddMinion(minionKey, 0);
            tile.SetTileObject(minion);
            SoundManager.Play(SFXType.MinionAppear);
        }
        Complete();
    }
}
