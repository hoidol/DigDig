using UnityEngine;
using TMPro;

// 다음 웨이브 바로 시작 버튼
// 현재 wave의 적이 모두 소환되면 표시, 누르면 바로 다음 wave 시작 (보스 wave는 안 나옴)
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
