using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
public class GameManager : MonoSingleton<GameManager>
{
    public int killEnemyCount { get; private set; }
    public StageData stageData;

    public bool isPlaying;
    EnemySpawnerContainer enemySpawnerContainer;
    // public WaveTimer waveTimer;
    protected void Awake()
    {
        GameEventBus.Clear();
    }

    void OnDestroy()
    {
        AuthoritySystem.End();
    }

    [field: SerializeField]
    public float gameTimer
    {
        get;
        private set;
    }
    public float mainWaveTimer;
    // public bool isBreak; // 웨이브 종료 후 다음 웨이브까지 휴식 중
    // public float breakTimer;
    public int waveIdx;
    // public DefenceType waveDefenceType; // 이번 wave에 소환될 적의 방어 타입 (wave 시작 시 DefenceWaveTypes로 결정)
    public int minionSpawnCount;
    public int drawItemCount;
    public MinionChancePity minionChancePity = new MinionChancePity(); // 슬라임 확률 연속 실패 보정 (한 판 동안 유지)

    async void Start()
    {
        await UniTask.WhenAll(
            StageManager.Instance.LoadTask,
            BulletManager.Instance.LoadTask,
            ItemManager.Instance.LoadTask,
            EnemyManager.Instance.LoadTask,
            AuthorityManager.Instance.LoadTask,
            MinionManager.Instance.LoadTask
        );

        PlayInGameBGM();
        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
        GameEventBus.Subscribe<KingSlimeHpChangedEvent>(OnCharacterHpChangedEvent);
        GameEventBus.Subscribe<SpawnMinionEvent>(OnSpawnMinieEvent);

        Debug.Log($"[GameManager] 스테이지 {UserDataManager.stageOrder} / {UserDataManager.difficultyType}");
        stageData = StageManager.Instance.GetCurrentStageData();
        if (stageData == null)
        {
            // 선택한 스테이지·난이도 데이터가 없으면 스테이지 1 같은 난이도 → 스테이지 1 Normal
            Debug.LogWarning($"[GameManager] 스테이지 데이터 없음 ({UserDataManager.stageOrder}_{UserDataManager.difficultyType}) → 스테이지 1로");
            UserDataManager.stageOrder = StageData.FIRST_STAGE_ORDER;
            stageData = StageManager.Instance.GetCurrentStageData()
                ?? StageManager.Instance.GetStageData(StageData.FIRST_STAGE_ORDER, DifficultyType.Normal);
        }
        stageData.Init();
        enemySpawnerContainer = Instantiate(stageData.enemySpawnerContainerPrefab);
        enemySpawnerContainer.Init();

        // 타일 위치는 GridLayoutGroup이 정함. 로드가 바로 끝나면 첫 레이아웃 계산 전이라 먼저 강제 계산
        Canvas.ForceUpdateCanvases();
        TileManager.Instance.AssignTileIndices();

        // 캐릭터를 임의의 빈 타일 위에 배치
        Tile characterTile = TileManager.Instance.GetRandomEmptyTile();
        if (characterTile != null)
            characterTile.SetTileObject(KingSlime.Instance);
        else
            Debug.LogWarning("GameManager 캐릭터를 올릴 빈 타일 없음");

        MinionFieldController.Instance.minionMaxCount += (int)KingSlime.Instance.statMgr.GetValue(StatType.MaxMinionCount);

        // 권능: 로비에서 해금된 묶음마다 고른 것 전부 (타일·킹 배치 후, 게임 시작 전)
        AuthoritySystem.Begin(UserDataManager.Instance.userAuthorityManager.GetActiveAuthorityKeys());

        // wave 카드 소환 (골드 소환 대신). wave 시작 이벤트보다 먼저 만들어 첫 wave 카드도 받게 함
        if (GameSetting.USE_WAVE_MINION_OFFER)
            MinionOfferSystem.Create();

        GameEventBus.Publish(new StartGameEvent(stageData));
        // 기존 Start() 로직 (이벤트 구독, FadeIn → StartGame) 이어서 진행
        // 첫 플레이 튜토리얼: 페이드 전에 시작해 킹 위치 지정 같은 준비가 화면이 밝아지기 전에 끝나게 한다
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.TryBegin(TutorialType.FirstPlay);

        FadeCanvs.Instance.FadeIn(stageData.Title, () =>
        {
            // PlayInGameBGM(); // InGame 씬에서 바로 시작하면 SoundManager.Start(로비 BGM)가 위 호출보다 늦을 수 있어 한 번 더 (이미 재생 중이면 무시)
            StartGameAfterTutorial().Forget();
        });
    }

