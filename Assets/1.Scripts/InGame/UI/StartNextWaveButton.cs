using UnityEngine;
using TMPro;

// 다음 웨이브 바로 시작 버튼
// 현재 wave 시작 후 SKIP_ENABLE_TIME초가 지나면 표시, 누르면 바로 다음 wave 시작 (이전 wave 남은 소환과 겹쳐 진행)
public class StartNextWaveButton : MonoBehaviour
{
    public GameObject childObject;

    void Start()
    {
        childObject.SetActive(false);
    }

    void Update()
    {
        if (!GameManager.Instance.CanSkipWave())
        {
            if (childObject.activeSelf)
                childObject.SetActive(false);
            return;
        }

        if (!childObject.activeSelf)
            childObject.SetActive(true);

    }


    public void OnClickedSkip()
    {
        if (!childObject.activeSelf || !GameManager.Instance.CanSkipWave())
            return;

        childObject.SetActive(false);
        GameManager.Instance.StartNextWave();
    }
}
