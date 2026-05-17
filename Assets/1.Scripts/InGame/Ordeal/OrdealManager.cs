using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
public class OrdealManager : MonoSingleton<OrdealManager>
{
    public const float WAIT_TIME = 180;
    public OrdealData[] ordealDatas;
    public CanvasGroup canvasGroup;
    public TMP_Text titleText;
    public TMP_Text descText;
    public const float DISTANCE = 30;
    public OrdealPoint ordealPointPrefab;
    public OrdealPoint hardOrdealPointPrefab;
    public Ordeal[] ordeals;
    Ordeal GetOrdeal(OrdealType type)
    {
        foreach (Ordeal ordeal in ordeals)
            ordeal.gameObject.SetActive(false);

        return ordeals.FirstOrDefault(x => x.ordealType == type);
    }
    void Awake()
    {
        foreach (Ordeal ordeal in ordeals)
            ordeal.gameObject.SetActive(false);
        ordealDatas = Resources.LoadAll<OrdealData>("OrdealData");
    }

    List<OrdealPoint> ordealPoints = new();
    public void StartOrdeal(OrdealProgressData ordealProgressData)
    {
        for (int i = 0; i < ordealPoints.Count; i++)
        {
            ordealPoints[i].Destroy();

        }
        ordealPoints.Clear();
        List<OrdealData> sameLevelOrdealDatas = ordealDatas.Where(oData => oData.level == ordealProgressData.ordealLevel).ToList();
        OrdealData selectedOrdealData = sameLevelOrdealDatas[Random.Range(0, sameLevelOrdealDatas.Count)];
        ShowCanvasThenHide($"시련을 극복하라", $"{selectedOrdealData.MissionInfo()}", 2).Forget();
        //시련 종류의 시련 처리 컴포넌트 StartOrdeal() 함수 호출
        Ordeal ordeal = GetOrdeal(selectedOrdealData.type);
        ordeal.StartOrdeal(selectedOrdealData);
        GameEventBus.Publish(new OrdealStartEvent(selectedOrdealData, ordealProgressData));
    }
    async UniTaskVoid ShowCanvasThenHide(string title, string desc, float time)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);
        titleText.text = title;
        descText.text = desc;

        float elapsed = 0f;
        var token = this.GetCancellationTokenOnDestroy();
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed);
            await UniTask.Yield(cancellationToken: token);
        }

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
    }



}