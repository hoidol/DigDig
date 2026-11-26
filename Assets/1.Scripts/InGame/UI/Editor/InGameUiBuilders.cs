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
