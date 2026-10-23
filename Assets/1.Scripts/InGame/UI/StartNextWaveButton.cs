using UnityEngine;
using TMPro;

// 전투 웨이브 스킵 버튼
// 전투 웨이브 시작 SKIP_SHOW_DELAY초 후부터 스킵 가능 (휴식 중, 보스 직전/보스 wave엔 숨김), 스킵하면 SKIP_WAVE_GOLD 고정 획득
public class StartNextWaveButton : MonoBehaviour
{
    public GameObject childObject;
    public int curGold;
    public TMP_Text goldText;

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

        SetGold(GameSetting.SKIP_WAVE_GOLD);
    }

    void SetGold(int gold)
    {
        if (curGold == gold)
            return;
        curGold = gold;
        goldText.text = $"+{curGold}";
    }

    public void OnClickedSkip()
    {
        if (!childObject.activeSelf || !GameManager.Instance.CanSkipWave())
            return;

        if (curGold > 0)
        {
            Character.Instance.AddCoin(curGold);
            CoinText.Instantiate().SetCoinText(Character.Instance.transform.position, $"+{curGold}");
        }
        childObject.SetActive(false);
        GameManager.Instance.StartNextWave();
    }
}