    static void PlayInGameBGM()
    {
        SoundManager soundManager = SoundManager.Instance;
        if (soundManager != null)
            soundManager.PlayBGM(BGMType.InGame);
    }

    // 튜토리얼이 진행 중이면 끝나거나 튜토리얼이 wave 시작을 요청할 때(StartWaveTutorialStep)까지 대기
    async UniTaskVoid StartGameAfterTutorial()
    {
        while (!isGameStarted && TutorialManager.Instance != null && TutorialManager.Instance.IsPlaying)
        {
            if (await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken).SuppressCancellationThrow())
                return;
        }
        StartGame();
    }

    bool isGameStarted;
    public bool IsGameStarted => isGameStarted;

    // wave 0 시작. 한 판에 한 번만 (튜토리얼이 먼저 불렀으면 페이드 뒤 호출은 무시)
    public void StartGame()
    {
        if (isGameStarted)
            return;
        isGameStarted = true;
        DamageMeterSystem.Clear(); // 피해 집계는 한 판 누적

        waveIdx = 0;
        gameTimer = 0;
        isPlaying = true;

        KingSlime.Instance.AddCoin((int)KingSlime.Instance.statMgr.GetValue(StatType.StartGold));
        if (GameSetting.USE_WAVE_MINION_OFFER)
            StartBreakTime(0); // 첫 wave 전에도 카드를 받고 배치 → Next
        else
            StartWave(waveIdx);
        ProcessWaveTimer().Forget();
    }

    #region wave 사이 브레이크타임 (카드 소환 모드, 2026-11-19)
    // wave의 적을 모두 소환했고 필드에 적이 없으면(처치 또는 경로 끝) wave 클리어 → 브레이크타임: 카드 3장 + Next 버튼
    // Next(EndBreakTime)를 눌러야 다음 wave 시작. 시간으로는 넘어가지 않음
    public bool isBreakTime;
    public int breakTimeNextWaveIdx; // Next를 누르면 시작할 wave

    void StartBreakTime(int nextWaveIdx)
    {
        isBreakTime = true;
        breakTimeNextWaveIdx = nextWaveIdx;
        waveIdx = nextWaveIdx;
        mainWaveTimer = 0;
        GameEventBus.Publish(new BreakTimeStartEvent(nextWaveIdx));
    }

    public void EndBreakTime()
    {
        if (!isPlaying || !isBreakTime)
            return;
        isBreakTime = false;
        StartWave(breakTimeNextWaveIdx);
    }

    // 현재 wave를 다 소환했고 살아 있는 적이 없음 (보스 wave는 보스 처치 시 isSpawnEnd)
    bool IsCurrentWaveCleared()
    {
        Wave wave = CurrentWave;
        return wave != null && wave.isSpawnEnd && EnemyFieldController.Instance.ActiveEnemyCount == 0;
    }

    // (2026-11-19) wave 시간 초과: 필드에 남은 적 1마리당 킹 피해 1 → 남은 적은 보상 없이 제거, 남은 소환도 취소 → 킹이 살아 있으면 wave 클리어(브레이크타임)
    // 보스 wave는 시간 제한 없음 (보스 처치까지)
    void OnWaveTimeOver()
    {
        Wave wave = CurrentWave;
        if (wave == null || wave.waveType == WaveType.Boss)
            return;
        int remainCount = EnemyFieldController.Instance.ActiveEnemyCount;
        wave.isSpawnEnd = true;
        if (!wave.isEnd)
            wave.EndWave(); // 남은 소환 취소
        EnemyFieldController.Instance.ClearAllEnemies();
        if (remainCount > 0)
            KingSlime.Instance.TakeDamage(new EnemyDamageData { damage = remainCount * TIME_OVER_DAMAGE_PER_ENEMY });
    }
    public const int TIME_OVER_DAMAGE_PER_ENEMY = 1;

    void OnWaveCleared()
    {
        Wave wave = CurrentWave;
        if (!wave.isEnd)
            wave.EndWave();
        EndWave(wave.waveIdx); // 이미 종료 처리됐으면 무시됨 (부활 시작 wave 기준)
        if (IsLastWave(wave.waveIdx))
        {
            isCleared = true;
            EndGame(true);
            return;
        }
        StartBreakTime(wave.waveIdx + 1);
    }
    #endregion
    public List<Wave> accWaves = new List<Wave>();//누적 웨이브
    public List<int> clearWaveIdxes = new List<int>();
    // mainWaveTimer를 WAVE_TIME → 0으로 낮춤 (StartWave에서 다시 WAVE_TIME으로 리셋), 0이 되면 다음 wave 시작
    // 게임 정지(사망 등) 중에는 멈춤. 보스 wave는 시간으로 넘어가지 않고 0에서 대기 → 보스 처치 시 BossWave가 다음 wave 시작
    public async UniTaskVoid ProcessWaveTimer()
    {
        var token = destroyCancellationToken;
        while (true)
        {
            if (await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow())
                return;
            if (!isPlaying)
                continue;

            // 카드 소환 모드: 적을 다 처리하면 브레이크타임. WAVE_TIME(90초)이 지나면 남은 적 수만큼 킹 피해 후 정리
            if (GameSetting.USE_WAVE_MINION_OFFER)
            {
                if (mainWaveTimer > 0)
                {
                    mainWaveTimer = Mathf.Max(0, mainWaveTimer - Time.deltaTime);
                    if (mainWaveTimer <= 0 && !isBreakTime)
                        OnWaveTimeOver();
                }
                if (isPlaying && !isBreakTime && IsCurrentWaveCleared())
                    OnWaveCleared();
                continue;
            }

            if (mainWaveTimer <= 0)
                continue;

            mainWaveTimer -= Time.deltaTime;
            if (mainWaveTimer <= 0)
            {
                mainWaveTimer = 0;
                if (CurrentWave == null || CurrentWave.waveType != WaveType.Boss)
                    StartNextWave();
            }
        }
    }

    // (2026-11-22) 테스트(미니언 피해 측정) 전용: 켜면 엘리트·보스 wave도 일반 전투로 진행. 기본 false
    public static bool forceBattleWaveForTest;

    public void StartWave(int waveIdx)
    {
        this.waveIdx = waveIdx;
        mainWaveTimer = GameSetting.WAVE_TIME;

        WaveType waveType = GetWaveType(waveIdx);
        if (forceBattleWaveForTest)
            waveType = WaveType.Battle; // 피해 측정: 엘리트·보스 없이
        DefenceType waveDefenceType = GetWaveDefenceType(waveIdx);

        // 웨이브 진행/종료는 각 Wave가 담당 (종료 시 Wave.MoveNextWave → EndWave)
        Debug.Log($"GameManager StartWave {waveIdx}");
        Wave wave = enemySpawnerContainer.GetWave(waveType);
        wave.gameObject.SetActive(true);
        accWaves.Add(wave);
        wave.StartWave(waveIdx, waveDefenceType);
        GameEventBus.Publish(new WaveStartEvent(waveIdx, waveType));
    }

    // wave 1~LAST_WAVE(60). wave 번호(waveIdx + 1)가 BOSS_WAVE_INTERVAL 배수면 보스, ELITE_WAVE_INTERVAL 배수면 엘리트, 나머지는 일반 전투
    public static WaveType GetWaveType(int waveIdx)
    {
        int waveNum = waveIdx + 1;
        if (waveNum % GameSetting.BOSS_WAVE_INTERVAL == 0)
            return WaveType.Boss;
        if (waveNum % GameSetting.ELITE_WAVE_INTERVAL == 0)
            return WaveType.Elite;
        return WaveType.Battle;
    }

    // 일반 적 방어 타입 = 지금 스테이지 StageData.defenceWaveTypes를 wave 1부터 순서대로 반복 (스테이지 데이터가 없으면 기본 순서)
    public static DefenceType GetWaveDefenceType(int waveIdx)
    {
        StageData stageData = Instance != null ? Instance.stageData : null;
        return StageData.GetDefenceType(stageData != null ? stageData.defenceWaveTypes : null, waveIdx);
    }

    // 현재 진행 중인 wave (가장 마지막에 시작한 wave)
    public Wave CurrentWave => accWaves.Count > 0 ? accWaves[^1] : null;

    // 타이머 종료 시 호출 → endWaveIdx wave만 종료(스폰 중지), 현재 wave였으면 휴식 후 다음 wave 시작
    public void EndWave(int endWaveIdx)
    {
        if (!isPlaying)
            return;

        // 이미 종료된 wave면 리턴 (중복 호출로 wave를 두 칸 건너뛰는 것 방지)
        if (clearWaveIdxes.Contains(endWaveIdx))
            return;

        clearWaveIdxes.Add(endWaveIdx);
        KingSlime.Instance.AddCoin((int)KingSlime.Instance.statMgr.GetValue(StatType.WaveClearGold));
        GameEventBus.Publish(new WaveEndEvent(endWaveIdx));
    }


    // 다음 wave 시작. 이전 wave는 끄지 않으므로 남은 적은 다음 wave와 겹쳐서 진행
    // 마지막 wave(LAST_WAVE, 보스)를 넘기면 다음 wave 대신 게임 클리어
    public void StartNextWave()
    {
        if (!isPlaying)
            return;
        if (IsLastWave(waveIdx))
        {
            isCleared = true;
            EndGame(true);
            return;
        }
        StartWave(waveIdx + 1);
    }

    public bool isCleared; // 마지막 wave까지 깬 판 (결과 화면 보상·클리어 기록)
    public static bool IsLastWave(int waveIdx) => waveIdx + 1 >= GameSetting.LAST_WAVE;


    // 스킵 가능(다음 wave 바로 시작): 현재 wave의 적이 모두 소환된 뒤 (Wave.isSpawnEnd, 2026-11-15)
    // 필드에 남은 적은 그대로 진행. 보스 wave는 안 나옴 (보스 처치 시 자동으로 다음 wave)
    public bool CanSkipWave()
    {
        if (!isPlaying || CurrentWave == null)
            return false;
        if (GameSetting.USE_WAVE_MINION_OFFER)
            return false; // 카드 소환 모드는 wave 클리어 후 Next 버튼
        return CanSkipWave(CurrentWave.waveType, CurrentWave.isSpawnEnd);
    }

    public static bool CanSkipWave(WaveType waveType, bool isSpawnEnd)
    {
        return waveType != WaveType.Boss && isSpawnEnd;
    }

    void Update()
    {
        if (!isPlaying)
            return;

        gameTimer += Time.deltaTime;
    }


    const string ACCEPT_REVIEW_KEY = "ACCEPT_REVIEW_KEY";

    public void EndGame(bool clear)
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        // 도전 횟수는 결과 화면이 나올 때 (부활하면 한 판으로 이어지므로 여기서 세지 않음)

        if (!clear)
        {
            FailCanvas.Instance.OpenCanvas();
        }
        else
        {
            GameEventBus.Publish(new ClearStageEvent(stageData.key));
            int accept = PlayerPrefs.GetInt(ACCEPT_REVIEW_KEY, 0);
            if (accept == 0)
            {
                // 리뷰 요청: Normal보다 높은 난이도를 클리어했을 때 (이전: 3번째 스테이지 이상)
                if (stageData.difficulty != DifficultyType.Normal)
                {
                    YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("review_title"), TranslateManager.GetText("review_body"), (accept) =>
                    {
                        if (accept)
                        {
                            Review review = new Review();
                            review.Request();

                            PlayerPrefs.SetInt(ACCEPT_REVIEW_KEY, 1);
                        }
                    });

                }
            }
            ResultCanvas.Instance.OpenCanvas();
        }
    }

    // 부활: 필드 정리 후 체력을 가득 채우고, 마지막으로 종료된 wave 다음 wave부터 다시 시작
    // ex) 0 종료, 1 스킵(진행 중), 2 진행 중 사망 → 1, 2 제거 후 1부터 다시
    public void Resume()
    {
        if (accWaves.Count > 0)
        {
            waveIdx = clearWaveIdxes.Count > 0 ? clearWaveIdxes[^1] + 1 : 0;
        }

        for (int i = 0; i < accWaves.Count; i++)
        {
            if (accWaves[i].waveIdx >= waveIdx)
            {
                Wave w = accWaves[i];
                accWaves[i].EndWave();
                accWaves.Remove(w);
                Destroy(w.gameObject);
                i--;
            }
        }

        EnemyFieldController.Instance.ClearAllEnemies();

        KingSlime.Instance.AddHp(KingSlime.Instance.health.MaxHp);
        isPlaying = true;
        GameEventBus.Publish(new ResumeEvent());

        // 카드 소환 모드: 그 wave의 브레이크타임로 (카드는 이미 받은 wave면 다시 안 줌)
        if (GameSetting.USE_WAVE_MINION_OFFER)
            StartBreakTime(waveIdx);
        else
            StartWave(waveIdx);
    }

    void OnCharacterHpChangedEvent(KingSlimeHpChangedEvent e)
    {
        if (e.curHp <= 0)
        {
            EndGame(false);
        }
    }

    void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        killEnemyCount++;
    }

    public void OnSpawnMinieEvent(SpawnMinionEvent e)
    {
        minionSpawnCount++;
    }
    public Wave GetWave(int wIdx)
    {
        for (int i = 0; i < accWaves.Count; i++)
        {
            if (accWaves[i].waveIdx == wIdx)
                return accWaves[i];
        }
        return null;
    }
}

