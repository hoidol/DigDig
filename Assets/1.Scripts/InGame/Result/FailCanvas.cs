using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

// 사망 시 열림: 광고 보고 부활(하루 MAX_DAILY_RESURRECTION_COUNT번) 또는 취소 → 결과 화면
public class FailCanvas : CanvasUI<FailCanvas>
{
    public const string DAILY_RESURRECTION_COUNT_KEY = "DAILY_RESURRECTION_COUNT"; // 오늘 남은 부활 횟수
    public const string DAILY_RESURRECTION_DATE_KEY = "DAILY_RESURRECTION_DATE";   // 남은 횟수를 기록한 날짜 (날짜가 바뀌면 다시 가득)
    public const int MAX_DAILY_RESURRECTION_COUNT = 5;
    public TMP_Text leftResurrectionText;
    public GameObject adRetryButton;

    bool isWatchingAd; // 광고 보는 동안 중복 클릭 방지

    // 오늘 남은 부활 횟수
    public static int LeftResurrectionCount
    {
        get
        {
            if (!DailyCheck.IsTodayPrefs(DAILY_RESURRECTION_DATE_KEY))
                return MAX_DAILY_RESURRECTION_COUNT;
            return PlayerPrefs.GetInt(DAILY_RESURRECTION_COUNT_KEY, MAX_DAILY_RESURRECTION_COUNT);
        }
        private set
        {
            DailyCheck.MarkTodayPrefs(DAILY_RESURRECTION_DATE_KEY);
            PlayerPrefs.SetInt(DAILY_RESURRECTION_COUNT_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        isWatchingAd = false;
        UpdateCanvas();
    }
    void UpdateCanvas()
    {
        int count = LeftResurrectionCount;
        leftResurrectionText.text = string.Format(TranslateManager.GetText("Resurrection_Daily"), count);

        adRetryButton.SetActive(count > 0);
    }

    // 광고 성공 시에만 횟수 차감 후 부활 (광고 제거 구매자는 AdManager.Show가 광고 없이 성공 처리)
    public void OnClickeResume()
    {
        if (isWatchingAd || LeftResurrectionCount <= 0)
            return;

        isWatchingAd = true;
        AdManager.Show("resurrection", AdType.RA, async (success) =>
        {
            await UniTask.SwitchToMainThread(); // 광고 SDK 콜백은 메인 스레드가 아닐 수 있음
            isWatchingAd = false;
            if (success)
                Resurrect();
            else
                UpdateCanvas();
        });
    }

    void Resurrect()
    {
        LeftResurrectionCount--;
        CloseCanvas();
        GameManager.Instance.Resume();
    }

    public void OnClickedCancel()
    {
        CloseCanvas();
        ResultCanvas.Instance.OpenCanvas();
    }
}
