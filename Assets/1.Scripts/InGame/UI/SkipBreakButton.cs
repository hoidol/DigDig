using UnityEngine;

// 휴식 시간 스킵 버튼 (보상 없음)
public class SkipBreakButton : MonoBehaviour
{
    public GameObject childObject;

    void Start()
    {
        childObject.SetActive(false);
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
    }


    void OnWaveStartEvent(WaveStartEvent e)
    {
        childObject.SetActive(false);
    }

    public void OnClickedSkip()
    {
        if (!childObject.activeSelf)
            return;

        childObject.SetActive(false);
    }
}
