using UnityEngine;

// 오브젝트 켜기/끄기 블록: 숨겨 둔 UI를 이 시점부터 보이게 하거나, 더 숨길 때 사용. 바로 다음 단계로
// 튜토리얼 프리팹 안의 오브젝트는 targets로 직접, 씬 오브젝트는 targetNames(이름 또는 경로)로 지정
public class SetActiveTutorialStep : TutorialStep
{
    public GameObject[] targets;
    [Header("씬 오브젝트 이름 또는 경로")]
    public string[] targetNames;
    public bool isActive = true;

    protected override void OnEnter()
    {
        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                    targets[i].SetActive(isActive);
            }
        }
        foreach (GameObject go in TutorialSceneFinder.FindAll(targetNames))
            go.SetActive(isActive);
        Complete();
    }
}
