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

    void Start()
    {
        GameEventBus.Subscribe<ApproachingOrdealStartEvent>(OnApproachingOrdealStartEvent);
        GameEventBus.Subscribe<OrdealStartEvent>(OnOrdealStartEvent);
    }
    List<OrdealPoint> ordealPoints = new();
    void OnApproachingOrdealStartEvent(ApproachingOrdealStartEvent e)
    {
        for (int i = 0; i < ordealPoints.Count; i++)
        {
            ordealPoints[i].OnDestroy();

        }
        ordealPoints.Clear();
        List<OrdealData> pickedOrdealDatas = new();
        for (int i = 0; i < e.ordealProgressData.ordealLevels.Length; i++)
        {
            List<OrdealData> sameLevelOrdealDatas = ordealDatas.Where(oData => oData.level == e.ordealProgressData.ordealLevels[i]).ToList();
            pickedOrdealDatas.Add(sameLevelOrdealDatas[Random.Range(0, sameLevelOrdealDatas.Count)]);
        }

        ShowCanvasThenHide($"{e.ordealProgressData.clearCount + 1}번째 시련", "시련 중 한곳으로 다가가세요.", 2).Forget();
        Vector2 playerPos = Player.Instance.transform.position;
        float baseAngle = Random.Range(0f, 360f);
        int spawnCount = pickedOrdealDatas.Count;
        float angleStep = 360f / spawnCount;
        float offsetRange = 50f / spawnCount;

        for (int i = 0; i < spawnCount; i++)
        {
            float angle = baseAngle + i * angleStep + Random.Range(-offsetRange, offsetRange);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 pos = playerPos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * DISTANCE;
            SpawnOrdealPoint(pickedOrdealDatas[i], pos);
        }
    }


    public void SpawnOrdealPoint(OrdealData data, Vector2 pos)
    {
        OrdealPoint ordealPoint = null;
        if (data.isHard)
        {
            ordealPoint = Instantiate(hardOrdealPointPrefab);
        }
        else
        {
            ordealPoint = Instantiate(ordealPointPrefab);
        }
        ordealPoint.transform.position = pos;
        ordealPoint.Spawn(data);
        ordealPoints.Add(ordealPoint);
    }


    void OnOrdealStartEvent(OrdealStartEvent e)
    {
        ShowCanvasThenHide($"시련을 극복하라", $"{e.ordealData.MissionInfo()}", 2).Forget();
        //시련 종류의 시련 처리 컴포넌트 StartOrdeal() 함수 호출
        Ordeal ordeal = GetOrdeal(e.ordealData.type);

        ordeal.StartOrdeal(e.ordealData);
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

    public Vector2 CalcSpawnPosition(Vector2 howFarRange)
    {
        Vector2 playerPos = Player.Instance.transform.position;
        return playerPos + Random.insideUnitCircle.normalized * DISTANCE;
    }


}