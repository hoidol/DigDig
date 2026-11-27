using System;
using System.Collections.Generic;
using UnityEngine;

// (2026-11-23) 인게임 미션 진행. 한 판 동안 InGameMissionSetting의 미션을 모두 동시에 진행, 미션마다 한 번만 완료
// 보상 = 완료 후 처음 오는 브레이크타임 주사위 코인 (Slime.bonusDiceCoinNextBreak → MinionOfferSystem이 브레이크타임에 지급)
//  - wave 클리어 순간 완료(WaveEndEvent가 BreakTimeStartEvent보다 먼저) → 바로 그 브레이크타임
//  - 브레이크타임 중 배치로 완료 → 다음 브레이크타임
// 판정 시점: wave 시작 / wave 클리어 / 필드 상황은 0.2초(실제 시간)마다
public class InGameMissionSystem : MonoBehaviour
{
    public enum State { Progress, Complete, Failed }

    public class Entry
    {
        public Mission mission;
        public State state;
    }

    public static InGameMissionSystem Instance { get; private set; }
    public static event Action Changed; // 미션 상태가 바뀜 (UI 갱신)

    public readonly List<Entry> entries = new();
    public MissionContext Context => ctx;

    readonly MissionContext ctx = new();
    float pollTimer;
    const float POLL_INTERVAL = 0.2f;

    public static void Begin()
    {
        if (Instance != null)
            return;
        new GameObject(nameof(InGameMissionSystem)).AddComponent<InGameMissionSystem>();
    }

    void Awake()
    {
        Instance = this;
        foreach (Mission m in InGameMissionSetting.GetMissions())
            entries.Add(new Entry { mission = m, state = State.Progress });
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStart);
        GameEventBus.Subscribe<WaveEndEvent>(OnWaveEnd);
        Changed?.Invoke();
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);
        GameEventBus.Unsubscribe<WaveEndEvent>(OnWaveEnd);
        if (Instance == this)
            Instance = null;
    }

    void OnWaveStart(WaveStartEvent e)
    {
        CollectField();
        ctx.waveIdx = e.waveIdx;
        ctx.minionCountAtWaveStart = ctx.fieldMinions.Count;
        Check();
    }

    void OnWaveEnd(WaveEndEvent e)
    {
        CollectField();
        ctx.isWaveCleared = e.waveIdx == ctx.waveIdx;
        Check();
        ctx.isWaveCleared = false;
    }

    void Update()
    {
        pollTimer += Time.unscaledDeltaTime;
        if (pollTimer < POLL_INTERVAL)
            return;
        pollTimer = 0;
        CollectField();
        Check();
    }

    // 타일 위 미니언 (슬라임 제외)
    void CollectField()
    {
        ctx.fieldMinions.Clear();
        TileManager tileManager = TileManager.Instance;
        if (tileManager == null || tileManager.tiles == null)
            return;
        foreach (Tile tile in tileManager.tiles)
        {
            if (tile != null && tile.tileObject is Minion minion && minion != null && minion.gameObject.activeInHierarchy)
                ctx.fieldMinions.Add(new MissionContext.FieldMinion(minion.key, minion.mergeLevel));
        }
    }

    void Check()
    {
        bool changed = false;
        foreach (Entry entry in entries)
        {
            if (entry.state != State.Progress)
                continue;
            if (entry.mission.IsComplete(ctx))
            {
                entry.state = State.Complete;
                GiveReward(entry.mission);
                changed = true;
            }
            else if (entry.mission.IsFailed(ctx))
            {
                entry.state = State.Failed;
                changed = true;
            }
        }
        if (changed)
            Changed?.Invoke();
    }

    void GiveReward(Mission mission)
    {
        if (mission.rewardDiceCoin > 0 && Slime.Instance != null)
            Slime.Instance.AddRewardDiceCoin(mission.rewardDiceCoin); // (2026-11-25) 브레이크타임 중 완료면 즉시
        ToastCanvas.Toast(string.Format(TranslateManager.GetText("Mission_Complete"), mission.GetTitle(), mission.rewardDiceCoin));
        SoundManager.Play(SFXType.RewardGet);
    }
}
