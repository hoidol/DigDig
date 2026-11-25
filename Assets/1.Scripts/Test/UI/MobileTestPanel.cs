using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// (2026-11-25) 기기(모바일)용 인게임 테스트 패널: 미니언·용사를 바꿔 가며 테스트
// BuildSetting.isTestMode일 때 InGame 씬(GameManager가 있는 씬)에 자동으로 생김 (씬에 둘 필요 없음, 실행 중 코드로 UI 생성)
// 화면 왼쪽 "TEST" 버튼 → 패널 열기/닫기
//   미니언: 종 ◀▶ · 머지 레벨 ◀▶ · 진화 · 소환(빈 칸) · 필드 미니언 전부 제거
//   용사: 용사 ◀▶ · 킹 레벨 ◀▶ · 스킬 강화 · 장착(그 레벨까지 스킬 해방) · 에너지 채우기 · 스킬 1·2·3 바로 쓰기
//   기타: 배속 · 무적 · 주사위 코인 +10 · wave 시작/다음 wave
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
    TMP_Text minionText, mergeText, evolveText, heroText, levelText, enhanceText, speedText, invincibleText, infoText;
    readonly TMP_Text[] skillTexts = new TMP_Text[HeroKingAbility.SKILL_COUNT];

    int minionIdx, mergeLevel, heroIdx, heroLevel = GameSetting.KING_MAX_LEVEL, speed = 1;
    bool evolve, enhance, invincible = true;
    static readonly int[] SPEEDS = { 1, 2, 4, 8 };

    void Start()
    {
        TMP_Text any = FindFirstObjectByType<TMP_Text>(FindObjectsInactive.Include);
        font = any != null ? any.font : TMP_Settings.defaultFontAsset;
        BuildUI();
        panel.SetActive(false);
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
        if (panel != null && panel.activeSelf)
            RefreshTexts();
    }

    #region 동작
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

    void NextWave()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
            return;
        if (gm.isBreakTime)
            gm.EndBreakTime();
        else
            gm.StartNextWave();
    }

    void RefreshTexts()
    {
        List<string> keys = Keys;
        if (keys.Count > 0)
        {
            minionIdx = (minionIdx % keys.Count + keys.Count) % keys.Count;
            MinionData d = MinionManager.Instance.minionDataDic[keys[minionIdx]];
            minionText.text = $"{d.Title} ({d.key})";
        }
        mergeText.text = $"머지 Lv.{mergeLevel + 1}";
        evolveText.text = evolve ? "진화 ON" : "진화 OFF";
        AbilityData[] heroes = Heroes;
        if (heroes.Length > 0)
        {
            heroIdx = (heroIdx % heroes.Length + heroes.Length) % heroes.Length;
            heroText.text = heroes[heroIdx].title;
        }
        levelText.text = $"킹 Lv.{heroLevel}";
        enhanceText.text = enhance ? "스킬 강화 ON" : "스킬 강화 OFF";
        speedText.text = $"x{speed}";
        invincibleText.text = invincible ? "무적 ON" : "무적 OFF";
        HeroKingAbility hero = CurrentHero;
        for (int i = 0; i < skillTexts.Length; i++)
            skillTexts[i].text = hero != null ? $"{i + 1}. {hero.GetSkillName(i)}{(hero.IsSkillUnlocked(i) ? "" : " (잠김)")}" : $"스킬 {i + 1}";
        KingSlime king = KingSlime.Instance;
        GameManager gm = GameManager.Instance;
        infoText.text = hero != null
            ? $"{hero.data.title} Lv.{hero.HeroLevel} · 에너지 {hero.Charges}/{hero.MaxCharges} · 킹 공격력 {king.statMgr.AttackPower:0.#}\n킹 피해 {DamageMeterSystem.KingTotalDamage:N0} · 미니언 피해 {DamageMeterSystem.TotalDamage:N0} · Wave {(gm != null ? gm.waveIdx + 1 : 0)}"
            : $"용사 없음 · 미니언 피해 {DamageMeterSystem.TotalDamage:N0}";
    }
    #endregion

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

        // 열기/닫기 버튼 (왼쪽 가운데)
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
        prt.sizeDelta = new Vector2(1000f, 0f);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        var v = panel.GetComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(20, 20, 20, 20);
        v.spacing = 10f;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Title(prt, "미니언");
        Row(prt, r => { Arrow(r, "◀", () => minionIdx--); minionText = Label(r, ""); Arrow(r, "▶", () => minionIdx++); });
        Row(prt, r =>
        {
            Arrow(r, "◀", () => mergeLevel = Mathf.Max(0, mergeLevel - 1));
            mergeText = Label(r, "");
            Arrow(r, "▶", () => mergeLevel = Mathf.Min(MinionData.MAX_MERGE_LEVEL, mergeLevel + 1));
            Btn(r, "", () => evolve = !evolve, out evolveText);
        });
        Row(prt, r => { Btn(r, "소환 (빈 칸)", SpawnMinion, out _); Btn(r, "필드 미니언 제거", GameTestUtil.ClearFieldMinions, out _); });

        Title(prt, "용사");
        Row(prt, r => { Arrow(r, "◀", () => heroIdx--); heroText = Label(r, ""); Arrow(r, "▶", () => heroIdx++); });
        Row(prt, r =>
        {
            Arrow(r, "◀", () => heroLevel = Mathf.Max(1, heroLevel - 1));
            levelText = Label(r, "");
            Arrow(r, "▶", () => heroLevel = Mathf.Min(GameSetting.KING_MAX_LEVEL, heroLevel + 1));
            Btn(r, "", () => enhance = !enhance, out enhanceText);
        });
        Row(prt, r => { Btn(r, "용사 장착", EquipHero, out _); Btn(r, "에너지 채우기", () => CurrentHero?.FillChargesForTest(), out _); });
        Row(prt, r =>
        {
            for (int i = 0; i < HeroKingAbility.SKILL_COUNT; i++)
            {
                int idx = i;
                Btn(r, "", () => UseSkill(idx), out skillTexts[i]);
            }
        });

        Title(prt, "기타");
        Row(prt, r =>
        {
            Btn(r, "", ToggleSpeed, out speedText);
            Btn(r, "", () => invincible = !invincible, out invincibleText);
            Btn(r, "주사위 +10", () => KingSlime.Instance?.AddDiceCoin(10), out _);
            Btn(r, "wave 시작/다음", NextWave, out _);
        });
        infoText = Label(prt, "");
        infoText.fontSize = 28;
        infoText.GetComponent<LayoutElement>().preferredHeight = 80f;
    }

    void Title(RectTransform parent, string text)
    {
        TMP_Text t = Label(parent, text);
        t.color = new Color(1f, 0.85f, 0.3f);
        t.alignment = TextAlignmentOptions.MidlineLeft;
    }

    void Row(RectTransform parent, Action<RectTransform> fill)
    {
        var go = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        go.GetComponent<LayoutElement>().preferredHeight = 90f;
        var h = go.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 10f;
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
        t.fontSizeMin = 18;
        t.fontSizeMax = 36;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.raycastTarget = false;
        t.text = text;
        return t;
    }

    void Arrow(RectTransform parent, string text, Action onClick)
    {
        Button b = NewButton(parent, text, new Color(0.3f, 0.3f, 0.35f, 1f), onClick, out _);
        var le = b.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 110f;
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
