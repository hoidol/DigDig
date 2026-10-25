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

    [field: SerializeField]
    public float gameTimer
    {
        get;
        private set;
    }
    public float mainWaveTimer;
    public bool isBreak; // 웨이브 종료 후 다음 웨이브까지 휴식 중
    public float breakTimer;
    public int waveIdx;
    // public DefenceType waveDefenceType; // 이번 wave에 소환될 적의 방어 타입 (wave 시작 시 DefenceWaveTypes로 결정)
    public int slimeSpawnCount;
    public int drawItemCount;

    async void Start()
    {
        await UniTask.WhenAll(
            StageManager.Instance.LoadTask,
            BulletManager.Instance.LoadTask,
            ItemManager.Instance.LoadTask,
            EnemyManager.Instance.LoadTask,
            EquipmentManager.Instance.LoadTask,
            SlimeManager.Instance.LoadTask
        );

        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
        GameEventBus.Subscribe<CharacterHpChangedEvent>(OnCharacterHpChangedEvent);
        GameEventBus.Subscribe<SpawnSlimeEvent>(OnSpawnMinieEvent);

        Debug.Log($"UserManager.STAGE_KEY {UserDataManager.STAGE_KEY}");
        stageData = StageManager.Instance.GetStageData(UserDataManager.STAGE_KEY);
        stageData.Init();
        enemySpawnerContainer = Instantiate(stageData.enemySpawnerContainerPrefab);
        enemySpawnerContainer.Init();

        // 캐릭터를 임의의 빈 타일 위에 배치
        Tile characterTile = TileManager.Instance.GetRandomEmptyTile();
        if (characterTile != null)
            characterTile.SetTileObject(Character.Instance);
        else
            Debug.LogWarning("GameManager 캐릭터를 올릴 빈 타일 없음");

        GameEventBus.Publish(new StartGameEvent(stageData));
        // 기존 Start() 로직 (이벤트 구독, FadeIn → StartGame) 이어서 진행
        FadeCanvs.Instance.FadeIn(stageData.Title, () =>
        {
            StartGame();
        });
    }

    void StartGame()
    {
        waveIdx = 0;
        gameTimer = 0;
        isPlaying = true;

        StartWave(waveIdx);
        ProcessWaveTimer().Forget();
    }
    public List<Wave> accWaves = new List<Wave>();//누적 웨이브
    public List<int> clearWaveIdxes = new List<int>();
    // mainWaveTimer를 WAVE_TIME → 0으로 낮춤 (StartWave에서 다시 WAVE_TIME으로 리셋)
    // 휴식 중에는 breakTimer를 낮추고, 0이 되면 다음 wave 시작
    // 게임 정지(사망 등) 중에는 멈춤. 보스 wave는 EndWave에서 막히므로 0에서 대기
    public async UniTaskVoid ProcessWaveTimer()
    {
        var token = destroyCancellationToken;
        while (true)
        {
            if (await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow())
                return;
            if (!isPlaying)
                continue;

            if (isBreak)
            {
                breakTimer -= Time.deltaTime;
                if (breakTimer <= 0)
                {
                    isBreak = false;

                    GameEventBus.Publish(new BreakEndEvent(waveIdx+1));
                    StartNextWave();
                }
                continue;
            }

            if (mainWaveTimer <= 0)
                continue;

            mainWaveTimer -= Time.deltaTime;
            if (mainWaveTimer <= 0)
            {
                mainWaveTimer = 0;
            }
        }
    }

    public void StartWave(int waveIdx)
    {
        this.waveIdx = waveIdx;
        mainWaveTimer = GameSetting.WAVE_TIME;
        isBreak = false;

        WaveType waveType = stageData.waveDatas[waveIdx].waveType;
        DefenceType waveDefenceType = stageData.GetWaveDefenceType(waveIdx);

        // 웨이브 진행/종료는 각 Wave가 담당 (종료 시 Wave.MoveNextWave → EndWave)
        Debug.Log($"GameManager StartWave {waveIdx}");
        Wave wave = enemySpawnerContainer.GetWave(waveType);
        wave.gameObject.SetActive(true);
        accWaves.Add(wave);
        wave.StartWave(waveIdx, waveDefenceType);
        GameEventBus.Publish(new WaveStartEvent(waveIdx, waveType));
    }

    // 현재 진행 중인 wave (가장 마지막에 시작한 wave)
    // public Wave CurrentWave => accWaves.Count > 0 ? accWaves[accWaves.Count - 1] : null;

    // 타이머 종료 시 호출 → endWaveIdx wave만 종료(스폰 중지), 현재 wave였으면 휴식 후 다음 wave 시작
    public void EndWave(int endWaveIdx)
    {
        if (!isPlaying)
            return;

        // 이미 종료된 wave면 리턴 (중복 호출로 wave를 두 칸 건너뛰는 것 방지)
        if (clearWaveIdxes.Contains(endWaveIdx))
            return;

        clearWaveIdxes.Add(endWaveIdx);
        GameEventBus.Publish(new WaveEndEvent(endWaveIdx));

        // 스킵으로 이미 지나간 이전 wave가 끝난 경우엔 진행에 영향 없음
        if (endWaveIdx == waveIdx && waveIdx < GameSetting.BOSS_PHASE)
        {
            isBreak = true;
            breakTimer = GameSetting.BREAK_TIME;
            GameEventBus.Publish(new BreakStartEvent(endWaveIdx+1));
        }
    }


    // 전투 웨이브 스킵. 이전 wave는 끄지 않으므로 남은 스폰이 다음 wave와 겹쳐서 진행 (누적)
    public void StartNextWave()
    {
        if (waveIdx >= GameSetting.BOSS_PHASE)
            return;
        StartWave(waveIdx + 1);
    }

    // 스킵 가능: 전투 wave 시작 후 SKIP_SHOW_DELAY초부터 (휴식 중, 보스 직전 wave, 보스 wave 불가)
    public bool CanSkipWave()
    {
        return isPlaying && !isBreak && waveIdx < GameSetting.BOSS_PHASE - 1
            && GameSetting.WAVE_TIME - mainWaveTimer >= GameSetting.SKIP_SHOW_DELAY;
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
        UserDataManager.Instance.userStageManager.TryStage(stageData.key);
        GameEventBus.Publish(new TryStageEvent(stageData.key));

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
                StageData maxStageData = StageManager.Instance.GetStageData(UserDataManager.Instance.userStageManager.GetMaxStage(UserDataManager.difficultyType));
                if (maxStageData.order >= 2)
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
            ResultCanvas.Instance.OpenCanvas(true);
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

        EnemySpawner.Instance.ClearAllEnemies();

        Character.Instance.AddHp(Character.Instance.health.MaxHp);
        isPlaying = true;
        GameEventBus.Publish(new ResumeEvent());

        StartWave(waveIdx);
    }

    void OnCharacterHpChangedEvent(CharacterHpChangedEvent e)
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

    public void OnSpawnMinieEvent(SpawnSlimeEvent e)
    {
        slimeSpawnCount++;
    }
    public Wave GetWave(int wIdx)
    {
        for(int i = 0; i < accWaves.Count; i++)
        {
            if(accWaves[i].waveIdx == wIdx)
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

public class BreakStartEvent
{
    public int nextWaveIdx;
    public BreakStartEvent(int w)
    {
        nextWaveIdx = w;
    }
}
public class BreakEndEvent
{
    public int nextWaveIdx;
    public BreakEndEvent(int w)
    {
        nextWaveIdx = w;
    }
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