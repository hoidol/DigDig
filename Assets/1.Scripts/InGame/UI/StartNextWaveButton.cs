using UnityEngine;
using TMPro;

// 전투 웨이브 스킵 버튼
// CAN_START_NEXT_PHASE_TIME에 누르면 SKIP_WAVE_MAX_GOLD 획득
// 이후 점점 줄어들어 웨이브 종료 SKIP_WAVE_END_BEFORE초 전에 0원 + 비활성화
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
        if (!(GameManager.Instance.currentWave is BattleWave battleWave) || GameManager.Instance.isBreak || !battleWave.CanSkip())
        {
            if (childObject.activeSelf)
                childObject.SetActive(false);
            return;
        }

        if (!childObject.activeSelf)
            childObject.SetActive(true);

        float elapsed = GameManager.Instance.waveTimer;
        float startTime = GameSetting.CAN_START_NEXT_PHASE_TIME;
        float zeroTime = GameManager.Instance.waveTime - GameSetting.SKIP_WAVE_END_BEFORE;
        float t = Mathf.InverseLerp(startTime, zeroTime, elapsed);
        SetGold(Mathf.CeilToInt(Mathf.Lerp(GameSetting.SKIP_WAVE_MAX_GOLD, 0, t)));
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
        if (!childObject.activeSelf)
            return;

        if (curGold > 0)
        {
            Character.Instance.AddCoin(curGold);
            CoinText.Instantiate().SetCoinText(Character.Instance.transform.position, $"+{curGold}");
        }
        childObject.SetActive(false);
        GameManager.Instance.SkipWave();
    }
}
