using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// (2026-11-25) 기기(모바일)용 인게임 테스트 패널
// BuildSetting.isTestMode일 때 InGame 씬(GameManager가 있는 씬)에 자동으로 생김 (테스트 도구라 씬에 두지 않고 실행 중 코드로 UI 생성)
// 화면 왼쪽 "TEST" 버튼 → 패널 열기, 패널 오른쪽 위 X → 닫기
// (2026-11-26) 위쪽 탭: [미니언 소환] [적 소환] [웨이브 전환] [적 체력] [용사] [기타]
//   웨이브 전환: 브레이크타임마다 판 상태를 저장(WaveSnapshotSystem) → [이전/다음 웨이브] = 그 wave 브레이크타임(레벨업·카드 사용 전)으로 복구
//               [정지] = 적 전부 제거 + 바로 다음 wave 브레이크타임 / [웨이브 시작] = 적이 나옴 / 번호 고르고 [이동] = 배치 그대로 그 wave로
//   wave 결과 요약: wave가 끝날 때마다 걸린 시간·처치·도착·킹이 받은 피해·피해 상위 미니언 (웨이브 전환 탭, 최근 5개)
//   적 체력: EnemyHp·AddHpRatioPerWave·MultiHpPerWave·CycleHpMuls 입력 → 적용(지금 판만, 씬을 나가면 원래대로) + wave별 체력 표시
// 동작은 에디터 Minion Test 창과 같은 GameTestUtil
public class MobileTestPanel : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryCreate();
    }

    static void OnSceneLoaded(Scene s, LoadSceneMode m) => TryCreate();

    static void TryCreate()
    {
        if (BuildSetting.Instance == null || !BuildSetting.Instance.isTestMode)
            return;
        if (FindFirstObjectByType<GameManager>() == null || FindFirstObjectByType<MobileTestPanel>() != null)
            return;
        new GameObject("MobileTestPanel").AddComponent<MobileTestPanel>();
    }

    TMP_FontAsset font;
    GameObject panel;
    readonly List<GameObject> pages = new();
    readonly List<Image> tabImages = new();
    int curTab;
    static readonly Color TAB_ON = new Color(0.95f, 0.6f, 0.15f, 1f);
    static readonly Color TAB_OFF = new Color(0.3f, 0.3f, 0.35f, 1f);

    // 미니언·용사·기타
    TMP_Text minionText, mergeText, evolveText, heroText, levelText, enhanceText, speedText, invincibleText, infoText;
    readonly TMP_Text[] skillTexts = new TMP_Text[HeroKingAbility.SKILL_COUNT];
    int minionIdx, mergeLevel, heroIdx, heroLevel = GameSetting.KING_MAX_LEVEL, speed = 1;
    bool evolve, enhance;
    bool invincible; // (2026-11-26) 기본 꺼짐 (켜져 있으면 킹 체력이 줄지 않음)
    static readonly int[] SPEEDS = { 1, 2, 4, 8 };

    // 적 소환
    static readonly EnemyType[] ENEMY_TYPES = { EnemyType.Normal, EnemyType.Elite, EnemyType.BonusElite, EnemyType.Boss };
    static readonly DefenceType[] DEFENCE_TYPES = { DefenceType.Light, DefenceType.Normal, DefenceType.Armored, DefenceType.Ethereal };
    static readonly int[] COUNTS = { 1, 5, 10, 20, 40 };
    int enemyTypeIdx, defenceIdx = -1, countIdx; // defenceIdx -1 = 지금 wave 방어 타입
    bool freezeEnemies;
    TMP_Text enemyTypeText, defenceText, countText, freezeText, enemyInfoText;

    // 웨이브 전환
    int jumpWave = 1; // 표시 번호
    TMP_Text waveInfoText, jumpText, noEliteText, summaryText;

    // wave 결과 요약 (진행 중인 wave 기록 → 브레이크타임이 오면 한 줄로)
    bool recording;
    int recWave, recKills, recLeaked;
    float recStartTime, recStartHp;
    double recStartKingDamage;
    readonly Dictionary<string, double> recStartDamage = new();
    readonly List<string> summaries = new();
    const int MAX_SUMMARIES = 5;

    // 적 체력
    TMP_InputField hpInput, addInput, mulInput, cycleInput;
    TMP_Text hpInfoText, hpWaveText;
    int hpWave = 1; // 표시 번호
    bool savedOriginal;
    float origHp, origAdd, origMul;
    float[] origCycle;

    void Start()
    {
        TMP_Text any = FindFirstObjectByType<TMP_Text>(FindObjectsInactive.Include);
        font = any != null ? any.font : TMP_Settings.defaultFontAsset;
        BuildUI();
        panel.SetActive(false);
        WaveSnapshotSystem.Begin(this);
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStart);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
        GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakStart);
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakStart);
        WaveSnapshotSystem.End();
        RevertHpData(); // 에디터에서 에셋이 바뀐 채로 남지 않게
        if (freezeEnemies)
            Enemy.ClearFreezeAll();
        GameManager.forceBattleWaveForTest = false;
    }

    void Update()
    {
        KingSlime king = KingSlime.Instance;
        if (king == null)
            return;
        if (invincible && king.health != null && king.health.curHp < king.MaxHp)
            king.health.curHp = king.MaxHp;
        if (speed > 1 && Time.timeScale > 0f && !Mathf.Approximately(Time.timeScale, speed))
            Time.timeScale = speed;
        if (freezeEnemies)
            Enemy.FreezeAll(1f);
        if (panel != null && panel.activeSelf)
            RefreshTexts();
    }

    #region 동작: 미니언·용사·기타
    List<string> Keys => GameTestUtil.MinionKeys();
    AbilityData[] Heroes => GameTestUtil.Heroes();
    HeroKingAbility CurrentHero => KingSlime.Instance != null ? KingSlime.Instance.abilityInventory.Get<HeroKingAbility>() : null;

    void SpawnMinion()
    {
        List<string> keys = Keys;
        if (keys.Count == 0)
            return;
        Minion m = GameTestUtil.SpawnMinion(keys[Mathf.Clamp(minionIdx, 0, keys.Count - 1)], mergeLevel, evolve);
        if (m == null)
            ToastCanvas.Toast("빈 칸이 없습니다");
    }

    void EquipHero()
    {
        AbilityData[] heroes = Heroes;
        if (heroes.Length == 0)
            return;
        HeroKingAbility hero = GameTestUtil.EquipHero(heroes[Mathf.Clamp(heroIdx, 0, heroes.Length - 1)], heroLevel, enhance);
        ToastCanvas.Toast(hero != null ? $"{hero.data.title} Lv.{heroLevel} 장착" : "용사 장착 실패");
    }

    void UseSkill(int idx)
    {
        HeroKingAbility hero = CurrentHero;
        if (hero == null)
        {
            ToastCanvas.Toast("용사를 먼저 장착하세요");
            return;
        }
        HeroKingAbility.UseResult r = GameTestUtil.UseHeroSkill(hero, idx);
        if (r != HeroKingAbility.UseResult.Used)
            ToastCanvas.Toast($"{hero.GetSkillName(idx)}: {r}");
    }

    void ToggleSpeed()
    {
        int i = Array.IndexOf(SPEEDS, speed);
        speed = SPEEDS[(i + 1) % SPEEDS.Length];
        Time.timeScale = speed;
    }

    void FullHeal()
    {
        KingSlime king = KingSlime.Instance;
        if (king != null && king.health != null)
            king.health.curHp = king.MaxHp;
    }
    #endregion

    #region 동작: 적 소환
    void SpawnEnemies()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
            return;
        EnemyType type = ENEMY_TYPES[enemyTypeIdx];
        DefenceType def = defenceIdx < 0 ? GameManager.GetWaveDefenceType(gm.waveIdx) : DEFENCE_TYPES[defenceIdx];
        int n = GameTestUtil.SpawnEnemies(type, def, COUNTS[countIdx]);
        if (n == 0)
            ToastCanvas.Toast("적 소환 실패 (프리팹 없음)");
    }

    void ToggleFreeze()
    {
        freezeEnemies = !freezeEnemies;
        if (!freezeEnemies)
            Enemy.ClearFreezeAll();
    }
    #endregion

    #region 동작: 웨이브 전환
    // 지금 기준 wave (waveIdx): 브레이크타임 = 다음에 시작할 wave, wave 중 = 진행 중인 wave
    static int CurrentWaveIdx
    {
        get
        {
            GameManager gm = GameManager.Instance;
            return gm == null ? 0 : gm.isBreakTime ? gm.breakTimeNextWaveIdx : gm.waveIdx;
        }
    }

    void StopWave()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
            return;
        if (!gm.StopWaveForTest())
            ToastCanvas.Toast("진행 중인 wave가 없습니다 (브레이크타임)");
    }

    void StartWave()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
            return;
        if (!gm.isBreakTime)
        {
            ToastCanvas.Toast("이미 wave 진행 중");
            return;
        }
        gm.EndBreakTime();
    }

    void PrevWave()
    {
        int target = CurrentWaveIdx - 1;
        if (target < 0)
        {
            ToastCanvas.Toast("첫 wave입니다");
            return;
        }
        if (!WaveSnapshotSystem.Restore(target))
            ToastCanvas.Toast($"Wave {target + 1} 저장 없음");
        else
            ToastCanvas.Toast($"Wave {target + 1} 브레이크타임으로 복구");
    }

    void NextWave()
    {
        int target = CurrentWaveIdx + 1;
        if (target >= GameSetting.LAST_WAVE)
        {
            ToastCanvas.Toast("마지막 wave입니다");
            return;
        }
        if (WaveSnapshotSystem.Restore(target))
        {
            ToastCanvas.Toast($"Wave {target + 1} 브레이크타임으로 복구");
            return;
        }
        // 저장이 없으면 지금 배치 그대로 그 wave 브레이크타임으로
        JumpTo(target);
        ToastCanvas.Toast($"Wave {target + 1} (저장 없음 → 지금 배치 그대로)");
    }

    // 배치 그대로 그 wave 브레이크타임으로 (카드·주사위 코인 새로 받음)
    void JumpTo(int waveIdx)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
            return;
        MinionOfferSystem.Instance?.ResetDealForTest();
        gm.GoToBreakTimeForTest(waveIdx);
    }

    void ToggleNoEliteBoss() => GameManager.forceBattleWaveForTest = !GameManager.forceBattleWaveForTest;
    #endregion

    #region wave 결과 요약
    void OnWaveStart(WaveStartEvent e)
    {
        KingSlime king = KingSlime.Instance;
        recording = true;
        recWave = e.waveIdx;
        recKills = recLeaked = 0;
        recStartTime = GameManager.Instance != null ? GameManager.Instance.gameTimer : 0f;
        recStartHp = king != null && king.health != null ? king.health.curHp : 0f;
        recStartKingDamage = DamageMeterSystem.KingTotalDamage;
        recStartDamage.Clear();
        foreach (string key in Keys)
            recStartDamage[key] = DamageMeterSystem.GetDamage(key);
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        if (!recording || e.enemy == null)
            return;
        if (e.enemy.CurHp <= 0)
            recKills++;
        else
            recLeaked++;
    }

    void OnBreakStart(BreakTimeStartEvent e)
    {
        if (!recording)
            return;
        recording = false;
        KingSlime king = KingSlime.Instance;
        float time = (GameManager.Instance != null ? GameManager.Instance.gameTimer : 0f) - recStartTime;
        float hpLost = king != null && king.health != null ? Mathf.Max(0f, recStartHp - king.health.curHp) : 0f;
        var top = recStartDamage.Keys
            .Select(k => (k, d: DamageMeterSystem.GetDamage(k) - recStartDamage[k]))
            .Where(x => x.d > 0).OrderByDescending(x => x.d).Take(3)
            .Select(x => $"{MinionName(x.k)} {x.d:N0}");
        double kingDmg = DamageMeterSystem.KingTotalDamage - recStartKingDamage;
        summaries.Insert(0, $"W{recWave + 1} {time:0}초 · 처치 {recKills} · 도착 {recLeaked} · 킹 -{hpLost:0}{(invincible ? "(무적)" : "")} · 킹 피해 {kingDmg:N0}"
            + (top.Any() ? $"\n   {string.Join(" / ", top)}" : ""));
        while (summaries.Count > MAX_SUMMARIES)
            summaries.RemoveAt(summaries.Count - 1);
    }

    static string MinionName(string key) =>
        MinionManager.Instance != null && MinionManager.Instance.minionDataDic.TryGetValue(key, out MinionData d) ? d.Title : key;
    #endregion

    #region 동작: 적 체력
    StageData Stage => GameManager.Instance != null ? GameManager.Instance.stageData : null;

    void SaveOriginalHp()
    {
        StageData s = Stage;
        if (savedOriginal || s == null)
            return;
        savedOriginal = true;
        origHp = s.enemyHp;
        origAdd = s.addHpRatioPerWave;
        origMul = s.multiHpPerWave;
        origCycle = s.cycleHpMuls != null ? (float[])s.cycleHpMuls.Clone() : null;
    }

    void ApplyHp()
    {
        StageData s = Stage;
        if (s == null)
            return;
        SaveOriginalHp();
        if (float.TryParse(hpInput.text, out float hp)) s.enemyHp = hp;
        if (float.TryParse(addInput.text, out float add)) s.addHpRatioPerWave = add;
        if (float.TryParse(mulInput.text, out float mul)) s.multiHpPerWave = mul;
        s.cycleHpMuls = string.IsNullOrWhiteSpace(cycleInput.text) ? null : s.ParseFloatList(cycleInput.text.Trim(), "CycleHpMuls");
        ToastCanvas.Toast("적 체력 수치 적용 (새로 나오는 적부터)");
        FillHpInputs();
    }

    void RevertHpData()
    {
        StageData s = Stage;
        if (!savedOriginal || s == null)
            return;
        s.enemyHp = origHp;
        s.addHpRatioPerWave = origAdd;
        s.multiHpPerWave = origMul;
        s.cycleHpMuls = origCycle != null ? (float[])origCycle.Clone() : null;
        savedOriginal = false;
    }

    void ResetHp()
    {
        RevertHpData();
        FillHpInputs();
        ToastCanvas.Toast("원래 수치로 되돌림");
    }

    void FillHpInputs()
    {
        StageData s = Stage;
        if (s == null || hpInput == null)
            return;
        hpInput.text = s.enemyHp.ToString("0.###");
        addInput.text = s.addHpRatioPerWave.ToString("0.###");
        mulInput.text = s.multiHpPerWave.ToString("0.####");
        cycleInput.text = s.cycleHpMuls != null ? string.Join("/", s.cycleHpMuls.Select(v => v.ToString("0.###"))) : "";
    }

    string GetHpInfo()
    {
        StageData s = Stage;
        if (s == null)
            return "스테이지 데이터 없음";
        int w = Mathf.Clamp(hpWave, 1, GameSetting.LAST_WAVE) - 1;
        float normal = GameTestUtil.GetEnemyHp(s, w, EnemyType.Normal);
        float elite = GameTestUtil.GetEnemyHp(s, w, EnemyType.Elite);
        float boss = GameTestUtil.GetEnemyHp(s, w, EnemyType.Boss);
        WaveType type = GameManager.GetWaveType(w);
        float total = normal * GameSetting.WAVE_ENEMY_COUNT + (type == WaveType.Elite ? elite : 0f) + (type == WaveType.Boss ? boss : 0f);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<color=#FFD94D>Wave {w + 1}</color> ({type}, 일반 적 방어 {DefenceInfo.Title(GameManager.GetWaveDefenceType(w))})");
        sb.AppendLine($"일반 {normal:N0} · 엘리트 {elite:N0} · 보스 {boss:N0}");
        sb.AppendLine($"wave 총 체력 약 {total:N0} (일반 {GameSetting.WAVE_ENEMY_COUNT}마리{(type == WaveType.Elite ? " + 엘리트" : type == WaveType.Boss ? " + 보스" : "")})");
        sb.Append("<size=80%>일반 적: ");
        int[] samples = { 1, 5, 10, 15, 20, 25, 30, 40, 50 };
        sb.Append(string.Join(" · ", samples.Where(n => n <= GameSetting.LAST_WAVE).Select(n => $"W{n} {GameTestUtil.GetEnemyHp(s, n - 1, EnemyType.Normal):N0}")));
        sb.Append("</size>");
        return sb.ToString();
    }
    #endregion

    void RefreshTexts()
    {
        GameManager gm = GameManager.Instance;
        KingSlime king = KingSlime.Instance;
        // 미니언
        List<string> keys = Keys;
        if (keys.Count > 0)
        {
            minionIdx = (minionIdx % keys.Count + keys.Count) % keys.Count;
            MinionData d = MinionManager.Instance.minionDataDic[keys[minionIdx]];
            minionText.text = $"{d.Title} ({d.key})";
        }
        mergeText.text = $"머지 Lv.{mergeLevel + 1}";
        evolveText.text = evolve ? "진화 ON" : "진화 OFF";
        // 용사
        AbilityData[] heroes = Heroes;
        if (heroes.Length > 0)
        {
            heroIdx = (heroIdx % heroes.Length + heroes.Length) % heroes.Length;
            heroText.text = heroes[heroIdx].title;
        }
        levelText.text = $"킹 Lv.{heroLevel}";
        enhanceText.text = enhance ? "스킬 강화 ON" : "스킬 강화 OFF";
        HeroKingAbility hero = CurrentHero;
        for (int i = 0; i < skillTexts.Length; i++)
            skillTexts[i].text = hero != null ? $"{i + 1}. {hero.GetSkillName(i)}{(hero.IsSkillUnlocked(i) ? "" : " (잠김)")}" : $"스킬 {i + 1}";
        // 기타
        speedText.text = $"x{speed}";
        invincibleText.text = invincible ? "무적 ON" : "무적 OFF";
        // 적 소환
        enemyTypeText.text = ENEMY_TYPES[enemyTypeIdx].ToString();
        defenceText.text = ENEMY_TYPES[enemyTypeIdx] != EnemyType.Normal ? "방어: 중갑(고정)"
            : defenceIdx < 0 ? $"방어: wave 기본({DefenceInfo.Title(GameManager.GetWaveDefenceType(gm != null ? gm.waveIdx : 0))})" : $"방어: {DefenceInfo.Title(DEFENCE_TYPES[defenceIdx])}";
        countText.text = $"{COUNTS[countIdx]}마리";
        freezeText.text = freezeEnemies ? "적 정지 ON" : "적 정지 OFF";
        int enemyCount = EnemyFieldController.Instance != null ? EnemyFieldController.Instance.GetTargetEnemies().Count : 0;
        enemyInfoText.text = $"필드 적 {enemyCount}마리 · 지금 wave 일반 적 체력 {GameTestUtil.GetEnemyHp(Stage, gm != null ? gm.waveIdx : 0, EnemyType.Normal):N0}";
        // 웨이브 전환
        jumpText.text = $"Wave {jumpWave}";
        noEliteText.text = GameManager.forceBattleWaveForTest ? "엘리트·보스 없이 ON" : "엘리트·보스 없이 OFF";
        string saved = string.Join(",", WaveSnapshotSystem.SavedWaves.Select(i => (i + 1).ToString()));
        waveInfoText.text = gm == null ? "" :
            (gm.isBreakTime ? $"지금: <color=#7CFF6B>Wave {gm.breakTimeNextWaveIdx + 1} 브레이크타임</color>" : $"지금: <color=#FF9A5C>Wave {gm.waveIdx + 1} 진행 중</color> (적 {enemyCount})")
            + $"\n저장된 wave: {(saved.Length > 0 ? saved : "없음")}";
        summaryText.text = summaries.Count > 0 ? "<color=#FFD94D>wave 결과</color>\n" + string.Join("\n", summaries) : "wave 결과: 아직 없음";
        // 적 체력
        hpWaveText.text = $"Wave {hpWave}";
        hpInfoText.text = GetHpInfo();
        // 아래 공통 정보
        infoText.text = king == null ? "" :
            $"킹 체력 {(king.health != null ? king.health.curHp : 0):0}/{king.MaxHp:0} · Lv.{king.lv} · 주사위 {king.diceCoin}"
            + (hero != null ? $" · {hero.data.title} 에너지 {hero.Charges}/{hero.MaxCharges}" : "")
            + $"\n킹 피해 {DamageMeterSystem.KingTotalDamage:N0} · 미니언 피해 {DamageMeterSystem.TotalDamage:N0}";
    }

    #region UI (코드로 생성)
    void BuildUI()
    {
        var canvasGo = new GameObject("TestCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        RectTransform root = (RectTransform)canvasGo.transform;

        // 열기 버튼 (왼쪽 가운데)
        Button toggle = NewButton(root, "TEST", new Color(0.85f, 0.2f, 0.2f, 0.85f), () => panel.SetActive(!panel.activeSelf), out _);
        RectTransform trt = (RectTransform)toggle.transform;
        trt.anchorMin = trt.anchorMax = new Vector2(0f, 0.62f);
        trt.pivot = new Vector2(0f, 0.5f);
        trt.anchoredPosition = new Vector2(8f, 0f);
        trt.sizeDelta = new Vector2(120f, 70f);

        // 패널
        panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform prt = (RectTransform)panel.transform;
        prt.SetParent(root, false);
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.pivot = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(1040f, 0f);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
        var v = panel.GetComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(16, 16, 16, 16);
        v.spacing = 10f;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 탭 줄 + 닫기
        string[] tabNames = { "미니언 소환", "적 소환", "웨이브 전환", "적 체력", "용사", "기타" };
        Row(prt, r =>
        {
            for (int i = 0; i < tabNames.Length; i++)
            {
                int idx = i;
                Button b = NewButton(r, tabNames[i], TAB_OFF, () => SelectTab(idx), out TMP_Text t);
                b.gameObject.AddComponent<LayoutElement>().flexibleWidth = 2f;
                t.fontSizeMax = 30;
                tabImages.Add(b.GetComponent<Image>());
            }
            Button close = NewButton(r, "X", new Color(0.75f, 0.2f, 0.2f, 1f), () => panel.SetActive(false), out _);
            var le = close.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 90f;
            le.flexibleWidth = 0f;
        }, 80f);

        pages.Add(Page(prt, BuildMinionPage));
        pages.Add(Page(prt, BuildEnemyPage));
        pages.Add(Page(prt, BuildWavePage));
        pages.Add(Page(prt, BuildHpPage));
        pages.Add(Page(prt, BuildHeroPage));
        pages.Add(Page(prt, BuildEtcPage));

        infoText = Label(prt, "");
        infoText.fontSize = 26;
        infoText.GetComponent<LayoutElement>().preferredHeight = 80f;
        SelectTab(0);
        FillHpInputs();
    }

    void SelectTab(int idx)
    {
        curTab = idx;
        for (int i = 0; i < pages.Count; i++)
            pages[i].SetActive(i == idx);
        for (int i = 0; i < tabImages.Count; i++)
            tabImages[i].color = i == idx ? TAB_ON : TAB_OFF;
        if (idx == 3)
        {
            GameManager gm = GameManager.Instance;
            hpWave = (gm != null ? CurrentWaveIdx : 0) + 1;
            FillHpInputs(); // 탭을 열 때마다 지금 적용 중인 수치를 미리 채움 (패널 생성 땐 스테이지 데이터가 아직 없을 수 있음)
        }
        if (idx == 2)
            jumpWave = CurrentWaveIdx + 1;
    }

    GameObject Page(RectTransform parent, Action<RectTransform> build)
    {
        var go = new GameObject("Page", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        var v = go.GetComponent<VerticalLayoutGroup>();
        v.spacing = 10f;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        build(rt);
        return go;
    }

    void BuildMinionPage(RectTransform p)
    {
        Title(p, "미니언 소환");
        Row(p, r => { Arrow(r, "◀", () => minionIdx--); minionText = Label(r, ""); Arrow(r, "▶", () => minionIdx++); });
        Row(p, r =>
        {
            Arrow(r, "◀", () => mergeLevel = Mathf.Max(0, mergeLevel - 1));
            mergeText = Label(r, "");
            Arrow(r, "▶", () => mergeLevel = Mathf.Min(MinionData.MAX_MERGE_LEVEL, mergeLevel + 1));
            Btn(r, "", () => evolve = !evolve, out evolveText);
        });
        Row(p, r => { Btn(r, "소환 (빈 칸)", SpawnMinion, out _); Btn(r, "필드 미니언 제거", GameTestUtil.ClearFieldMinions, out _); });
    }

    void BuildEnemyPage(RectTransform p)
    {
        Title(p, "적 소환");
        Row(p, r => { Arrow(r, "◀", () => enemyTypeIdx = (enemyTypeIdx + ENEMY_TYPES.Length - 1) % ENEMY_TYPES.Length); enemyTypeText = Label(r, ""); Arrow(r, "▶", () => enemyTypeIdx = (enemyTypeIdx + 1) % ENEMY_TYPES.Length); });
        Row(p, r => { Arrow(r, "◀", () => defenceIdx = defenceIdx <= -1 ? DEFENCE_TYPES.Length - 1 : defenceIdx - 1); defenceText = Label(r, ""); Arrow(r, "▶", () => defenceIdx = defenceIdx >= DEFENCE_TYPES.Length - 1 ? -1 : defenceIdx + 1); });
        Row(p, r => { Arrow(r, "◀", () => countIdx = Mathf.Max(0, countIdx - 1)); countText = Label(r, ""); Arrow(r, "▶", () => countIdx = Mathf.Min(COUNTS.Length - 1, countIdx + 1)); });
        Row(p, r =>
        {
            Btn(r, "적 소환", SpawnEnemies, out _);
            Btn(r, "적 모두 제거", () => EnemyFieldController.Instance?.ClearAllForTest(), out _);
            Btn(r, "", ToggleFreeze, out freezeText);
        });
        enemyInfoText = Label(p, "");
        enemyInfoText.fontSize = 26;
    }

    void BuildWavePage(RectTransform p)
    {
        Title(p, "웨이브 전환");
        waveInfoText = Label(p, "");
        waveInfoText.GetComponent<LayoutElement>().preferredHeight = 90f;
        Row(p, r => { Btn(r, "◀ 이전 웨이브", PrevWave, out _); Btn(r, "다음 웨이브 ▶", NextWave, out _); });
        Row(p, r => { Btn(r, "정지 (적 제거 → 다음 wave)", StopWave, out _); Btn(r, "웨이브 시작", StartWave, out _); });
        Row(p, r =>
        {
            Arrow(r, "◀", () => jumpWave = Mathf.Max(1, jumpWave - 1));
            jumpText = Label(r, "");
            Arrow(r, "▶", () => jumpWave = Mathf.Min(GameSetting.LAST_WAVE, jumpWave + 1));
            Btn(r, "이동 (배치 유지)", () => JumpTo(jumpWave - 1), out _);
        });
        Row(p, r => { Btn(r, "", ToggleNoEliteBoss, out noEliteText); });
        summaryText = Label(p, "");
        summaryText.fontSize = 24;
        summaryText.fontSizeMax = 24;
        summaryText.alignment = TextAlignmentOptions.TopLeft;
        summaryText.GetComponent<LayoutElement>().preferredHeight = 300f;
        TMP_Text help = Label(p, "이전/다음 웨이브 = 그 wave 브레이크타임(레벨업·카드 사용 전) 상태로 복구: 미니언·룬·막힌 칸·킹 레벨·용사·미니언 강화·주사위 코인. 카드는 새로 받음.\n다음 웨이브에 저장이 없으면 지금 배치 그대로 이동.");
        help.fontSize = 22;
        help.fontSizeMax = 22;
        help.GetComponent<LayoutElement>().preferredHeight = 90f;
    }

    void BuildHpPage(RectTransform p)
    {
        Title(p, "적 체력 (지금 판만 적용, 씬을 나가면 원래대로)");
        Row(p, r => { FieldName(r, "EnemyHp"); hpInput = Input(r, TMP_InputField.ContentType.DecimalNumber); });
        Row(p, r => { FieldName(r, "AddHpRatioPerWave"); addInput = Input(r, TMP_InputField.ContentType.DecimalNumber); });
        Row(p, r => { FieldName(r, "MultiHpPerWave"); mulInput = Input(r, TMP_InputField.ContentType.DecimalNumber); });
        Row(p, r => { FieldName(r, "CycleHpMuls"); cycleInput = Input(r, TMP_InputField.ContentType.Standard); });
        Row(p, r => { Btn(r, "적용", ApplyHp, out _); Btn(r, "원래대로", ResetHp, out _); });
        Row(p, r =>
        {
            Arrow(r, "◀", () => hpWave = Mathf.Max(1, hpWave - 1));
            hpWaveText = Label(r, "");
            Arrow(r, "▶", () => hpWave = Mathf.Min(GameSetting.LAST_WAVE, hpWave + 1));
            Btn(r, "-10", () => hpWave = Mathf.Max(1, hpWave - 10), out _);
            Btn(r, "+10", () => hpWave = Mathf.Min(GameSetting.LAST_WAVE, hpWave + 10), out _);
        });
        hpInfoText = Label(p, "");
        hpInfoText.fontSize = 26;
        hpInfoText.alignment = TextAlignmentOptions.TopLeft;
        hpInfoText.GetComponent<LayoutElement>().preferredHeight = 200f;
    }

    void BuildHeroPage(RectTransform p)
    {
        Title(p, "용사");
        Row(p, r => { Arrow(r, "◀", () => heroIdx--); heroText = Label(r, ""); Arrow(r, "▶", () => heroIdx++); });
        Row(p, r =>
        {
            Arrow(r, "◀", () => heroLevel = Mathf.Max(1, heroLevel - 1));
            levelText = Label(r, "");
            Arrow(r, "▶", () => heroLevel = Mathf.Min(GameSetting.KING_MAX_LEVEL, heroLevel + 1));
            Btn(r, "", () => enhance = !enhance, out enhanceText);
        });
        Row(p, r => { Btn(r, "용사 장착", EquipHero, out _); Btn(r, "에너지 채우기", () => CurrentHero?.FillChargesForTest(), out _); });
        Row(p, r =>
        {
            for (int i = 0; i < HeroKingAbility.SKILL_COUNT; i++)
            {
                int idx = i;
                Btn(r, "", () => UseSkill(idx), out skillTexts[i]);
            }
        });
    }

    void BuildEtcPage(RectTransform p)
    {
        Title(p, "기타");
        Row(p, r =>
        {
            Btn(r, "", ToggleSpeed, out speedText);
            Btn(r, "", () => invincible = !invincible, out invincibleText);
            Btn(r, "체력 회복", FullHeal, out _);
        });
        Row(p, r =>
        {
            Btn(r, "주사위 +10", () => KingSlime.Instance?.AddDiceCoin(10), out _);
            Btn(r, "경험치 +1", () => KingSlime.Instance?.AddExp(1), out _);
        });
    }

    void Title(RectTransform parent, string text)
    {
        TMP_Text t = Label(parent, text);
        t.color = new Color(1f, 0.85f, 0.3f);
        t.alignment = TextAlignmentOptions.MidlineLeft;
    }

    void Row(RectTransform parent, Action<RectTransform> fill, float height = 90f)
    {
        var go = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        go.GetComponent<LayoutElement>().preferredHeight = height;
        var h = go.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 8f;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;
        fill(rt);
    }

    TMP_Text Label(RectTransform parent, string text)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<LayoutElement>().preferredHeight = 60f;
        go.GetComponent<LayoutElement>().flexibleWidth = 3f;
        var t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = 36;
        t.enableAutoSizing = true;
        t.fontSizeMin = 16;
        t.fontSizeMax = 36;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.raycastTarget = false;
        t.richText = true;
        t.text = text;
        return t;
    }

    // 입력칸 이름 (너비 고정 → 입력칸들이 같은 줄에 맞춰짐)
    void FieldName(RectTransform parent, string text)
    {
        TMP_Text t = Label(parent, text);
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.fontSizeMax = 30;
        var le = t.GetComponent<LayoutElement>();
        le.preferredWidth = 400f;
        le.flexibleWidth = 0f;
    }

    TMP_InputField Input(RectTransform parent, TMP_InputField.ContentType type)
    {
        var go = new GameObject("Input", typeof(RectTransform), typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.95f);
        go.GetComponent<LayoutElement>().flexibleWidth = 3f;
        var area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        var art = (RectTransform)area.transform;
        art.SetParent(go.transform, false);
        art.anchorMin = Vector2.zero;
        art.anchorMax = Vector2.one;
        art.offsetMin = new Vector2(12f, 6f);
        art.offsetMax = new Vector2(-12f, -6f);
        var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = (RectTransform)textGo.transform;
        trt.SetParent(art, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var text = textGo.GetComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.fontSize = 34;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        var input = go.GetComponent<TMP_InputField>();
        input.textViewport = art;
        input.textComponent = text;
        input.contentType = type;
        input.fontAsset = font;
        input.pointSize = 34;
        return input;
    }

    void Arrow(RectTransform parent, string text, Action onClick)
    {
        Button b = NewButton(parent, text, new Color(0.3f, 0.3f, 0.35f, 1f), onClick, out _);
        var le = b.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 100f;
        le.flexibleWidth = 0f;
    }

    void Btn(RectTransform parent, string text, Action onClick, out TMP_Text label)
    {
        Button b = NewButton(parent, text, new Color(0.25f, 0.45f, 0.75f, 1f), onClick, out label);
        b.gameObject.AddComponent<LayoutElement>().flexibleWidth = 2f;
    }

    Button NewButton(RectTransform parent, string text, Color color, Action onClick, out TMP_Text label)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        var b = go.GetComponent<Button>();
        b.onClick.AddListener(() =>
        {
            try { onClick?.Invoke(); }
            catch (Exception e) { Debug.LogException(e); ToastCanvas.Toast(e.Message); }
            if (panel != null && panel.activeSelf) RefreshTexts();
        });
        label = Label((RectTransform)go.transform, text);
        RectTransform lrt = label.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(6f, 4f);
        lrt.offsetMax = new Vector2(-6f, -4f);
        UnityEngine.Object.Destroy(label.GetComponent<LayoutElement>());
        return b;
    }
    #endregion
}
