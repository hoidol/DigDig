using System.Collections;
using UnityEngine;

// 드래그 블록 공통: 지정한 유닛만 드래그할 수 있고 지정한 칸에만 놓을 수 있다 (다른 칸에 놓으면 원위치)
// 이 단계 동안 화면 블록을 풀고, 유닛 칸 → 목표 칸으로 손가락 드래그 연출을 끝날 때까지 반복 (플레이어가 드래그하는 동안은 숨김)
public abstract class DragTutorialStep : TutorialStep
{
    [Header("목표 칸 표시 (월드 오브젝트, 없어도 됨). 단계 동안 목표 칸 위치에 켜짐")]
    public Transform targetMarker;

    protected ITileObject dragObject;
    protected Tile targetTile;
    bool isGuiding;

    public override void Prepare()
    {
        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);
    }

    // OnEnter에서 호출: obj를 target 칸으로 드래그하도록 안내 시작
    protected void BeginGuide(ITileObject obj, Tile target)
    {
        dragObject = obj;
        targetTile = target;
        isGuiding = true;

        if (targetMarker != null)
        {
            targetMarker.position = targetTile.transform.position;
            targetMarker.gameObject.SetActive(true);
        }

        MinionFieldController.Instance.dragFilter = CanDrag;
        MinionFieldController.Instance.dropFilter = CanDrop;
        GameEventBus.Subscribe<DragStartEvent>(OnDragStart);
        GameEventBus.Subscribe<DragEndEvent>(OnDragEnd);
        SetBlock(false);
        PlayFinger();
    }

    protected override void OnExit()
    {
        StopAllCoroutines();
        EndGuide();
    }

    void EndGuide()
    {
        if (!isGuiding)
            return;
        isGuiding = false;

        SetBlock(true);
        GameEventBus.Unsubscribe<DragStartEvent>(OnDragStart);
        GameEventBus.Unsubscribe<DragEndEvent>(OnDragEnd);
        if (MinionFieldController.Instance != null)
        {
            MinionFieldController.Instance.dragFilter = null;
            MinionFieldController.Instance.dropFilter = null;
        }
        if (Canvas != null)
            Canvas.HideFinger();
        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);
    }

    bool CanDrag(ITileObject tileObject) => tileObject == dragObject;
    bool CanDrop(ITileObject tileObject, Tile tile) => tile == targetTile;

    void PlayFinger()
    {
        if (Canvas == null)
            return;
        Tile fromTile = TileManager.Instance.GetTile(dragObject);
        if (fromTile != null)
            Canvas.PlayFingerDrag(fromTile.transform, targetTile.transform);
    }

    void OnDragStart(DragStartEvent e)
    {
        if (Canvas != null)
            Canvas.HideFinger();
    }

    void OnDragEnd(DragEndEvent e)
    {
        if (IsDropDone())
        {
            StartCoroutine(CoComplete());
            return;
        }
        if (isGuiding)
            PlayFinger(); // 목표 칸에 놓지 못했으면 다시 안내
    }

    // 드롭 직후 판정: true면 단계 완료 (합성처럼 결과가 나중에 나오는 경우는 false를 돌려주고 직접 CompleteAfterDrop 호출)
    protected abstract bool IsDropDone();

    protected void CompleteAfterDrop()
    {
        if (IsActive)
            StartCoroutine(CoComplete());
    }

    // 드롭 처리(놓은 칸 선택)가 끝난 다음 프레임에 선택을 풀고 넘어간다
    IEnumerator CoComplete()
    {
        EndGuide();
        yield return null;
        MinionFieldController.Instance.DeselectTile();
        Complete();
    }
}
