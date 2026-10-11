using UnityEngine;
using TMPro;

public class SkipBreakButton : MonoBehaviour
{
    public GameObject childObject;
    //5초 이내 누르면 curGold 만큼 획득
    //다음 서서히 골드 줄어듦(3초 전에 0원되어 childObject 비활성화)
    public int initGold = 5;
    public int rewardGold = 0;
    public int curGold;
    public TMP_Text goldText;

    public float fullGoldTime = 5f;   // 이 시간까지는 rewardGold 전부 지급
    public float hideBeforeEnd = 3f;  // 쉬는시간 종료 n초 전에 0원 + 비활성화

    void Start()
    {
        childObject.SetActive(false);
        GameEventBus.Subscribe<BreakStartEvent>(OnBreakStartEvent);
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
    }

    void OnBreakStartEvent(BreakStartEvent e)
    {
        rewardGold = initGold + (e.phaseIdx * 2);
        SetGold(rewardGold);
        childObject.SetActive(true);
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {
        childObject.SetActive(false);
    }

    void Update()
    {
        if (!childObject.activeSelf)
            return;

        float timer = GameManager.Instance.breakTimer;
        float zeroTime = GameSetting.BREAK_TIME - hideBeforeEnd;
        if (timer >= zeroTime)
        {
            SetGold(0);
            childObject.SetActive(false);
            return;
        }

        if (timer <= fullGoldTime)
        {
            SetGold(rewardGold);
            return;
        }

        float t = (timer - fullGoldTime) / (zeroTime - fullGoldTime);
        SetGold(Mathf.CeilToInt(Mathf.Lerp(rewardGold, 0, t)));
    }

    void SetGold(int gold)
    {
        curGold = gold;
        goldText.text = $"+{curGold}";
    }

    public void OnClickedSkip()
    {
        if (!childObject.activeSelf)
            return;

        Character.Instance.AddCoin(curGold);
        CoinText.Instantiate().SetCoinText(Character.Instance.transform.position, $"+{curGold}");
        childObject.SetActive(false);
        GameManager.Instance.SkipBreak();
    }
}
