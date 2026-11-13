using System.Collections;
using UnityEngine;

// 대기 블록: 화면을 막은 채 seconds초(실시간) 기다린 뒤 다음 단계
public class WaitTutorialStep : TutorialStep
{
    public float seconds = 1f;

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
        yield return new WaitForSecondsRealtime(seconds);
        Complete();
    }
}
