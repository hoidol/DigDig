using System.Collections;
using UnityEngine;

// 소환 블록(1기): 플레이어가 소환 버튼을 누르면 지정한 종이 지정한 칸에 나오고 다음 단계
// 여러 기를 뽑게 하려면 이 블록을 여러 개 이어 둔다 (사이에 대화를 끼울 수 있음)
// 합성을 가르치려면 두 블록을 같은 key로 (예: 1번째·3번째 = 같은 key → 합성 블록에서 두 칸 지정)
// 이 단계 동안은 화면 블록을 푼다. 소환 가격보다 코인이 모자라면 모자란 만큼 채워 준다 (소환은 평소처럼 코인을 냄)
// 소환 버튼을 숨겨 두었다면 이 블록 앞에 ShowSpawnButton 블록으로 켜 둘 것
public class SpawnMinionTutorialStep : TutorialStep
{
    [Header("나올 미니언 key (장착 여부와 무관)")]
    public string minionKey = "Ice";

    [Header("나올 칸. 전체 6*6 기준 (x: 왼쪽 0 → 오른쪽 5, y: 맨 위 0 → 맨 아래 5). 비어 있지 않으면 랜덤 칸")]
    public Vector2Int tileIndex;

    [Header("손 터치 연출 위치. 비워 두면 씬의 소환 버튼(MinionSpawnButton)")]
    public Transform fingerTarget;

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

        if (fingerTarget == null)
        {
            MinionSpawnButton button = FindFirstObjectByType<MinionSpawnButton>(FindObjectsInactive.Include);
            if (button != null)
                fingerTarget = button.transform;
        }

        int lack = MinionSpawnButton.GetSpawnPrice() - KingSlime.Instance.coin;
        if (lack > 0)
            KingSlime.Instance.AddCoin(lack);

        MinionSpawnButton.forcedMinionKey = minionKey;
        MinionSpawnButton.forcedTile = TileManager.Instance.GetTile(tileIndex);
        GameEventBus.Subscribe<SpawnMinionEvent>(OnSpawnMinion);
        SetBlock(false);
        if (Canvas != null && fingerTarget != null)
            Canvas.PlayFingerTap(fingerTarget);
    }

    protected override void OnExit()
    {
        StopAllCoroutines();
        SetBlock(true);
        GameEventBus.Unsubscribe<SpawnMinionEvent>(OnSpawnMinion);
        MinionSpawnButton.forcedMinionKey = null;
        MinionSpawnButton.forcedTile = null;
        if (Canvas != null)
            Canvas.HideFinger();
    }

    void OnSpawnMinion(SpawnMinionEvent e)
    {
        StartCoroutine(CoComplete());
    }

    // 소환 버튼 처리(칸 배치·가격 갱신)가 끝난 다음 프레임에 넘어간다
    IEnumerator CoComplete()
    {
        SetBlock(true);
        if (Canvas != null)
            Canvas.HideFinger();
        yield return null;
        Complete();
    }
}
