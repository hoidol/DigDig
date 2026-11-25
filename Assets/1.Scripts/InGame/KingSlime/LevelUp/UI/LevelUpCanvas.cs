using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
namespace InGame
{
    public class LevelUpCanvas : CanvasUI<LevelUpCanvas>
{
    public LevelUpStatPanel[] levelUpStatPanels;
    public TMPro.TMP_Text headerText; // 비워도 됨. "레벨 업!"
    public TMPro.TMP_Text subText;    // 비워도 됨. 무엇을 고르는지 (종족을 선택하세요 …)
    // (2026-11-25) 이번 레벨업으로 오르는 용사 능력치 (작게 "공격력 : 15 → 23"). 레벨업 첫 화면(보통 단계 카드)에만. 비우면 실행 중 subText 아래에 생성
    public TMPro.TMP_Text statChangeText;
    // (2026-11-25) 미니언 강화 카드 새로고침 (한 판 무료 2번 → 광고 3번, RerollCounter). 비우면 실행 중 카드 아래에 생성 (카드의 선택 버튼 모양 복사)
    public UnityEngine.UI.Button rerollButton;
    public TMPro.TMP_Text rerollText;

    // (2026-11-23) 창 하단 "레벨업 보상 [주사위 코인]+1" (지급은 KingSlime.AddExp). 비우면 실행 중 카드 아래에 기본 모양 생성(BuildRewardPanel)
    public RectTransform rewardPanel;
    public TMPro.TMP_Text rewardValueText; // "+1"

    // (2026-11-21) 레벨업 창은 전투 중엔 안 열림. (2026-11-22) 경험치가 다 차면 바로 OpenPending (KingSlime.AddExp, 레벨업 버튼 삭제)
    // 밀린 레벨업(KingSlime.pendingLevelUps)만큼 연속으로 열림: 하나 고르면 닫히고 남았으면 다시 열림. (2026-11-22) 시간은 멈추지 않음
    float prevTimeScale = 1f;
    bool isPausing;

    // (2026-11-25) 레벨업 한 번 = ① 단계 카드(용사·스킬 해방·용사 강화, LevelUpChoiceRule. 그 레벨에 없으면 건너뜀) → ② 미니언 강화 카드(장착 미니언 × 스탯, MinionEnhanceSystem)
    // 단계 카드를 고르면 레벨업을 소모하지 않고 같은 레벨의 미니언 강화 카드로 다시 열림. 미니언 강화까지 고르면 소모
    // (이전 2026-11-23: ① 킹 능력치 카드 → ② 단계 카드)
    bool stageStepDone;
    bool stageSkipped; // 이번 레벨에 단계 카드가 없어 바로 미니언 강화 → 능력치 변화는 그 화면에
    public bool IsStatStep { get; private set; } // 지금 창 = 미니언 강화 카드 단계 (이름은 이전 능력치 카드 단계 그대로)

    public static bool CanOpen => !RuneRewardSystem.IsOpen && Instance != null && KingSlime.Instance != null && KingSlime.Instance.pendingLevelUps > 0
        && GameManager.Instance != null && GameManager.Instance.isBreakTime;

    public static void OpenPending()
    {
        if (!CanOpen || Instance.gameObject.activeSelf)
            return;
        Instance.OpenCanvas();
    }

    void OnDestroy()
    {
        if (isPausing)
            Time.timeScale = prevTimeScale;
    }

    public override void CloseCanvas()
    {
        base.CloseCanvas();
        if (!IsStatStep) // 단계 카드를 골랐음 → 같은 레벨의 미니언 강화 카드
        {
            stageStepDone = true;
            OpenCanvas();
            return;
        }
        IsStatStep = false;
        stageStepDone = false;
        stageSkipped = false;
        KingSlime.Instance?.ConsumeLevelUp();
        if (KingSlime.Instance != null && KingSlime.Instance.pendingLevelUps > 0)
        {
            OpenCanvas();
            return;
        }
        if (isPausing)
        {
            isPausing = false;
            Time.timeScale = prevTimeScale;
        }
        GameEventBus.Publish(new LevelUpCanvasClosedEvent()); // (2026-11-25) 주사위 코인·최대 미니언 수 UI 갱신
        RuneRewardSystem.TryOpen(); // 레벨업 동안 기다린 룬 보상
    }

