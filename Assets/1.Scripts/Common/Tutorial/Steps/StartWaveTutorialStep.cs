// wave 시작 블록: 튜토리얼 도중에 게임(wave 0)을 시작한다 → 이후 블록은 적이 나오는 중에 진행
// 이 블록이 없으면 튜토리얼이 끝난 뒤에 wave 시작. 페이드가 끝난 뒤에 둘 것 (WaitFadeEnd 뒤)
public class StartWaveTutorialStep : TutorialStep
{
    protected override void OnEnter()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        Complete();
    }
}
