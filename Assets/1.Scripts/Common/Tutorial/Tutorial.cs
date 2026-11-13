using System;
using System.Collections.Generic;
using UnityEngine;

// 튜토리얼 종류. 저장 데이터에 번호로 들어갈 수 있으므로 뒤에만 추가
public enum TutorialType
{
    None = 0,
    FirstPlay = 1, // 처음 실행: 로비 대신 바로 InGame → 킹 이동 → 첫 소환
}

// 튜토리얼 하나(종류 = tutorialType). 프리팹으로 두고 TutorialManager가 필요할 때 생성한다
// 하위 TutorialStep을 순서대로 진행 (자식 오브젝트 순서 = 진행 순서)
// 튜토리얼 전용 Canvas·안내 오브젝트는 이 오브젝트의 자식으로 둔다 → 끝나면 통째로 삭제
public class Tutorial : MonoBehaviour
{
    public TutorialType tutorialType;
    public List<TutorialStep> steps = new List<TutorialStep>();

    [Header("튜토리얼 동안 숨길 씬 오브젝트 이름 또는 경로 (끝나면 다시 켬). 중간에 보이게 하려면 SetActive 블록")]
    public List<string> hideObjectNames = new List<string>();

    public TutorialCanvas canvas; // 비워 두면 자식에서 찾음

    public int StepIdx { get; private set; } = -1;
    public bool IsPlaying { get; private set; }

    public TutorialStep CurrentStep => StepIdx >= 0 && StepIdx < steps.Count ? steps[StepIdx] : null;

    public event Action<Tutorial> onEnded;

    readonly HashSet<Vector2Int> reservedTiles = new HashSet<Vector2Int>(); // 단계가 쓰는 타일 (막힌 칸이면 시작할 때 풀어 둠)
    readonly List<GameObject> hiddenObjects = new List<GameObject>();

    void Awake()
    {
        if (steps.Count == 0)
            GetComponentsInChildren(true, steps);
        if (canvas == null)
            canvas = GetComponentInChildren<TutorialCanvas>(true);

        for (int i = 0; i < steps.Count; i++)
        {
            steps[i].Init(this);
        }

        // 시작 전에는 튜토리얼 화면(블록·대화창)이 게임을 가리지 않게 꺼 둔다
        if (canvas != null)
            canvas.gameObject.SetActive(false);

    }

    void OnDestroy()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnTutorialDestroyed(this);
    }

    // 이 튜토리얼을 시작할 수 있는지 (조건은 종류마다 재정의)
    public virtual bool CanBegin()
    {
        return !IsPlaying && steps.Count > 0;
    }

    // 단계가 Prepare에서 호출: 이 칸을 튜토리얼에서 쓴다 (막힌 칸이면 시작할 때 다른 칸과 바꿔 풀어 둠)
    public void ReserveTile(Vector2Int tileIndex)
    {
        reservedTiles.Add(tileIndex);
    }

    public void Begin()
    {
        if (!CanBegin())
            return;

        IsPlaying = true;
        StepIdx = -1;

        reservedTiles.Clear();
        for (int i = 0; i < steps.Count; i++)
        {
            steps[i].Prepare();
        }
        if (reservedTiles.Count > 0 && TileManager.Instance != null)
            TileManager.Instance.EnsureUsable(reservedTiles);

        hiddenObjects.Clear();
        foreach (GameObject go in TutorialSceneFinder.FindAll(hideObjectNames))
        {
            if (!go.activeSelf)
                continue;
            go.SetActive(false);
            hiddenObjects.Add(go);
        }

        // 튜토리얼 동안 화면은 기본으로 막아 두고, 직접 조작하는 단계(킹 이동·소환)만 그 단계 동안 푼다
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
            canvas.HideDialogue();
            canvas.SetBlock(true);
        }

        OnBegin();
        NextStep();
    }

    // 현재 단계를 끝내고 다음 단계로. 마지막 단계였으면 튜토리얼 종료
    public void NextStep()
    {
        if (!IsPlaying)
            return;

        TutorialStep prevStep = CurrentStep;
        if (prevStep != null)
            prevStep.Exit();

        StepIdx++;
        if (StepIdx >= steps.Count)
        {
            End();
            return;
        }

        CurrentStep.Enter();
    }

    public void End()
    {
        if (!IsPlaying)
            return;

        // 중간에 끝내는 경우 진행 중이던 단계 정리
        TutorialStep step = CurrentStep;
        if (step != null && step.IsActive)
            step.Exit();

        IsPlaying = false;
        StepIdx = -1;

        for (int i = 0; i < hiddenObjects.Count; i++)
        {
            if (hiddenObjects[i] != null)
                hiddenObjects[i].SetActive(true);
        }
        hiddenObjects.Clear();

        OnEnd();
        onEnded?.Invoke(this);

        // 끝난 튜토리얼은 다시 쓰지 않는다 → 하위 오브젝트(전용 Canvas 포함)까지 통째로 삭제
        Destroy(gameObject);
    }

    protected virtual void OnBegin() { }
    protected virtual void OnEnd() { }
}