    //뽑기
    public override void OpenCanvas(Action closeCallback = null)
    {
        // (2026-11-22) 시간은 멈추지 않음 (브레이크타임에만 열려서 멈출 필요 없음, 룬 타일 선택도 마찬가지)
        base.OpenCanvas(closeCallback);

        // 카드 구성 = ① LevelUpChoiceRule 단계 카드 (그 레벨에 없으면 건너뜀) → ② 미니언 강화 카드 (최대 레벨 제외, 고를 게 없으면 닫음)
        KingSlime king = KingSlime.Instance;
        KingGrowthSetting setting = KingGrowthSetting.Instance;
        List<AbilityData> choices = new List<AbilityData>();
        IsStatStep = false;
        if (king != null && setting != null && !stageStepDone)
        {
            choices = LevelUpChoiceRule.GetChoices(MakeChoiceInput(king, setting), rng);
            stageSkipped = choices.Count == 0;
            if (choices.Count == 0)
                stageStepDone = true;
        }
        if (stageStepDone)
        {
            IsStatStep = true;
            choices = PickMinionEnhanceChoices();
        }

        // 버프 칸 확장 카드: 카드마다 추가 칸 (지금 버프 칸을 뺀 나머지 중 랜덤, 카드끼리 다르게)
        List<Vector2Int> currentTiles = king != null ? king.abilityInventory.GetBuffTileOffsets() : new List<Vector2Int>();
        List<Vector2Int> expandTiles = LevelUpChoiceRule.PickExpandTiles(currentTiles, choices.Count, rng);

        EnsurePanels(choices.Count);
        ShowReward();
        ShowStatChange(king, !IsStatStep || stageSkipped);
        if (headerText != null)
            headerText.text = king != null ? $"레벨 업! Lv.{king.ChoiceLevel}" : "레벨 업!";
        AbilityCategory stage = !IsStatStep && king != null && setting != null ? LevelUpChoiceRule.GetStage(MakeChoiceInput(king, setting)) : AbilityCategory.Stat;
        HeroKingAbility ownedHero = king != null ? king.abilityInventory.Get<HeroKingAbility>() : null;
        if (subText != null)
        {
            subText.text = IsStatStep ? "강화할 미니언을 선택하세요"
                : stage == AbilityCategory.Hero ? "동경하는 용사를 선택하세요"
                : stage == AbilityCategory.HeroSkill ? "새 스킬이 해방됩니다!"
                : stage == AbilityCategory.HeroBoost ? "용사를 강화하세요"
                : stage == AbilityCategory.TileExpand ? "추가할 버프 칸을 선택하세요"
                : "능력을 선택하세요";
        }
        int i = 0;
        foreach (var panel in levelUpStatPanels)
        {
            bool active = i < choices.Count;
            panel.gameObject.SetActive(active);
            if (!active)
                continue;
            int nextLevel = choices[i].IsMinionEnhance ? choices[i].GetMinionEnhanceLevel() + choices[i].minionAmount : king.abilityInventory.GetLevel(choices[i]) + 1;
            if (stage == AbilityCategory.Hero) // (2026-11-24) 용사 카드마다 정예병 칸 랜덤 2칸
                panel.SetHeroData(choices[i], LevelUpChoiceRule.PickHeroTiles(rng));
            else if (stage == AbilityCategory.HeroSkill && ownedHero != null)
                panel.SetHeroSkillData(ownedHero, ownedHero.NextSkillIndex);
            else if (choices[i].category == AbilityCategory.TileExpand && i < expandTiles.Count)
                panel.SetExpandData(choices[i], currentTiles, expandTiles[i], nextLevel);
            else
                panel.SetData(choices[i], nextLevel);
            RectTransform rTr = panel.GetComponent<RectTransform>();
            rTr.DOKill();
            rTr.localScale = Vector3.zero;
            rTr.DOScale(1.1f, 0.2f + i * 0.15f).SetUpdate(true).OnComplete(() =>
            {
                rTr.DOScale(1, 0.25f).SetUpdate(true);
            });
            i++;
        }

        UpdateRerollButton(IsStatStep && choices.Count > 0);

        // 고를 카드가 없으면(모두 최대 레벨) 그냥 닫음
        if (choices.Count == 0)
            CloseCanvas();
    }

    #region 미니언 강화 카드 새로고침
    static RerollCounter Reroll => RerollCounter.MinionEnhance;

    void UpdateRerollButton(bool show)
    {
        show &= Reroll.CanReroll;
        if (rerollButton == null && show)
            BuildRerollButton();
        if (rerollButton == null)
            return;
        rerollButton.gameObject.SetActive(show);
        rerollButton.interactable = !Reroll.WaitingAd;
        if (rerollText != null)
            rerollText.text = Reroll.GetLabel("새로고침");
    }

