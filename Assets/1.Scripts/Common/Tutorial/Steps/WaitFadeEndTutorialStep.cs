using System.Collections;

// 화면 페이드(FadeCanvs)가 끝날 때까지 기다리는 블록
// 튜토리얼은 화면이 밝아지기 전에 시작하므로, 킹 위치 지정 같은 준비 블록 뒤·첫 대화 앞에 둔다
public class WaitFadeEndTutorialStep : TutorialStep
{
    protected override void OnEnter()
    {
        StartCoroutine(CoWait());
    }

    protected override void OnExit()
    {
        StopAllCoroutines();
    }

    IEnumerator CoWait()
    {
        yield return null; // 페이드가 이 프레임에 시작되는 경우
        while (FadeCanvs.Instance != null && FadeCanvs.Instance.gameObject.activeInHierarchy)
            yield return null;
        Complete();
    }
}
