using UnityEngine;

// 튜토리얼 안에서 진행해야 할 단계 하나(블록). 단계별 동작은 상속해서 OnEnter/OnExit에 구현하고
// 단계 조건을 만족하면 Complete()를 호출해 다음 단계로 넘긴다
public abstract class TutorialStep : MonoBehaviour
{
    public Tutorial Tutorial { get; private set; }
    public bool IsActive { get; private set; }

    protected TutorialCanvas Canvas => Tutorial != null ? Tutorial.canvas : null;

    public void Init(Tutorial tutorial)
    {
        Tutorial = tutorial;
        IsActive = false;
    }

    // 튜토리얼 시작 직전에 모든 단계에서 한 번 호출 (쓸 타일 예약 등)
    public virtual void Prepare() { }

    public void Enter()
    {
        if (IsActive)
            return;

        IsActive = true;
        OnEnter();
    }

    public void Exit()
    {
        if (!IsActive)
            return;

        IsActive = false;
        OnExit();
    }

    // 이 단계 완료 → 다음 단계. 진행 중인 단계가 아니면 무시
    public void Complete()
    {
        if (!IsActive)
            return;

        Tutorial.NextStep();
    }

    // 화면 블록 켜기/끄기 (직접 조작하는 단계는 OnEnter에서 풀고 OnExit에서 다시 막는다)
    protected void SetBlock(bool isBlock)
    {
        if (Canvas != null)
            Canvas.SetBlock(isBlock);
    }

    // 단계 시작: 안내 표시, 완료 조건 감지 시작
    protected abstract void OnEnter();

    // 단계 종료: 안내 숨김, 감지 해제 (완료·중단 모두 호출)
    protected virtual void OnExit() { }
}
