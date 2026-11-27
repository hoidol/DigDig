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
    // (2026-11-27) Lv.1 용사 선택 카드 = 일반 카드와 따로 (씬 LevelUpCanvas/Panel/Bg/HeroSelectContainer, 메뉴 Tools/InGame/Add Hero Select Cards To LevelUpCanvas)
    //   1장만 두면 모자란 만큼 복사. 비어 있으면 경고 + 일반 카드로 대신 보여 줌
    public HeroSelectCard[] heroSelectCards;
    public TMPro.TMP_Text headerText; // 비워도 됨. "레벨 업!"
    public TMPro.TMP_Text subText;    // 비워도 됨. 무엇을 고르는지 (종족을 선택하세요 …)
    // (2026-11-25) 이전 작은 능력치 변화 글씨. (2026-11-26) 스탯 변화 화면(statScreen*)으로 옮겨서 항상 숨김 (씬 연결 유지용)
    public TMPro.TMP_Text statChangeText;
    // (2026-11-26) 용사 스탯 변화 화면: 카드 대신 이번 레벨업 변화(킹 스탯·정예병 칸·미니언 수·스킬 해방) + 확인 버튼. 비우면 실행 중 생성
    public RectTransform statScreenRoot;
    public TMPro.TMP_Text statScreenText;
    public UnityEngine.UI.Button statScreenConfirmButton;
    // (2026-11-26) 스탯 변화 화면 안 "새 스킬" 칸 (씬 StatChangeScreen/NewSkill): 스킬이 해방되는 레벨에 얻은 스킬을 자세히. 비우면 스탯 글씨에 같이 씀
    public LevelUpNewSkillPanel newSkillPanel;
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

    // (2026-11-26) 레벨업 한 번 = ① 용사 선택(Lv.1, 아직 안 골랐을 때만) → ② 용사 스탯 변화 화면(스킬 해방은 여기서 자동, 보여줄 게 없으면 건너뜀)
    //   → ③ 용사 강화 카드(그 레벨에 있을 때) → ④ 미니언 강화 카드. 한 단계를 마치면(CloseCanvas) 같은 레벨의 다음 단계로 다시 열림, ④까지 고르면 레벨업 소모
    // (이전 2026-11-25: ① 단계 카드(용사·스킬 해방 카드·용사 강화) → ② 미니언 강화)
    public enum Step { Hero, StatChange, HeroBoost, MinionEnhance }
    public Step CurrentStep { get; private set; } = Step.Hero;
    public bool IsStatStep => CurrentStep == Step.MinionEnhance; // 지금 창 = 미니언 강화 카드 단계 (이름은 이전 능력치 카드 단계 그대로)
    public bool IsStatChangeScreen => CurrentStep == Step.StatChange && statScreenRoot != null && statScreenRoot.gameObject.activeSelf;
    string statChangeScreenText; // 이번 레벨의 스탯 변화 화면 글씨 (스킬 해방은 한 번만 일어나서 저장해 둠)
    HeroKingAbility newSkillHero; // 이번 레벨에 해방한 스킬 (새 스킬 칸에 표시)
    int newSkillIdx = -1;

    public static bool CanOpen => !RuneRewardSystem.IsOpen && Instance != null && KingSlime.Instance != null && KingSlime.Instance.pendingLevelUps > 0
        && GameManager.Instance != null && GameManager.Instance.isBreakTime;

    public static void OpenPending()
    {
        if (!CanOpen || Instance.gameObject.activeSelf)
            return;
        Instance.OpenCanvas();
    }

    // (2026-11-26) 씬을 디자인하다 창을 켠 채 저장해도 게임 시작 때는 닫힌 상태로 (열려 있으면 OpenPending이 안 열림)
    bool openedByCode;
    void Start()
    {
        if (!openedByCode)
            gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (isPausing)
            Time.timeScale = prevTimeScale;
    }

    public override void CloseCanvas()
    {
        base.CloseCanvas();
        if (CurrentStep != Step.MinionEnhance) // 같은 레벨의 다음 단계
        {
            CurrentStep++;
            OpenCanvas();
            return;
        }
        CurrentStep = Step.Hero;
        statChangeScreenText = null;
        newSkillHero = null;
        newSkillIdx = -1;
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
        openedByCode = true;
        base.OpenCanvas(closeCallback);
        // (2026-11-26) 씬에서 카드를 지우면 배열에 빈 칸(null)이 남음 → 빼고 씀 (모자라면 EnsurePanels가 첫 카드를 복사)
        levelUpStatPanels = (levelUpStatPanels ?? new LevelUpStatPanel[0]).Where(p => p != null).ToArray();

        KingSlime king = KingSlime.Instance;
        KingGrowthSetting setting = KingGrowthSetting.Instance;
        List<AbilityData> choices = new List<AbilityData>();
        AbilityCategory stage = AbilityCategory.Stat;
        // 이번 단계에 보여줄 게 없으면 다음 단계로 (용사 → 스탯 변화 → 용사 강화 → 미니언 강화)
        while (true)
        {
            if (CurrentStep == Step.Hero || CurrentStep == Step.HeroBoost)
            {
                stage = king != null && setting != null ? LevelUpChoiceRule.GetStage(MakeChoiceInput(king, setting)) : AbilityCategory.Stat;
                AbilityCategory want = CurrentStep == Step.Hero ? AbilityCategory.Hero : AbilityCategory.HeroBoost;
                if (stage == want)
                {
                    choices = LevelUpChoiceRule.GetChoices(MakeChoiceInput(king, setting), rng);
                    if (choices.Count > 0)
                        break;
                }
                stage = AbilityCategory.Stat;
                CurrentStep++;
                continue;
            }
            if (CurrentStep == Step.StatChange)
            {
                if (statChangeScreenText == null)
                    statChangeScreenText = PrepareStatChange(king, setting); // 스킬 해방도 여기서
                if (!string.IsNullOrEmpty(statChangeScreenText))
                    break;
                CurrentStep++;
                continue;
            }
            choices = PickMinionEnhanceChoices();
            break;
        }

        heroSelectCards = (heroSelectCards ?? new HeroSelectCard[0]).Where(c => c != null).ToArray();
        SetHeroContainerActive(false);
        if (CurrentStep == Step.StatChange)
        {
            ShowStatScreen(king, statChangeScreenText);
            return;
        }
        if (statScreenRoot != null)
            statScreenRoot.gameObject.SetActive(false);

        // 버프 칸 확장 카드: 카드마다 추가 칸 (지금 버프 칸을 뺀 나머지 중 랜덤, 카드끼리 다르게)
        List<Vector2Int> currentTiles = king != null ? king.abilityInventory.GetBuffTileOffsets() : new List<Vector2Int>();
        List<Vector2Int> expandTiles = LevelUpChoiceRule.PickExpandTiles(currentTiles, choices.Count, rng);

        bool useHeroCards = stage == AbilityCategory.Hero && heroSelectCards.Length > 0;
        if (stage == AbilityCategory.Hero && !useHeroCards)
            Debug.LogWarning("[LevelUpCanvas] 용사 선택 카드(heroSelectCards)가 비어 있음 → 메뉴 Tools/InGame/Add Hero Select Cards To LevelUpCanvas");
        EnsurePanels(useHeroCards ? 0 : choices.Count);
        SetCardContainerActive(!useHeroCards);
        if (useHeroCards)
            ShowHeroCards(choices);
        ShowReward();
        HideOldStatChangeText();
        if (headerText != null)
            headerText.text = king != null ? $"레벨 업! Lv.{king.ChoiceLevel}" : "레벨 업!";
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
        foreach (var panel in useHeroCards ? new LevelUpStatPanel[0] : levelUpStatPanels)
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
    bool rerollHooked;

    void UpdateRerollButton(bool show)
    {
        show &= Reroll.CanReroll;
        if (rerollButton == null)
            return; // 씬에 연결 (Tools/InGame/Add LevelUp Screens To LevelUpCanvas)
        if (!rerollHooked)
        {
            rerollHooked = true;
            rerollButton.onClick.AddListener(OnClickedReroll);
        }
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
    // 에디터 빌더(LevelUpScreensBuilder)가 씬에 만들 때 씀
    public void BuildRerollButton()
    {
        LevelUpStatPanel first = levelUpStatPanels != null ? levelUpStatPanels.FirstOrDefault(p => p != null) : null;
        UnityEngine.UI.Button template = GetTemplateButton(first);
        if (template == null)
            return;
        // (2026-11-26) 카드 줄(LevelUpContainer) 맨 아래 칸 = 카드 아래에 붙어 같이 켜지고 꺼짐 (이전: 창 루트, 카드 아래 위치를 계산)
        Transform parent = first != null && first.transform.parent != null ? first.transform.parent : transform;
        UnityEngine.UI.Button btn = Instantiate(template, parent);
        btn.name = "MinionEnhanceRerollButton";
        btn.onClick = new UnityEngine.UI.Button.ButtonClickedEvent(); // 복사된 버튼 연결 제거
        btn.transform.SetAsLastSibling();
        RectTransform rt = (RectTransform)btn.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, 320f), Mathf.Max(rt.sizeDelta.y, 90f));
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
            heroPool = GetOwnedHeroPool(setting), // (2026-11-26) 보유한 용사만
            boostPool = setting.heroBoostAbilities,
            getLevel = inv.GetLevel,
        };
    }

    // (2026-11-26) 용사 선택 카드 후보 = 보유한 용사 (UserHeroManager). 보유 정보가 없거나 하나도 없으면 전체
    public static IList<AbilityData> GetOwnedHeroPool(KingGrowthSetting setting)
    {
        if (setting == null || setting.heroAbilities == null)
            return new List<AbilityData>();
        UserHeroManager heroes = UserDataManager.Instance != null ? UserDataManager.Instance.userHeroManager : null;
        if (heroes == null)
            return setting.heroAbilities;
        List<AbilityData> owned = setting.heroAbilities.Where(d => d != null && d.prefab is HeroKingAbility h && heroes.IsOwned(h.HeroDataKey)).ToList();
        return owned.Count > 0 ? owned : (IList<AbilityData>)setting.heroAbilities;
    }

    #region 레벨업 능력치 변화
    // 화면 표시 순서
    static readonly StatType[] HERO_STAT_DISPLAY = { StatType.AttackPower, StatType.AttackSpeed, StatType.CritChance, StatType.AttackRange, StatType.MaxHp };

    // 용사 레벨 toLevel-1 → toLevel 변화 ("공격력 : 15 → 23" 줄들). 용사가 없거나 바뀌는 게 없으면 ""
    // (2026-11-26) Lv.1(용사를 막 고름) = 킹 기본(KingSlimeData) → 용사 Lv.1 스탯 + 정예병 칸 버프
    public static string GetHeroLevelUpText(KingSlime king, int toLevel)
    {
        HeroKingAbility hero = king != null && king.abilityInventory != null ? king.abilityInventory.Get<HeroKingAbility>() : null;
        if (hero == null || toLevel < 1 || king.statMgr == null)
            return "";
        HeroDataTable.HeroData data = hero.HeroData;
        bool first = toLevel <= 1;
        var lines = new List<string>();
        if (data != null)
            foreach (StatType stat in HERO_STAT_DISPLAY)
            {
                float from = first ? king.statMgr.GetHeroStat(null, stat, 1) : king.statMgr.GetHeroStat(data, stat, toLevel - 1);
                float to = king.statMgr.GetHeroStat(data, stat, toLevel);
                if (Mathf.Approximately(from, to))
                    continue;
                string unit = stat == StatType.CritChance ? "%" : "";
                lines.Add($"{GetStatName(stat)} : {KingAbility.FormatNumber(from)}{unit} → <color=#7CFF6B>{KingAbility.FormatNumber(to)}{unit}</color>");
            }
        string eliteTo = hero.GetEliteBuffText(toLevel);
        if (first)
        {
            if (!string.IsNullOrEmpty(eliteTo))
                lines.Add($"정예 버프 : <color=#7CFF6B>{eliteTo}</color>");
            return string.Join("\n", lines);
        }
        string eliteFrom = hero.GetEliteBuffText(toLevel - 1);
        if (eliteFrom != eliteTo)
            lines.Add($"정예 버프 : {eliteFrom} → <color=#7CFF6B>{eliteTo}</color>");
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

    void HideOldStatChangeText()
    {
        if (statChangeText != null)
            statChangeText.gameObject.SetActive(false);
    }

    // (2026-11-26) 스탯 변화 화면 글씨. 밀린 스킬 해방(Lv.2·6·10)을 여기서 해방하고 같이 적음. 보여줄 게 없으면 ""
    string PrepareStatChange(KingSlime king, KingGrowthSetting setting)
    {
        if (king == null)
            return "";
        HeroKingAbility hero = king.abilityInventory.Get<HeroKingAbility>();
        var lines = new List<string>();
        string stats = GetHeroLevelUpText(king, king.ChoiceLevel);
        if (!string.IsNullOrEmpty(stats))
            lines.Add(stats);
        // 필드 최대 미니언 수 (Lv.7 +1, 적용은 레벨업을 마칠 때 HeroKingAbility.RefreshHeroLevel)
        int minionAdd = LevelUpChoiceRule.GetMinionCountAdd(king.ChoiceLevel) - LevelUpChoiceRule.GetMinionCountAdd(king.ChoiceLevel - 1);
        if (hero != null && minionAdd > 0 && MinionFieldController.Instance != null)
        {
            int cur = MinionFieldController.Instance.minionMaxCount;
            lines.Add($"필드 최대 미니언 수 : {cur} → <color=#7CFF6B>{cur + minionAdd}</color>");
        }
        // 스킬 해방
        if (hero != null && setting != null)
        {
            int guard = 0;
            while (guard++ < HeroKingAbility.SKILL_COUNT && LevelUpChoiceRule.GetStage(MakeChoiceInput(king, setting)) == AbilityCategory.HeroSkill)
            {
                int idx = hero.NextSkillIndex;
                if (idx < 0)
                    break;
                hero.UnlockNextSkill();
                if (newSkillPanel != null && newSkillIdx < 0) // 첫 해방 스킬은 새 스킬 칸에 자세히 (한 번에 둘 이상이면 나머지는 글씨로)
                {
                    newSkillHero = hero;
                    newSkillIdx = idx;
                    continue;
                }
                string stageName = idx == HeroKingAbility.COMMON_SKILL_INDEX ? "3단계 공용 스킬" : $"{idx + 1}단계 스킬";
                string skill = $"\n<color=#FFD54A><size=115%><b>{stageName} 해방! {hero.GetSkillName(idx)}</b></size></color>\n<size=85%>{hero.GetSkillDesc(idx)}</size>";
                if (idx == 0) // 에너지는 1단계 해방 때 최대 2개
                    skill += $"\n<size=85%><color=#{ColorUtility.ToHtmlStringRGB(hero.energyColor)}>{hero.energyName}</color> 최대 {GameSetting.HERO_ENERGY_BASE_MAX}개 (하단 스킬 버튼으로 사용)</size>";
                lines.Add(skill);
            }
        }
        string result = string.Join("\n", lines).Trim('\n');
        // 스탯 변화가 없어도 새 스킬이 있으면 화면을 보여 줘야 함 (빈 글씨 = 화면 건너뜀)
        return string.IsNullOrEmpty(result) && newSkillIdx >= 0 ? " " : result;
    }

    // (2026-11-27) 용사 선택 카드: 카드마다 정예 버프 칸 랜덤 2칸
    void ShowHeroCards(List<AbilityData> choices)
    {
        var cards = heroSelectCards.ToList();
        while (cards.Count < choices.Count)
        {
            HeroSelectCard copy = Instantiate(cards[0], cards[0].transform.parent);
            copy.name = $"{cards[0].name}_{cards.Count}";
            copy.transform.SetSiblingIndex(cards[cards.Count - 1].transform.GetSiblingIndex() + 1);
            cards.Add(copy);
        }
        heroSelectCards = cards.ToArray();
        SetHeroContainerActive(true);
        for (int i = 0; i < heroSelectCards.Length; i++)
        {
            bool active = i < choices.Count;
            heroSelectCards[i].gameObject.SetActive(active);
            if (!active)
                continue;
            heroSelectCards[i].Set(choices[i], LevelUpChoiceRule.PickHeroTiles(rng));
            heroSelectCards[i].PlayAppear(i);
        }
    }

    void SetHeroContainerActive(bool active)
    {
        HeroSelectCard first = heroSelectCards != null ? heroSelectCards.FirstOrDefault(c => c != null) : null;
        Transform container = first != null ? first.transform.parent : null;
        if (container != null && container != transform)
            container.gameObject.SetActive(active);
    }

    // 카드 줄(카드의 부모, 씬 LevelUpContainer): 카드 단계에선 켬, 스탯 변화 화면에선 끔 (씬에서 꺼 둔 채 저장해도 동작)
    void SetCardContainerActive(bool active)
    {
        LevelUpStatPanel first = levelUpStatPanels != null ? levelUpStatPanels.FirstOrDefault(p => p != null) : null;
        Transform container = first != null ? first.transform.parent : null;
        if (container != null && container != transform)
            container.gameObject.SetActive(active);
    }

    void ShowStatScreen(KingSlime king, string text)
    {
        SetCardContainerActive(false);
        foreach (var panel in levelUpStatPanels ?? new LevelUpStatPanel[0])
            if (panel != null)
                panel.gameObject.SetActive(false);
        UpdateRerollButton(false);
        HideOldStatChangeText();
        ShowReward();
        if (headerText != null)
            headerText.text = king != null ? $"레벨 업! Lv.{king.ChoiceLevel}" : "레벨 업!";
        if (subText != null)
            subText.text = king != null && king.ChoiceLevel <= LevelUpChoiceRule.HERO_LEVEL ? "용사의 능력치" : "용사가 강해졌어요!";
        if (statScreenRoot == null) // 씬에 연결 안 됨 → 건너뜀 (Tools/InGame/Add LevelUp Screens To LevelUpCanvas)
        {
            Debug.LogWarning("[LevelUpCanvas] 스탯 변화 화면(statScreenRoot) 연결 없음 → 메뉴 Tools/InGame/Add LevelUp Screens To LevelUpCanvas");
            CloseCanvas();
            return;
        }
        statScreenRoot.gameObject.SetActive(true);
        if (statScreenConfirmButton != null && !confirmHooked) // 씬에서 연결한 버튼도 코드에서 연결
        {
            confirmHooked = true;
            statScreenConfirmButton.onClick.RemoveListener(OnClickedStatScreenConfirm);
            statScreenConfirmButton.onClick.AddListener(OnClickedStatScreenConfirm);
        }
        if (statScreenText != null)
            statScreenText.text = text;
        if (newSkillPanel != null)
        {
            if (newSkillIdx >= 0)
                newSkillPanel.Show(newSkillHero, newSkillIdx);
            else
                newSkillPanel.Hide();
        }
        statScreenRoot.DOKill();
        statScreenRoot.localScale = Vector3.one * 0.8f;
        statScreenRoot.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    bool confirmHooked;

    // 확인 → 같은 레벨의 다음 단계 (용사 강화 / 미니언 강화)
    public void OnClickedStatScreenConfirm()
    {
        if (CurrentStep != Step.StatChange || !gameObject.activeSelf)
            return;
        statScreenRoot.gameObject.SetActive(false);
        CloseCanvas();
    }

    // 기본 모양: 카드 영역 가운데 어두운 판 + 큰 글씨 + 아래 확인 버튼(카드 선택 버튼 복사)
    // 에디터 빌더(LevelUpScreensBuilder)가 씬에 만들 때 씀
    public void BuildStatScreen()
    {
        LevelUpStatPanel first = levelUpStatPanels != null ? levelUpStatPanels.FirstOrDefault(p => p != null) : null;
        RectTransform container = first != null ? first.transform.parent as RectTransform : null;
        RectTransform parent = container != null ? container.parent as RectTransform : transform as RectTransform;
        if (parent == null)
            return;
        var go = new GameObject("StatChangeScreen", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        statScreenRoot = (RectTransform)go.transform;
        statScreenRoot.SetParent(parent, false);
        if (container != null)
        {
            statScreenRoot.anchorMin = container.anchorMin;
            statScreenRoot.anchorMax = container.anchorMax;
            statScreenRoot.pivot = container.pivot;
            statScreenRoot.anchoredPosition = container.anchoredPosition;
            statScreenRoot.SetSiblingIndex(container.GetSiblingIndex() + 1);
        }
        else
            statScreenRoot.anchorMin = statScreenRoot.anchorMax = statScreenRoot.pivot = new Vector2(0.5f, 0.5f);
        statScreenRoot.sizeDelta = new Vector2(900f, 760f);
        var bg = go.GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image cardBg = first != null ? first.GetComponent<UnityEngine.UI.Image>() : null;
        if (cardBg != null && cardBg.sprite != null)
        {
            bg.sprite = cardBg.sprite;
            bg.type = cardBg.type;
        }
        bg.color = new Color(0.08f, 0.08f, 0.14f, 0.92f);

        TMPro.TMP_Text fontSrc = headerText != null ? headerText : GetComponentInChildren<TMPro.TMP_Text>(true);
        var textGo = new GameObject("StatChangeText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        var trt = (RectTransform)textGo.transform;
        trt.SetParent(statScreenRoot, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(50f, 150f);
        trt.offsetMax = new Vector2(-50f, -50f);
        var t = textGo.GetComponent<TMPro.TextMeshProUGUI>();
        if (fontSrc != null)
        {
            t.font = fontSrc.font;
            t.fontSharedMaterial = fontSrc.fontSharedMaterial;
        }
        t.fontSize = 42f;
        t.enableAutoSizing = true;
        t.fontSizeMin = 22f;
        t.fontSizeMax = 46f;
        t.color = Color.white;
        t.alignment = TMPro.TextAlignmentOptions.Center;
        t.raycastTarget = false;
        statScreenText = t;

        UnityEngine.UI.Button template = GetTemplateButton(first);
        if (template != null)
        {
            UnityEngine.UI.Button btn = Instantiate(template, statScreenRoot);
            btn.name = "StatChangeConfirmButton";
            btn.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            btn.gameObject.SetActive(true);
            RectTransform rt = (RectTransform)btn.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, 320f), Mathf.Max(rt.sizeDelta.y, 90f));
            rt.anchoredPosition = new Vector2(0f, 40f);
            var label = btn.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (label != null)
            {
                foreach (var tt in label.GetComponents<TranslateText>())
                    DestroyImmediate(tt);
                label.text = "확인";
            }
            statScreenConfirmButton = btn;
        }
        else
            Debug.LogWarning("[LevelUpCanvas] 스탯 변화 화면 확인 버튼을 만들 카드 버튼이 없음");
    }
    #endregion

    void ShowReward()
    {
        if (rewardPanel == null)
            return; // 씬에 연결 (Tools/InGame/Add LevelUp Screens To LevelUpCanvas)
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
    // 에디터 빌더(LevelUpScreensBuilder)가 씬에 만들 때 씀
    public void BuildRewardPanel()
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

    // 빌더가 버튼을 만들 때 복사할 모양: 카드 안 버튼 → (2026-11-26 카드 선택 버튼 삭제) 씬의 확인·새로고침 버튼
    UnityEngine.UI.Button GetTemplateButton(LevelUpStatPanel first)
    {
        UnityEngine.UI.Button b = first != null ? first.GetComponentsInChildren<UnityEngine.UI.Button>(true).LastOrDefault() : null;
        return b != null ? b : statScreenConfirmButton != null ? statScreenConfirmButton : rerollButton;
    }

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
            // 카드 줄 안에 다른 것(새로고침 버튼 등)이 있어도 카드끼리 붙어 있게 마지막 카드 바로 뒤에
            copy.transform.SetSiblingIndex(panels[panels.Count - 1].transform.GetSiblingIndex() + 1);
            panels.Add(copy);
        }
        levelUpStatPanels = panels.ToArray();
    }
}
}