    // 무료 2번 → 광고 3번 (광고 제거 구매자는 광고 없이, 횟수는 같음)
    public void OnClickedReroll()
    {
        if (!IsStatStep)
            return;
        Reroll.TryReroll(RerollMinionEnhance, () =>
        {
            if (this != null)
                UpdateRerollButton(IsStatStep && gameObject.activeSelf);
        });
        UpdateRerollButton(IsStatStep && gameObject.activeSelf);
    }

    // 같은 레벨의 미니언 강화 카드를 다시 뽑음 (단계는 그대로)
    void RerollMinionEnhance()
    {
        if (this != null && gameObject.activeSelf && IsStatStep)
            OpenCanvas();
    }

    // 기본 모양: 첫 카드의 선택 버튼을 복사해 카드 줄 아래 가운데에
    void BuildRerollButton()
    {
        LevelUpStatPanel first = levelUpStatPanels != null ? levelUpStatPanels.FirstOrDefault(p => p != null) : null;
        UnityEngine.UI.Button template = first != null ? first.GetComponentsInChildren<UnityEngine.UI.Button>(true).LastOrDefault() : null;
        if (template == null)
            return;
        RectTransform panelRt = (RectTransform)first.transform;
        RectTransform container = panelRt.parent as RectTransform;
        UnityEngine.UI.Button btn = Instantiate(template, transform);
        btn.name = "MinionEnhanceRerollButton";
        btn.onClick = new UnityEngine.UI.Button.ButtonClickedEvent(); // 복사된 카드 선택 연결 제거
        btn.onClick.AddListener(OnClickedReroll);
        RectTransform rt = (RectTransform)btn.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, 320f), Mathf.Max(rt.sizeDelta.y, 90f));
        // 카드 아래 끝 (카드는 열릴 때 크기 0에서 커지므로 부모 기준으로 계산)
        Vector3 scale = container != null ? container.lossyScale : Vector3.one;
        float bottom = panelRt.position.y - panelRt.rect.height * panelRt.pivot.y * scale.y;
        rt.position = new Vector3(((RectTransform)transform).position.x, bottom - 30f * scale.y, rt.position.z);
        rerollText = btn.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (rerollText != null)
            rerollText.enableAutoSizing = true;
        rerollButton = btn;
    }
    #endregion

    static readonly System.Random rng = new System.Random();

    // (2026-11-25) 미니언 강화 카드 3장 = 장착 미니언 × 스탯 (서로 다른 조합, 최대 레벨 제외)
    public static List<AbilityData> PickMinionEnhanceChoices()
    {
        var keys = new List<string>();
        UserMinion[] equipped = UserDataManager.Instance != null ? UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions : null;
        if (equipped != null)
            foreach (UserMinion m in equipped)
                if (m != null && !string.IsNullOrEmpty(m.key))
                    keys.Add(m.key);
        MinionData GetData(string key) => MinionManager.Instance != null && MinionManager.Instance.minionDataDic.TryGetValue(key, out MinionData d) ? d : null;
        var result = new List<AbilityData>();
        foreach (var (key, stat) in MinionEnhanceSystem.PickChoices(keys, GetData, LevelUpChoiceRule.CHOICE_COUNT, rng))
            result.Add(AbilityData.CreateMinionEnhanceCard(key, stat, MinionEnhanceSystem.RollAmount(key, stat, (float)rng.NextDouble() * 100f))); // 대박 1% (×3) · 행운 10% (×2)
        return result;
    }

    // 다음 레벨업 창의 카드 구성 입력 (ExpBar 버튼 글씨에도 사용)
    public static LevelUpChoiceRule.Input MakeChoiceInput(KingSlime king, KingGrowthSetting setting)
    {
        // (2026-11-23) 룬은 레벨업에서 빠짐 (엘리트·보스 보상 RuneRewardSystem), 패시브 단계 없음
        // (2026-11-23) 종족·직업 → 용사 하나로. (2026-11-24) 버프 칸 확장 없음 → 스킬 해방·용사 강화
        // (2026-11-25) 용사 강화 수 = 용사 강화 카드(정예 칸 추가·충전 가속) 레벨 합
        KingAbilityInventory inv = king.abilityInventory;
        HeroKingAbility hero = inv.Get<HeroKingAbility>();
        int boostCount = 0;
        if (setting.heroBoostAbilities != null)
            foreach (var d in setting.heroBoostAbilities.Distinct())
                if (d != null)
                    boostCount += inv.GetLevel(d);
        return new LevelUpChoiceRule.Input
        {
            level = king.ChoiceLevel,
            ownedHero = hero != null ? hero.data : null,
            heroLevel = hero != null ? hero.level : 0,
            heroSkillCount = hero != null ? hero.UnlockedSkillCount : 0,
            boostCount = boostCount,
            heroPool = setting.heroAbilities,
            boostPool = setting.heroBoostAbilities,
            getLevel = inv.GetLevel,
        };
    }

    #region 레벨업 능력치 변화
    // 화면 표시 순서
    static readonly StatType[] HERO_STAT_DISPLAY = { StatType.AttackPower, StatType.AttackSpeed, StatType.CritChance, StatType.AttackRange, StatType.MaxHp };

    // 용사 레벨 toLevel-1 → toLevel 변화 ("공격력 : 15 → 23" 줄들). 용사가 없거나 바뀌는 게 없으면 ""
    public static string GetHeroLevelUpText(KingSlime king, int toLevel)
    {
        HeroKingAbility hero = king != null && king.abilityInventory != null ? king.abilityInventory.Get<HeroKingAbility>() : null;
        if (hero == null || toLevel <= 1 || king.statMgr == null)
            return "";
        HeroDataTable.HeroData data = hero.HeroData;
        var lines = new List<string>();
        if (data != null)
            foreach (StatType stat in HERO_STAT_DISPLAY)
            {
                float from = king.statMgr.GetHeroStat(data, stat, toLevel - 1);
                float to = king.statMgr.GetHeroStat(data, stat, toLevel);
                if (Mathf.Approximately(from, to))
                    continue;
                string unit = stat == StatType.CritChance ? "%" : "";
                lines.Add($"{GetStatName(stat)} : {KingAbility.FormatNumber(from)}{unit} → <color=#7CFF6B>{KingAbility.FormatNumber(to)}{unit}</color>");
            }
        string eliteFrom = hero.GetEliteBuffText(toLevel - 1), eliteTo = hero.GetEliteBuffText(toLevel);
        if (eliteFrom != eliteTo)
            lines.Add($"정예병 칸 : {eliteFrom} → <color=#7CFF6B>{eliteTo}</color>");
        return string.Join("\n", lines);
    }

    static string GetStatName(StatType stat)
    {
        string title = TranslateManager.GetText($"{stat}_title");
        if (!title.StartsWith("*"))
            return title;
        switch (stat)
        {
            case StatType.AttackPower: return "공격력";
            case StatType.AttackSpeed: return "공속";
            case StatType.CritChance: return "치명타 확률";
            case StatType.AttackRange: return "공격 범위";
            case StatType.MaxHp: return "최대 체력";
            default: return stat.ToString();
        }
    }

    void ShowStatChange(KingSlime king, bool show)
    {
        string text = show && king != null ? GetHeroLevelUpText(king, king.ChoiceLevel) : "";
        if (statChangeText == null && string.IsNullOrEmpty(text))
            return;
        if (statChangeText == null)
            BuildStatChangeText();
        if (statChangeText == null)
            return;
        statChangeText.gameObject.SetActive(!string.IsNullOrEmpty(text));
        statChangeText.text = text;
    }

    // 기본 모양: 안내 글씨(subText, 없으면 제목) 바로 아래 가운데, 작은 글씨
    void BuildStatChangeText()
    {
        TMPro.TMP_Text anchor = subText != null ? subText : headerText;
        if (anchor == null)
            return;
        RectTransform at = anchor.rectTransform;
        var go = new GameObject("StatChangeText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        var rt = (RectTransform)go.transform;
        rt.SetParent(at.parent, false);
        rt.anchorMin = at.anchorMin;
        rt.anchorMax = at.anchorMax;
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(Mathf.Max(at.rect.width, 700f), 160f);
        rt.anchoredPosition = at.anchoredPosition + new Vector2(0f, -at.rect.height * (1f - at.pivot.y) - 4f);
        var t = go.GetComponent<TMPro.TextMeshProUGUI>();
        t.font = anchor.font;
        t.fontSharedMaterial = anchor.fontSharedMaterial;
        t.fontSize = Mathf.Max(22f, anchor.fontSize * 0.6f);
        t.color = new Color(1f, 1f, 1f, 0.9f);
        t.alignment = TMPro.TextAlignmentOptions.Top;
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        statChangeText = t;
    }
    #endregion

    void ShowReward()
    {
        if (rewardPanel == null)
            BuildRewardPanel();
        if (rewardPanel == null)
            return;
        rewardPanel.gameObject.SetActive(GameSetting.LEVEL_UP_DICE_COIN > 0);
        if (rewardValueText != null)
            rewardValueText.text = $"+{GameSetting.LEVEL_UP_DICE_COIN}";
        rewardPanel.DOKill();
        rewardPanel.localScale = Vector3.zero;
        rewardPanel.DOScale(1f, 0.3f).SetDelay(0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    #region 레벨업 보상 기본 모양 (씬에 없을 때)
    // (2026-11-23) "레벨 업!" 제목 바로 위 가운데에 한 줄: "레벨업 보상 [주사위 코인 아이콘] +1"
    //   (이전: 카드 영역 아래 → 화면 아래쪽이라 눈에 안 띔)
    void BuildRewardPanel()
    {
        RectTransform parent = headerText != null ? headerText.rectTransform
            : levelUpStatPanels != null && levelUpStatPanels.Length > 0 && levelUpStatPanels[0] != null
                ? levelUpStatPanels[0].transform.parent?.parent as RectTransform : null;
        if (parent == null)
            parent = transform as RectTransform;
        TMPro.TMP_FontAsset font = headerText != null ? headerText.font : GetComponentInChildren<TMPro.TMP_Text>(true)?.font;
        DiceCoinPanel dicePanel = FindFirstObjectByType<DiceCoinPanel>(FindObjectsInactive.Include);
        UnityEngine.UI.Image diceIcon = null;
        if (dicePanel != null) // 주사위 그림 우선 (패널 바탕 그림 말고)
        {
            var images = dicePanel.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            diceIcon = images.FirstOrDefault(img => img.sprite != null && img.sprite.name.ToLower().Contains("dice"))
                ?? images.FirstOrDefault(img => img.sprite != null && img.gameObject != dicePanel.gameObject)
                ?? images.FirstOrDefault();
        }

        var go = new GameObject("LevelUpRewardPanel", typeof(RectTransform));
        rewardPanel = (RectTransform)go.transform;
        rewardPanel.SetParent(parent, false);
        rewardPanel.anchorMin = rewardPanel.anchorMax = new Vector2(0.5f, 1f);
        rewardPanel.pivot = new Vector2(0.5f, 0f);
        rewardPanel.sizeDelta = new Vector2(600f, 80f);
        rewardPanel.anchoredPosition = new Vector2(0f, 10f);

        var row = new GameObject("RewardRow", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
        var rowRt = (RectTransform)row.transform;
        rowRt.SetParent(rewardPanel, false);
        rowRt.anchorMin = Vector2.zero;
        rowRt.anchorMax = Vector2.one;
        rowRt.offsetMin = rowRt.offsetMax = Vector2.zero;
        NewText("TitleText", rowRt, font, 44, "레벨업 보상", Vector2.zero, new Vector2(240f, 70f), TMPro.TextAlignmentOptions.MidlineRight)
            .color = new Color(1f, 0.9f, 0.4f);
        var layout = row.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 8f;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;

        var iconGo = new GameObject("DiceCoinIcon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        var iconRt = (RectTransform)iconGo.transform;
        iconRt.SetParent(rowRt, false);
        iconRt.sizeDelta = new Vector2(64f, 64f);
        var icon = iconGo.GetComponent<UnityEngine.UI.Image>();
        icon.sprite = diceIcon != null ? diceIcon.sprite : null;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.enabled = icon.sprite != null;

        rewardValueText = NewText("ValueText", rowRt, font, 56, "+1", Vector2.zero, new Vector2(90f, 70f), TMPro.TextAlignmentOptions.MidlineLeft);
        rewardValueText.fontStyle = TMPro.FontStyles.Bold;
    }

    static TMPro.TMP_Text NewText(string name, RectTransform parent, TMPro.TMP_FontAsset font, float size, string text,
        Vector2 pos, Vector2 sizeDelta, TMPro.TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = pos;
        var t = go.GetComponent<TMPro.TextMeshProUGUI>();
        if (font != null)
            t.font = font;
        t.fontSize = size;
        t.color = Color.white;
        t.alignment = align;
        t.raycastTarget = false;
        t.text = text;
        return t;
    }
    #endregion

    // 패널은 씬에 1개만 두어도 됨: 빈 칸(null)은 빼고, 모자라면 첫 패널을 같은 부모 아래에 복사
    void EnsurePanels(int count)
    {
        var panels = (levelUpStatPanels ?? new LevelUpStatPanel[0]).Where(p => p != null).ToList();
        if (panels.Count == 0)
        {
            Debug.LogWarning("[LevelUpCanvas] 레벨업 패널 없음");
            levelUpStatPanels = panels.ToArray();
            return;
        }
        while (panels.Count < count)
        {
            LevelUpStatPanel copy = Instantiate(panels[0], panels[0].transform.parent);
            copy.name = $"{panels[0].name}_{panels.Count}";
            panels.Add(copy);
        }
        levelUpStatPanels = panels.ToArray();
    }
}
}
