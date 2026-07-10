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

        public int phaseIdx;
    }

    float eventObjectMinDistance = 5f; //EventObject 간 떨어진 거리
    [SerializeField] private int maxAttempts = 5;

    private Dictionary<EventType, EventObject> prefabMap;
    private List<ConditionState> conditionStates = new List<ConditionState>();
    private readonly List<EventObject> activeEventObjects = new();
    public int spawnCount;
    private void Awake()
    {
        prefabMap = new Dictionary<EventType, EventObject>();
        foreach (var entry in prefabs)
            prefabMap[entry.eventType] = entry;
    }

    private void Start()
    {
        GameEventBus.Subscribe<PhaseEndEvent>(OnPhaseEndEvent);
        GameEventBus.Subscribe<StartGameEvent>(OnStartGameEvent);
        GameEventBus.Subscribe<BossSpawnEvent>(OnBossSpawnEvent);
        GameEventBus.Subscribe<EliteSpawnEvent>(OnEliteSpawnEvent);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);


    }
    Enemy bossOrElite;
    void OnBossSpawnEvent(BossSpawnEvent e)
    {
        bossOrElite = e.boss;
    }
    void OnEliteSpawnEvent(EliteSpawnEvent e)
    {
        bossOrElite = e.eliteEnemy;
    }
    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if (e.enemy == bossOrElite)
        {
            bossOrElite = null;
        }
    }
    private void OnDestroy()
    {
        GameEventBus.Unsubscribe<PhaseEndEvent>(OnPhaseEndEvent);
        GameEventBus.Unsubscribe<StartGameEvent>(OnStartGameEvent);
    }
    List<EventRepeatSpawner> eventRepeatSpawners = new();
    private void OnStartGameEvent(StartGameEvent e)
    {
        spawnCount = 0;

        EventRepeatSpawner itemBoxSpawner = new EventRepeatSpawner(EventType.ItemBox, Random.Range(0, 1), 2, 5);
        eventRepeatSpawners.Add(itemBoxSpawner);

        // conditionStates.Clear();

        // foreach (var eventData in e.stageData.eventDatas)
        // {
        //     EventType? resolved = ResolveEventType(eventData);
        //     if (resolved == null) continue;

        //     conditionStates.Add(new ConditionState
        //     {
        //         eventType = resolved.Value,
        //         triggers = eventData.triggers,

        //         phaseIdx = eventData.phaseIdx,

        //     });
        // }
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

    private void OnPhaseEndEvent(PhaseEndEvent e)
    {
        foreach (var state in conditionStates)
        {
            if (AllTriggersSatisfied(state, e))
                Spawn(state.eventType);
        }
    }

    private bool AllTriggersSatisfied(ConditionState state, PhaseEndEvent e)
    {
        foreach (var trigger in state.triggers)
        {
            switch (trigger)
            {
                case EventTrigger.PhaseEnd:
                    if (state.phaseIdx != e.phaseIdx) return false;
                    break;
            }
        }
        return true;
    }


    private void Update()
    {
        for (int i = 0; i < eventRepeatSpawners.Count; i++)
        {
            eventRepeatSpawners[i].Update();
        }
    }



    // EventObjectType에 해당하는 NPC를 계산된 위치에 스폰
    public EventObject Spawn(EventType eventType)
    {
        if (!prefabMap.TryGetValue(eventType, out EventObject prefab))
        {
            Debug.LogWarning($"EventObjectManager: {eventType} 프리팹이 등록되지 않았습니다.");
            return null;
        }
        if (!TryGetValidSpawnPosition(out Vector2 pos))
        {
            Debug.LogWarning($"설치할 위치없음");
            return null;
        }

        //보스나 엘리트 상황에서 생성안됨
        if (bossOrElite != null)
            return null;


        EventObject eventObject = Instantiate(prefab, pos, Quaternion.identity);
        activeEventObjects.Add(eventObject);
        eventObject.Appear(pos);
        spawnCount++;
        return eventObject;
    }

    public void RemoveEventObject(EventObject eventObject)
    {
        activeEventObjects.Remove(eventObject);
    }

    private bool TryGetValidSpawnPosition(out Vector2 result)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 pos = CalcSpawnPosition();
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
                return false;
            }
        }

        return true;
    }

    public Vector2 CalcSpawnPosition()
    {
        float farDistance = Player.Instance.distanceMaxDistanceDestroiedStone + Random.Range(4, 5) + spawnCount + 1.5f;
        Vector2 playerPos = Player.Instance.transform.position;
        return MapManager.SnappedPosition(playerPos + Random.insideUnitCircle.normalized * farDistance);
    }
}

public enum EventType
{
    FallenAngel,    // 타락 천사 - 추가 능력치, 패널티
    Snake,          // 뱀 - 추가 능력치, 패널티
    LifeFountain,   // 생명 분수 - 체력 증가
    ItemBox,
    StatStone
}

public class EventRepeatSpawner
{
    public EventType eventType;
    public EventObject eventObject;
    public float repeatTime;
    public float afterDestroyTime;
    public float repeatTimer;
    public float afterDestroyTimer;
    public float startTimer;
    public EventRepeatSpawner(EventType type, float sTime, float rTime, float aDTime)
    {
        eventType = type;
        repeatTime = rTime;
        repeatTimer = rTime;
        afterDestroyTime = aDTime;
        startTimer = sTime;
    }

    public void Update()
    {
        if (startTimer > 0)
        {
            startTimer -= Time.deltaTime;

            if (startTimer <= 0)
            {
                repeatTimer = repeatTime;
            }
            return;
        }

        if (repeatTimer > 0)
            repeatTimer -= Time.deltaTime;


        if (repeatTimer <= 0 && eventObject == null)
        {
            Spawn();
        }
    }
    public void Spawn()
    {
        // Debug.Log($"EventRepeatSpawner Spawn() {eventType}");
        eventObject = EventManager.Instance.Spawn(eventType);
        repeatTimer = repeatTime;
    }
}