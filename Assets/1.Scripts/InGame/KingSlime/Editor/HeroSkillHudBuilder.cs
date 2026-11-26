using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// (2026-11-24) InGame 씬 BottomGameCanvas에 wave 중 하단 UI(HeroSkillHud: 킹 체력·에너지·스킬 버튼)를 실제 오브젝트로 만든다
// OfferButtons 바로 옆(같은 부모)에 기본 모양으로 생성 + BottomGameCanvas.heroSkillHud 연결. 이미 있으면 지우고 다시 만듦
// 씬은 저장하지 않고 dirty만 표시 → 확인 후 직접 저장. 이후 모양·위치·글씨 크기는 씬에서 마음대로 고치면 됨
public static class HeroSkillHudBuilder
{
    const string LOCK_SPRITE_PATH = "Assets/2.Sprites/Rune/preparation/UI_Common_Lock_01_Silver.png";
    static Sprite Plain => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    static Sprite CostSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); // 동그라미 (용사 energyIcon이 있으면 그 그림)

    // Slider 모양 = 하단 경험치바(OfferButtons/ExpBar/ExpSlider)
    static UnityEngine.UI.Slider FindSliderTemplate(BottomGameCanvas bottom) =>
        bottom != null && bottom.offerButtonObject != null ? bottom.offerButtonObject.GetComponentInChildren<UnityEngine.UI.Slider>(true) : null;

    // (2026-11-24) 씬의 HUD를 새 모양으로: 체력·충전 바 → Slider(경험치바 복사), 에너지 개수 칸 삭제(숫자만), 스킬 칸 "투지 1" → 소모 아이콘 줄
    // 직접 고친 위치·글씨 등은 그대로 두고 바뀐 부분만 교체
    [MenuItem("Tools/Hero/Update HeroSkillHud (Slider·Cost)")]
    public static string UpgradeHud()
    {
        var hud = Object.FindFirstObjectByType<HeroSkillHud>(FindObjectsInactive.Include);
        if (hud == null)
            return Log("HeroSkillHud가 열린 씬에 없음");
        var bottom = Object.FindFirstObjectByType<BottomGameCanvas>(FindObjectsInactive.Include);
        Undo.RegisterFullObjectHierarchyUndo(hud.gameObject, "Update HeroSkillHud");
        int changed = HeroSkillHud.Upgrade(hud, Plain, CostSprite, FindSliderTemplate(bottom));
        EditorUtility.SetDirty(hud);
        EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
        return Log($"바뀐 곳 {changed}개 (씬 저장은 직접)");
    }

    // (2026-11-24) 씬에 있는 HUD의 스킬 칸에 잠김 표시(자물쇠 + 필요 레벨)만 추가 (다른 수정은 건드리지 않음). 이미 있는 칸은 건너뜀
    [MenuItem("Tools/Hero/Add Skill Lock To HeroSkillHud")]
    public static string AddLocks()
    {
        var hud = Object.FindFirstObjectByType<HeroSkillHud>(FindObjectsInactive.Include);
        if (hud == null)
            return Log("HeroSkillHud가 열린 씬에 없음");
        Undo.RegisterFullObjectHierarchyUndo(hud.gameObject, "Add Skill Lock");
        Sprite lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LOCK_SPRITE_PATH);
        TMPro.TMP_FontAsset font = hud.GetComponentInChildren<TMPro.TMP_Text>(true)?.font;
        int added = 0;
        for (int i = 0; i < hud.slots.Length; i++)
        {
            var slot = hud.slots[i];
            if (slot == null || slot.lockObject != null)
                continue;
            HeroSkillHud.AddLock(slot, i, lockSprite, font);
            added++;
        }
        EditorUtility.SetDirty(hud);
        EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
        return Log($"잠김 표시 추가 {added}칸 (씬 저장은 직접)");
    }

    [MenuItem("Tools/Hero/Add HeroSkillHud To InGame Scene")]
    public static string Build()
    {
        var bottom = Object.FindFirstObjectByType<BottomGameCanvas>(FindObjectsInactive.Include);
        if (bottom == null)
            return Log("BottomGameCanvas가 열린 씬에 없음 (InGame 씬을 열고 실행)");
        if (bottom.offerButtonObject == null)
            return Log("BottomGameCanvas.offerButtonObject(OfferButtons) 연결 없음");

        Undo.RecordObject(bottom, "Add HeroSkillHud");
        var old = Object.FindFirstObjectByType<HeroSkillHud>(FindObjectsInactive.Include);
        if (old != null)
            Undo.DestroyObjectImmediate(old.gameObject);

        RectTransform reference = (RectTransform)bottom.offerButtonObject.transform;
        Sprite box = HeroSkillHud.FindSprite(reference.root, "Button_01_White_Bg");
        HeroSkillHud hud = HeroSkillHud.Build(reference, Plain, box, AssetDatabase.LoadAssetAtPath<Sprite>(LOCK_SPRITE_PATH), CostSprite, FindSliderTemplate(bottom));
        Undo.RegisterCreatedObjectUndo(hud.gameObject, "Add HeroSkillHud");
        bottom.heroSkillHud = hud;
        EditorUtility.SetDirty(bottom);
        EditorSceneManager.MarkSceneDirty(bottom.gameObject.scene);
        Selection.activeGameObject = hud.gameObject;
        return Log($"생성: {GetPath(hud.transform)} (씬 저장은 직접)");
    }

    static string GetPath(Transform t) => t.parent == null ? t.name : $"{GetPath(t.parent)}/{t.name}";

    static string Log(string msg)
    {
        Debug.Log($"[HeroSkillHudBuilder] {msg}");
        return msg;
    }
}
