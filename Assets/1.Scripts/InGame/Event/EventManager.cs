using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class EventManager : MonoSingleton<EventManager>
{

    public EventObject[] prefabs;
    public class ConditionState
    {
        public EventType eventType;
        public EventTrigger[] triggers;
        public float hpThreshold;
        public int ordealClearCount;
        public Vector2 distanceFromPlayer;
        public bool hpArmed;
    }

    float eventObjectMinDistance = 7f;
    [SerializeField] private int maxAttempts = 5;

    private Dictionary<EventType, EventObject> prefabMap;
    private List<ConditionState> conditionStates = new List<ConditionState>();
    private readonly List<EventObject> activeEventObjects = new();

    private void Awake()
    {
        prefabMap = new Dictionary<EventType, EventObject>();
        foreach (var entry in prefabs)
            prefabMap[entry.eventType] = entry;
    }

    private void Start()
    {
        GameEventBus.Subscribe<OrdealEndEvent>(OnOrdealEndEvent);
        GameEventBus.Subscribe<StartGameEvent>(OnStartGameEvent);

    }

    private void OnDestroy()
    {
        GameEventBus.Unsubscribe<OrdealEndEvent>(OnOrdealEndEvent);
        GameEventBus.Unsubscribe<StartGameEvent>(OnStartGameEvent);
    }

    private void OnStartGameEvent(StartGameEvent e)
    {
        conditionStates.Clear();
        //EventData[] eventDatas = Resources.LoadAll<EventData>("EventData");
        foreach (var eventData in e.stageData.eventDatas)
        {
            EventType? resolved = ResolveEventType(eventData);
            if (resolved == null) continue;

            conditionStates.Add(new ConditionState
            {
                eventType = resolved.Value,
                triggers = eventData.triggers,
                hpThreshold = eventData.hpThreshold,
                ordealClearCount = eventData.ordealClearCount,
                distanceFromPlayer = eventData.distanceFromPlayer,
            });
        }
    }

    // eventTypes가 여럿이면 chances 가중치로 하나 선택,
    // eventTypes가 하나이고 chances[0]이 있으면 그 확률로 등장 여부 결정
    private EventType? ResolveEventType(EventData data)
    {
        if (data.eventTypes == null || data.eventTypes.Length == 0) return null;

        if (data.eventTypes.Length == 1)
        {
            if (data.chances != null && data.chances.Length > 0 && data.chances[0] > 0)
                if (Random.Range(0f, 100f) >= data.chances[0]) return null;
            return data.eventTypes[0];
        }

        // 여러 타입: chances 가중치 합산 후 랜덤 선택
        float total = 0f;
        foreach (var c in data.chances) total += c;
        if (total <= 0f) return data.eventTypes[0];

        float roll = Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < data.eventTypes.Length; i++)
        {
            cumulative += i < data.chances.Length ? data.chances[i] : 0f;
            if (roll < cumulative) return data.eventTypes[i];
        }
        return data.eventTypes[^1];
    }

    private void OnOrdealEndEvent(OrdealEndEvent e)
    {
        foreach (var state in conditionStates)
        {
            if (AllTriggersSatisfied(state, e))
                TrySpawn(state);
        }
    }

    private bool AllTriggersSatisfied(ConditionState state, OrdealEndEvent e)
    {
        foreach (var trigger in state.triggers)
        {
            switch (trigger)
            {
                case EventTrigger.OrdealEnd:
                    if (state.ordealClearCount != e.ordealClearCount) return false;
                    break;
                case EventTrigger.LowHp:
                    float hpRate = Player.Instance.curHp / Player.Instance.statMgr.MaxHp;
                    if (hpRate > state.hpThreshold) return false;
                    break;
            }
        }
        return true;
    }


#if UNITY_EDITOR
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.N))
        //     Spawn(EventType.NormalMerchant);
    }
#endif



    private void TrySpawn(ConditionState state)
    {
        Spawn(state);
    }

    // EventObjectType에 해당하는 NPC를 계산된 위치에 스폰
    public EventObject Spawn(ConditionState state)
    {
        if (!prefabMap.TryGetValue(state.eventType, out EventObject prefab))
        {
            Debug.LogWarning($"EventObjectManager: {state.eventType} 프리팹이 등록되지 않았습니다.");
            return null;
        }

        if (!TryGetValidSpawnPosition(state.distanceFromPlayer, out Vector2 spawnPos))
        {
            Debug.Log($"EventObjectManager: {state.eventType} 스폰 취소 - 유효한 위치를 찾지 못했습니다.");
            return null;
        }

        EventObject eventObject = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeEventObjects.Add(eventObject);
        eventObject.Appear(spawnPos);
        return eventObject;
    }

    public void RemoveEventObject(EventObject eventObject)
    {
        activeEventObjects.Remove(eventObject);
    }

    private bool TryGetValidSpawnPosition(Vector2 howFarRange, out Vector2 result)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 pos = CalcSpawnPosition(howFarRange);
            if (IsFarFromAllEventObjects(pos))
            {
                result = pos;
                return true;
            }
        }
        result = Vector2.zero;
        return false;
    }

    private bool IsFarFromAllEventObjects(Vector2 pos)
    {
        foreach (var eventObject in activeEventObjects)
        {
            if (Vector2.Distance(pos, eventObject.transform.position) < eventObjectMinDistance)
            {
                // Debug.Log($"IsFarFromAllEventObjects pos {pos} 위치에 뭐 있음");
                return false;
            }
        }

        return true;
    }

    public Vector2 CalcSpawnPosition(Vector2 howFarRange)
    {
        Vector2 playerPos = Player.Instance.transform.position;
        return playerPos + Random.insideUnitCircle.normalized * Random.Range(howFarRange.x, howFarRange.y);
    }
}


public enum EventType
{
    NormalMerchant,       // 일반 상인 
    RareMerchant,  // 레어 상인 
    UniqueMerchant, // 유니크 상인 
    FallenAngel,    // 타락 천사 - 추가 능력치, 패널티
    Snake,          // 뱀 - 추가 능력치, 패널티
    LifeFountain,   // 생명 분수 - 체력 증가
    NormalBox,
    RareBox,
    UniqueBox,

}
