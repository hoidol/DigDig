using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class EventManager : MonoSingleton<EventManager>
{

    public EventObject[] prefabs;
    private class ConditionState
    {
        public EventData data;
        public int spawnCount;
        public float lastSpawnTime = float.NegativeInfinity;
        public bool distanceTriggered; // DistanceReached: 한 번 발동하면 true
        public bool hpArmed = true;    // LowHp: HP threshold 위로 회복되면 재무장
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
        GameEventBus.Subscribe<UndergroundStartEvent>(OnUndergroundStart);
        GameEventBus.Subscribe<WaveEndEvent>(OnWaveEnd);
        PollLoop().Forget();
    }

    private void OnDestroy()
    {
        GameEventBus.Unsubscribe<UndergroundStartEvent>(OnUndergroundStart);
        GameEventBus.Unsubscribe<WaveEndEvent>(OnWaveEnd);
    }

    private void OnUndergroundStart(UndergroundStartEvent e)
    {
        conditionStates.Clear();
        if (e.undergroundData.eventDatas == null) return;

        foreach (var condition in e.undergroundData.eventDatas)
        {
            if (condition.trigger == EventSpawnTrigger.Time)
            {
                WaitTimeEvent(condition).Forget();
            }
            else
            {
                conditionStates.Add(new ConditionState { data = condition });
            }
        }
    }

    private void OnWaveEnd(WaveEndEvent e)
    {
        // 조건 데이터 기반 WaveEnd 트리거
        foreach (var state in conditionStates)
        {
            if (state.data.trigger == EventSpawnTrigger.WaveEnd)
                TrySpawn(state);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            Spawn(EventType.NormalMerchant);
    }
#endif

    async UniTaskVoid PollLoop()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (!token.IsCancellationRequested)
        {
            CheckPollingConditions();
            await UniTask.Delay(2000, cancellationToken: token);
        }
    }
    async UniTaskVoid WaitTimeEvent(EventData eData)
    {
        float time = Random.Range(eData.time.x, eData.time.y);
        var token = this.GetCancellationTokenOnDestroy();
        await UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: token);
        Debug.Log($"Start Event eData.eventType {eData.eventType} spawnMode  {eData.spawnMode}");
        if (token.IsCancellationRequested) return;
        Spawn(eData.ResolveEventType(), eData.spawnMode);
    }

    private void CheckPollingConditions()
    {
        if (conditionStates.Count == 0) return;


        float playerDistance = Player.Instance.maxDistance;
        float hpRate = Player.Instance.curHp / Player.Instance.statMgr.MaxHp;

        foreach (var state in conditionStates)
        {
            switch (state.data.trigger)
            {
                case EventSpawnTrigger.LowHp:
                    if (hpRate > state.data.hpThreshold)
                        state.hpArmed = true;
                    else if (state.hpArmed)
                    {
                        state.hpArmed = false;
                        TrySpawn(state);
                    }
                    break;

                case EventSpawnTrigger.DistanceReached:
                    if (!state.distanceTriggered && playerDistance >= state.data.triggerDistance)
                    {
                        state.distanceTriggered = true;
                        TrySpawn(state);
                    }
                    break;
            }
        }
    }

    private void TrySpawn(ConditionState state)
    {
        EventData d = state.data;

        if (d.maxSpawnCount > 0 && state.spawnCount >= d.maxSpawnCount) return;
        if (d.cooldown > 0 && Time.time - state.lastSpawnTime < d.cooldown) return;
        if (Player.Instance.maxDistance < d.minDistanceFromOrigin) return;

        EventObject eventObject = Spawn(d.ResolveEventType(), d.spawnMode);
        if (eventObject != null)
        {
            state.spawnCount++;
            state.lastSpawnTime = Time.time;
        }
    }

    // EventObjectType에 해당하는 NPC를 계산된 위치에 스폰
    public EventObject Spawn(EventType type, EventSpawnMode spawnMode = EventSpawnMode.DirectionBased)
    {
        if (!prefabMap.TryGetValue(type, out EventObject prefab))
        {
            Debug.LogWarning($"EventObjectManager: {type} 프리팹이 등록되지 않았습니다.");
            return null;
        }

        if (!TryGetValidSpawnPosition(spawnMode, out Vector2 spawnPos))
        {
            Debug.Log($"EventObjectManager: {type} 스폰 취소 - 유효한 위치를 찾지 못했습니다.");
            return null;
        }

        EventObject eventObject = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeEventObjects.Add(eventObject);
        eventObject.OnAppear(spawnPos);
        return eventObject;
    }

    public void RemoveEventObject(EventObject eventObject)
    {
        activeEventObjects.Remove(eventObject);
    }

    private bool TryGetValidSpawnPosition(EventSpawnMode spawnMode, out Vector2 result)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 pos = CalcSpawnPosition(spawnMode);
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

    public Vector2 CalcSpawnPosition(EventSpawnMode spawnMode = EventSpawnMode.DirectionBased)
    {
        Vector2 playerPos = Player.Instance.transform.position;

        if (spawnMode == EventSpawnMode.NearPlayer)
            return playerPos + Random.insideUnitCircle.normalized * 3.5f;

        // 중앙→플레이어 방향에서 -30~30도 랜덤 회전, 플레이어로부터 6~8 거리
        Vector2 dir = playerPos == Vector2.zero ? Vector2.right : playerPos.normalized;
        float angle = Random.Range(-30f, 30f);
        float rad = angle * Mathf.Deg2Rad;
        Vector2 rotatedDir = new Vector2(
            dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
            dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad)
        );
        return playerPos + rotatedDir * Random.Range(15f, 18f);
    }
}

public enum EventSpawnMode
{
    DirectionBased, // 중앙→플레이어 방향 기준 ±30도, 6~8 거리
    NearPlayer,     // 플레이어 기준 360도 랜덤 방향, 3.5f 거리
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
    RandomMerchant,
    RandomBox,
    OrdealPoint,

}