public class StartGameEvent
{
    public StageData stageData;
    public StartGameEvent(StageData data)
    {
        stageData = data;
    }
}

public class StartBossEvent
{

}

// 카드 소환 모드: wave 클리어(또는 게임 시작·부활) 후 다음 wave 브레이크타임 시작
public class BreakTimeStartEvent
{
    public int nextWaveIdx;
    public BreakTimeStartEvent(int nextWaveIdx) { this.nextWaveIdx = nextWaveIdx; }
}

public class WaveStartEvent
{
    public int waveIdx;
    public WaveType waveType;
    public WaveStartEvent(int p, WaveType waveType)
    {
        waveIdx = p;
        this.waveType = waveType;
    }
}
public class WaveEndEvent
{
    public int waveIdx;
    public WaveType waveType;
    public WaveEndEvent(int p)
    {
        waveIdx = p;
    }
}
public class ClearStageEvent
{
    public string key;
    public ClearStageEvent(string key)
    {
        this.key = key;
    }
}

public class TryStageEvent
{
    public string key;
    public TryStageEvent(string key)
    {
        this.key = key;
    }
}

public class ResumeEvent
{

}

/*
성장2이 있어야되는 이유
뱀서에 각성같은거야... 필요해
그냥 되는게 있고 안되는게 있게 하자 마음 편하게 
모든 얘가 될 수는 없어
양분 슬라임은 없을 수 있는거야 
조커 슬라임도 없을 수 있는거야

- 불량 + 화염 : 폭주 슬라임 
- 라이트닝 + [?] : 토르 슬라임 : 
- 탄성 + 관통 : 새총 슬라임  : 화면 전체로 팅기는 
- 불량 : 궤도 슬라임 커지고 많아지고 
- 화염 얼음 : 
- 폭발 

불량
탄성
화염
궤도
관통
얼음
썬더
라이트닝
도파민
치료
폭발
방패 - 


*/