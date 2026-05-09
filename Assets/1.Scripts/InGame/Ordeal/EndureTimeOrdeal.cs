using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

public class EndureTimeOrdeal : Ordeal
{
    public TMP_Text remainTimeText;
    public Image timeImage;
    public float RemainTime { get; private set; }
    public float TotalTime { get; private set; }

    public override void StartOrdeal(OrdealData ordealData)
    {
        base.StartOrdeal(ordealData);
        remainTimeText.text = $"{TotalTime}초";
        TotalTime = float.Parse(ordealData.data);
        RemainTime = TotalTime;
        RunTimer().Forget();
    }

    async UniTaskVoid RunTimer()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (RemainTime > 0f)
        {
            RemainTime -= Time.deltaTime;
            float ratio = Mathf.Clamp01(RemainTime / TotalTime);
            if (timeImage != null) timeImage.fillAmount = ratio;
            if (remainTimeText != null) remainTimeText.text = Mathf.CeilToInt(RemainTime).ToString();
            await UniTask.Yield(cancellationToken: token);
        }
        RemainTime = 0f;
        EndOrdeal();
    }

    public override void EndOrdeal()
    {
        enemyPattern.EndPattern();
        base.EndOrdeal();
    }
}
