using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 킹 이동 블록: 플레이어가 킹슬라임을 드래그해 지정한 칸에 놓으면 다음 단계
// 놓을 때까지 손가락 드래그 연출이 계속 나온다 (DragTutorialStep)
// 안내하는 동안 게임 시간을 startTimeScale에서 slowDuration초(실제 시간)에 걸쳐 0까지 줄인다.
// 드래그·손가락·목표 칸 표시·킹 애니메이션은 실제 시간으로 움직여 원래 속도로 보인다. 단계가 끝나면 이전 속도로 복구
public class MoveKingTutorialStep : DragTutorialStep
{
    [Header("킹을 옮겨야 하는 칸. 전체 6*6 기준 (x: 왼쪽 0 → 오른쪽 5, y: 맨 위 0 → 맨 아래 5)")]
    public Vector2Int targetTileIndex = new Vector2Int(0, 2);

    [Header("안내 중 게임 슬로우: startTimeScale → 0 (slowDuration초, 실제 시간)")]
    public float startTimeScale = 0.3f;
    public float slowDuration = 5f;

    bool isSlowing;
    float prevTimeScale;
    readonly List<(Animator animator, AnimatorUpdateMode mode)> unscaledAnimators = new();

    public override void Prepare()
    {
        base.Prepare();
        Tutorial.ReserveTile(targetTileIndex);
    }

    protected override void OnEnter()
    {
        Tile tile = TileManager.Instance.GetTile(targetTileIndex);
        if (tile == null)
        {
            Debug.LogError($"MoveKingTutorialStep 없는 칸 {targetTileIndex}");
            Complete();
            return;
        }
        if (tile.tileObject is KingSlime)
        {
            Complete();
            return;
        }

        BeginGuide(KingSlime.Instance, tile);
        BeginSlow();
    }

    protected override void OnExit()
    {
        base.OnExit(); // StopAllCoroutines 포함 → 슬로우 코루틴도 멈춤
        EndSlow();
    }

    protected override bool IsDropDone() => targetTile.tileObject is KingSlime;

    void BeginSlow()
    {
        isSlowing = true;
        prevTimeScale = Time.timeScale;

        // 슬로우 중에도 원래 속도로 보여야 하는 것: 킹(드래그 대상), 목표 칸 표시
        SetUnscaledAnimators(KingSlime.Instance);
        SetUnscaledAnimators(targetMarker);

        StartCoroutine(CoSlow());
    }

    IEnumerator CoSlow()
    {
        for (float t = 0; t < slowDuration; t += Time.unscaledDeltaTime)
        {
            Time.timeScale = Mathf.Lerp(startTimeScale, 0f, t / slowDuration);
            yield return null;
        }
        Time.timeScale = 0f;
    }

    void EndSlow()
    {
        if (!isSlowing)
            return;
        isSlowing = false;

        Time.timeScale = prevTimeScale;
        foreach (var (animator, mode) in unscaledAnimators)
        {
            if (animator != null)
                animator.updateMode = mode;
        }
        unscaledAnimators.Clear();
    }

    void SetUnscaledAnimators(Component root)
    {
        if (root == null)
            return;

        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
        {
            unscaledAnimators.Add((animator, animator.updateMode));
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }
}
