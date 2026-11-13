// 소환 버튼 보이기 블록: Tutorial.hideObjectNames로 숨긴 소환 버튼(MinionSpawnButton)을 이 시점부터 보이게 한다. 바로 다음 단계로
// 씬 오브젝트를 직접 연결하지 않아도 되어 튜토리얼 프리팹에 그대로 둘 수 있다 (다른 오브젝트는 SetActive 블록)
public class ShowSpawnButtonTutorialStep : TutorialStep
{
    protected override void OnEnter()
    {
        MinionSpawnButton button = FindFirstObjectByType<MinionSpawnButton>(UnityEngine.FindObjectsInactive.Include);
        if (button != null)
            button.gameObject.SetActive(true);
        Complete();
    }
}
