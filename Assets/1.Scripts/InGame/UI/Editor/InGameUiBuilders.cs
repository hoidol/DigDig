using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) InGame 씬 UI를 실제 오브젝트로 만들어 연결하는 에디터 메뉴 (실행 중 생성 대신. CLAUDE.md "UI 작성 규칙")
// 만든 뒤 모양·위치는 씬에서 직접 수정. 씬 저장은 직접
public static class InGameUiBuilders
{
    // 5성 진화 선택 창: 씬 루트 MinionEvolutionCanvas (오버레이 정렬 140, 갈래 칸 2개, 처음엔 패널 꺼 둠). 이미 있으면 지우고 새로
    [MenuItem("Tools/InGame/Add MinionEvolutionCanvas To Scene")]
    public static void AddEvolutionCanvas()
    {
        var old = Object.FindFirstObjectByType<MinionEvolutionCanvas>(FindObjectsInactive.Include);
        if (old != null)
            Undo.DestroyObjectImmediate(old.gameObject);
        var go = new GameObject(nameof(MinionEvolutionCanvas), typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Add MinionEvolutionCanvas");
        var canvas = go.AddComponent<MinionEvolutionCanvas>();
        canvas.BuildUI();
        canvas.panel.SetActive(false);
        Done(go, "MinionEvolutionCanvas");
    }

    // 진화 발동 스킬 버튼: GameWorldCanvas의 미니언 머리 위 상태 템플릿(MinionGameStatePanel) 하위 (미니언마다 복사됨, 처음엔 꺼 둠)
    [MenuItem("Tools/InGame/Add Evolution Skill Button To State Panel")]
    public static void AddEvolutionSkillButton()
    {
        var world = Object.FindFirstObjectByType<GameWorldCanvas>(FindObjectsInactive.Include);
        MinionGameStatePanel template = world != null ? (world.minionStateTemplate != null ? world.minionStateTemplate : world.GetComponentInChildren<MinionGameStatePanel>(true)) : null;
        if (template == null)
        {
            Debug.LogWarning("[InGameUiBuilders] GameWorldCanvas의 MinionGameStatePanel 템플릿이 없음 (InGame 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(template, "Add Evolution Skill Button");
        if (template.skillButton != null)
            Undo.DestroyObjectImmediate(template.skillButton.gameObject);
        template.skillButton = template.CreateSkillButtonForEditor();
        Undo.RegisterCreatedObjectUndo(template.skillButton.gameObject, "Add Evolution Skill Button");
        if (world.GetComponent<GraphicRaycaster>() == null) // 월드 캔버스에서 눌리게
            Undo.AddComponent<GraphicRaycaster>(world.gameObject);
        EditorUtility.SetDirty(template);
        Done(template.skillButton.gameObject, "MinionGameStatePanel/EvolutionSkillButton");
    }

    // 레벨업 창: 스탯 변화 화면(StatChangeScreen) · 미니언 강화 새로고침 버튼 · 레벨업 보상 줄 — 연결이 빈 것만 새로 (이미 있으면 그대로)
    [MenuItem("Tools/InGame/Add LevelUp Screens To LevelUpCanvas")]
    public static void AddLevelUpScreens()
    {
        var levelUp = Object.FindFirstObjectByType<InGame.LevelUpCanvas>(FindObjectsInactive.Include);
        if (levelUp == null)
        {
            Debug.LogWarning("[InGameUiBuilders] LevelUpCanvas가 없음 (InGame 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(levelUp, "Add LevelUp Screens");
        string made = "";
        if (levelUp.statScreenRoot == null)
        {
            levelUp.BuildStatScreen();
            if (levelUp.statScreenRoot != null)
            {
                Undo.RegisterCreatedObjectUndo(levelUp.statScreenRoot.gameObject, "Add LevelUp Screens");
                levelUp.statScreenRoot.gameObject.SetActive(false);
                if (levelUp.statScreenText != null)
                    levelUp.statScreenText.text = "공격력 : 15 → <color=#7CFF6B>20</color>";
                made += " StatChangeScreen";
            }
        }
        if (levelUp.rerollButton == null)
        {
            levelUp.BuildRerollButton();
            if (levelUp.rerollButton != null)
            {
                Undo.RegisterCreatedObjectUndo(levelUp.rerollButton.gameObject, "Add LevelUp Screens");
                levelUp.rerollButton.gameObject.SetActive(false);
                if (levelUp.rerollText != null)
                    levelUp.rerollText.text = "새로고침";
                made += " RerollButton";
            }
        }
        if (levelUp.rewardPanel == null)
        {
            levelUp.BuildRewardPanel();
            if (levelUp.rewardPanel != null)
            {
                Undo.RegisterCreatedObjectUndo(levelUp.rewardPanel.gameObject, "Add LevelUp Screens");
                made += " RewardPanel";
            }
        }
        EditorUtility.SetDirty(levelUp);
        Done(levelUp.gameObject, $"LevelUpCanvas:{(made.Length > 0 ? made : " (이미 다 있음)")}");
    }

    // (2026-11-26) 레벨업 창: ① 스탯 변화 화면의 NewSkill(사용자가 카드 복사로 만든 칸) → 새 스킬 칸(LevelUpNewSkillPanel)으로 연결
    //   ② 미니언 강화 새로고침 버튼을 카드 줄(LevelUpContainer) 맨 아래로 옮김 (레이아웃이 카드 아래에 배치, 카드 줄과 같이 켜지고 꺼짐)
    [MenuItem("Tools/InGame/Connect LevelUp NewSkill · Move Reroll Into Cards")]
    public static void ConnectLevelUpNewSkill()
    {
        var levelUp = Object.FindFirstObjectByType<InGame.LevelUpCanvas>(FindObjectsInactive.Include);
        if (levelUp == null)
        {
            Debug.LogWarning("[InGameUiBuilders] LevelUpCanvas가 없음 (InGame 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(levelUp, "Connect LevelUp NewSkill");
        string made = "";

        // ① 새 스킬 칸
        Transform newSkill = levelUp.statScreenRoot != null ? levelUp.statScreenRoot.Find("NewSkill") : null;
        if (newSkill == null)
            Debug.LogWarning("[InGameUiBuilders] StatChangeScreen/NewSkill이 없음 (카드를 복사해 이름을 NewSkill로)");
        else
        {
            var old = newSkill.GetComponent<LevelUpStatPanel>(); // 카드 복사본의 선택 기능은 빼야 함 (누르면 카드처럼 고르게 됨)
            if (old != null)
                Undo.DestroyObjectImmediate(old);
            var panel = newSkill.GetComponent<LevelUpNewSkillPanel>();
            if (panel == null)
                panel = Undo.AddComponent<LevelUpNewSkillPanel>(newSkill.gameObject);
            Undo.RecordObject(panel, "Connect LevelUp NewSkill");
            panel.headerText = FindComp<TMPro.TMP_Text>(newSkill, "TitleText (1)") ?? panel.headerText;
            panel.titleText = FindComp<TMPro.TMP_Text>(newSkill, "TitleText") ?? panel.titleText;
            panel.descText = FindComp<TMPro.TMP_Text>(newSkill, "DescText") ?? panel.descText;
            panel.thumImage = FindComp<Image>(newSkill, "IconBg/Thum") ?? panel.thumImage;
            panel.levelText = FindComp<TMPro.TMP_Text>(newSkill, "IconBg/LevelBadge/LevelText") ?? panel.levelText;
            panel.borderImage = FindComp<Image>(newSkill, "Border") ?? panel.borderImage;
            panel.iconBgImage = FindComp<Image>(newSkill, "IconBg") ?? panel.iconBgImage;
            EditorUtility.SetDirty(panel);
            levelUp.newSkillPanel = panel;
            made += " NewSkill";
        }

        // ② 새로고침 버튼 → 카드 줄 맨 아래
        LevelUpStatPanel card = levelUp.levelUpStatPanels != null ? System.Array.Find(levelUp.levelUpStatPanels, p => p != null) : null;
        Transform container = card != null ? card.transform.parent : null;
        if (levelUp.rerollButton != null && container != null && levelUp.rerollButton.transform.parent != container)
        {
            Undo.SetTransformParent(levelUp.rerollButton.transform, container, "Move Reroll Button");
            levelUp.rerollButton.transform.SetAsLastSibling();
            var rt = (RectTransform)levelUp.rerollButton.transform;
            Undo.RecordObject(rt, "Move Reroll Button");
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            made += " RerollButton→" + container.name;
        }
        EditorUtility.SetDirty(levelUp);
        Done(levelUp.gameObject, $"LevelUpCanvas:{(made.Length > 0 ? made : " (바꿀 것 없음)")}");
    }

    // (2026-11-27) Lv.1 용사 선택 카드: LevelUpCanvas 카드 줄(LevelUpContainer) 옆에 HeroSelectContainer + 카드 1장 (일반 카드 모양 복사 → HeroSelectCard)
    // 카드 = 왼쪽 아이콘 · 가운데 이름 + "정예 버프 : …" · 오른쪽 정예 버프 칸 3×3 (레벨 배지 등 다른 글씨 없음). 이미 있으면 지우고 새로
    [MenuItem("Tools/InGame/Add Hero Select Cards To LevelUpCanvas")]
    public static void AddHeroSelectCards()
    {
        var levelUp = Object.FindFirstObjectByType<InGame.LevelUpCanvas>(FindObjectsInactive.Include);
        LevelUpStatPanel src = levelUp != null && levelUp.levelUpStatPanels != null ? System.Array.Find(levelUp.levelUpStatPanels, p => p != null) : null;
        if (src == null)
        {
            Debug.LogWarning("[InGameUiBuilders] LevelUpCanvas 또는 레벨업 카드가 없음 (InGame 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(levelUp, "Add Hero Select Cards");
        var srcContainer = (RectTransform)src.transform.parent;
        Transform oldContainer = srcContainer.parent.Find("HeroSelectContainer");
        if (oldContainer != null)
            Undo.DestroyObjectImmediate(oldContainer.gameObject);

        // 카드 줄: LevelUpContainer와 같은 자리·같은 세로 레이아웃
        var containerGo = new GameObject("HeroSelectContainer", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(containerGo, "Add Hero Select Cards");
        var container = (RectTransform)containerGo.transform;
        container.SetParent(srcContainer.parent, false);
        container.SetSiblingIndex(srcContainer.GetSiblingIndex() + 1);
        container.anchorMin = srcContainer.anchorMin;
        container.anchorMax = srcContainer.anchorMax;
        container.pivot = srcContainer.pivot;
        container.sizeDelta = srcContainer.sizeDelta;
        container.anchoredPosition = srcContainer.anchoredPosition;
        var srcLayout = srcContainer.GetComponent<VerticalLayoutGroup>();
        if (srcLayout != null)
            EditorUtility.CopySerialized(srcLayout, containerGo.AddComponent<VerticalLayoutGroup>());

        // 카드 1장: 일반 카드 복사 → 컴포넌트 교체
        GameObject cardGo = Object.Instantiate(src.gameObject, container);
        cardGo.name = "HeroSelectCard";
        cardGo.SetActive(true);
        Object.DestroyImmediate(cardGo.GetComponent<LevelUpStatPanel>());
        var card = cardGo.AddComponent<HeroSelectCard>();
        Transform t = cardGo.transform;
        card.titleText = FindComp<TMPro.TMP_Text>(t, "TitleText");
        card.eliteBuffText = FindComp<TMPro.TMP_Text>(t, "DescText");
        card.thumImage = FindComp<Image>(t, "IconBg/Thum");
        card.iconBgImage = FindComp<Image>(t, "IconBg");
        card.borderImage = FindComp<Image>(t, "Border");
        Transform badge = t.Find("IconBg/LevelBadge");
        if (badge != null)
            Object.DestroyImmediate(badge.gameObject);

        // 배치 (카드 1000×300): 아이콘 왼쪽 · 글씨 가운데 · 칸 그림 오른쪽
        if (card.iconBgImage != null)
        {
            card.iconBgImage.gameObject.SetActive(true);
            Place(card.iconBgImage.rectTransform, new Vector2(-355f, 0f), new Vector2(230f, 230f));
        }
        if (card.titleText != null)
        {
            Place(card.titleText.rectTransform, new Vector2(-25f, 55f), new Vector2(390f, 80f));
            card.titleText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            card.titleText.text = "용사 이름";
        }
        if (card.eliteBuffText != null)
        {
            Place(card.eliteBuffText.rectTransform, new Vector2(-25f, -45f), new Vector2(390f, 110f));
            card.eliteBuffText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            card.eliteBuffText.fontSize = 40;
            card.eliteBuffText.enableAutoSizing = false;
            card.eliteBuffText.text = "정예 버프 : 공격력 +10%";
        }
        Transform grid = t.Find("ShapeGrid");
        if (grid != null)
        {
            grid.gameObject.SetActive(true);
            SetShapeGrid((RectTransform)grid, new Vector2(330f, 0f));
            card.shapeCells = new Image[9];
            for (int i = 0; i < 9 && i < grid.childCount; i++)
                card.shapeCells[i] = grid.GetChild(i).GetComponent<Image>();
        }
        EditorUtility.SetDirty(card);
        containerGo.SetActive(false);
        levelUp.heroSelectCards = new[] { card };
        EditorUtility.SetDirty(levelUp);
        Done(containerGo, "HeroSelectContainer/HeroSelectCard");
    }

    // (2026-11-27) 레벨업 카드(LevelUpCard: 미니언 강화·용사 강화) 안을 가로형으로: 왼쪽 아이콘(또는 정예 칸 그림) · 오른쪽 이름 + 설명
    // 카드 자체가 선택 (선택하기 버튼이 남아 있으면 지움). 이후 모양은 씬에서 직접 수정
    [MenuItem("Tools/InGame/Layout LevelUp Card Horizontal")]
    public static void LayoutLevelUpCardHorizontal()
    {
        var levelUp = Object.FindFirstObjectByType<InGame.LevelUpCanvas>(FindObjectsInactive.Include);
        LevelUpStatPanel card = levelUp != null && levelUp.levelUpStatPanels != null ? System.Array.Find(levelUp.levelUpStatPanels, p => p != null) : null;
        if (card == null)
        {
            Debug.LogWarning("[InGameUiBuilders] 레벨업 카드가 없음 (InGame 씬을 열고 실행)");
            return;
        }
        Transform t = card.transform;
        foreach (var rt in t.GetComponentsInChildren<RectTransform>(true))
            Undo.RecordObject(rt, "Layout LevelUp Card");
        foreach (var tmp in t.GetComponentsInChildren<TMPro.TMP_Text>(true))
            Undo.RecordObject(tmp, "Layout LevelUp Card");
        int removed = 0;
        foreach (var b in t.GetComponentsInChildren<Button>(true))
        {
            Undo.DestroyObjectImmediate(b.gameObject);
            removed++;
        }
        Transform icon = t.Find("IconBg");
        if (icon != null)
            Place((RectTransform)icon, new Vector2(-355f, 0f), new Vector2(230f, 230f));
        Transform grid = t.Find("ShapeGrid");
        if (grid != null)
            SetShapeGrid((RectTransform)grid, new Vector2(-355f, 0f));
        if (card.titleText != null)
        {
            Place(card.titleText.rectTransform, new Vector2(105f, 55f), new Vector2(620f, 80f));
            card.titleText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        }
        if (card.descriptionText != null)
        {
            Place(card.descriptionText.rectTransform, new Vector2(105f, -45f), new Vector2(620f, 120f));
            card.descriptionText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        }
        // 용사 선택 카드 칸 그림도 같은 칸 크기로
        if (levelUp.heroSelectCards != null)
            foreach (var hero in levelUp.heroSelectCards)
            {
                Transform hg = hero != null ? hero.transform.Find("ShapeGrid") : null;
                if (hg == null)
                    continue;
                Undo.RecordObject(hg, "Layout LevelUp Card");
                SetShapeGrid((RectTransform)hg, ((RectTransform)hg).anchoredPosition);
            }
        EditorUtility.SetDirty(card);
        Done(card.gameObject, $"LevelUpCard 가로형 (버튼 {removed}개 삭제)");
    }

    // 정예 칸 3×3 그림: 칸 72 · 간격 10 (GridLayoutGroup이 있으면 그걸로, 없으면 직접 배치)
    static void SetShapeGrid(RectTransform grid, Vector2 pos)
    {
        const float CELL = 72f, GAP = 10f;
        Place(grid, pos, Vector2.one * (CELL * 3 + GAP * 2));
        var layout = grid.GetComponent<GridLayoutGroup>();
        if (layout != null)
        {
            Undo.RecordObject(layout, "Shape Grid");
            layout.cellSize = Vector2.one * CELL;
            layout.spacing = Vector2.one * GAP;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            layout.childAlignment = TextAnchor.MiddleCenter;
            return;
        }
        for (int i = 0; i < 9 && i < grid.childCount; i++)
            Place((RectTransform)grid.GetChild(i), new Vector2((i % 3 - 1) * (CELL + GAP), -(i / 3 - 1) * (CELL + GAP)), Vector2.one * CELL);
    }

    // 가운데 기준으로 자리·크기
    static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    static T FindComp<T>(Transform root, string path) where T : Component
    {
        Transform t = root.Find(path);
        return t != null ? t.GetComponent<T>() : null;
    }

    // 룬 정보 패널 강화 버튼 (패널 아래 가운데, 처음엔 꺼 둠) — 비어 있을 때만
    [MenuItem("Tools/InGame/Add Rune Upgrade Button To Info Panel")]
    public static void AddRuneUpgradeButton()
    {
        var panel = Object.FindFirstObjectByType<TileRuneInfoPanel>(FindObjectsInactive.Include);
        if (panel == null)
        {
            Debug.LogWarning("[InGameUiBuilders] TileRuneInfoPanel이 없음 (InGame 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(panel, "Add Rune Upgrade Button");
        if (panel.upgradeButton == null)
        {
            panel.BuildUpgradeButton();
            Undo.RegisterCreatedObjectUndo(panel.upgradeButton.gameObject, "Add Rune Upgrade Button");
            if (panel.upgradeText != null)
                panel.upgradeText.text = "강화 Lv.2  주사위 3";
            panel.upgradeButton.gameObject.SetActive(false);
        }
        EditorUtility.SetDirty(panel);
        Done(panel.upgradeButton.gameObject, "TileRuneInfoPanel/RuneUpgradeButton");
    }

    // (2026-11-26) 브레이크타임 미니언 카드 다시 뽑기 버튼: Next 버튼을 복사해 그 위에 (MinionOfferSystem.rerollButton). 이미 있으면 지우고 새로
    [MenuItem("Tools/InGame/Add Offer Reroll Button")]
    public static void AddOfferRerollButton()
    {
        var offer = Object.FindFirstObjectByType<MinionOfferSystem>(FindObjectsInactive.Include);
        if (offer == null || offer.nextButton == null)
        {
            Debug.LogWarning("[InGameUiBuilders] MinionOfferSystem(nextButton 연결)이 없음 (InGame 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(offer, "Add Offer Reroll Button");
        if (offer.rerollButton != null)
            Undo.DestroyObjectImmediate(offer.rerollButton.gameObject);
        offer.rerollButton = offer.BuildRerollButtonForEditor();
        Undo.RegisterCreatedObjectUndo(offer.rerollButton.gameObject, "Add Offer Reroll Button");
        EditorUtility.SetDirty(offer);
        Done(offer.rerollButton.gameObject, "OfferButtons/RerollButton");
    }

    static void Done(GameObject go, string what)
    {
        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;
        Debug.Log($"[InGameUiBuilders] 생성: {what} (씬 저장은 직접)");
    }
}
