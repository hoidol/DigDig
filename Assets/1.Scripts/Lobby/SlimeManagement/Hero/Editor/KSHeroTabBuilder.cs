using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// (2026-11-26) 로비 슬라임 관리 화면 [용사] 탭을 Lobby 씬에 실제로 만듦 (에디터 메뉴, 실행 후 씬 저장)
// - HeroContainer: 수련 탭 창(테두리·스크롤) 복사 → 내용 비우고 3열 GridLayoutGroup + 용사 칸 템플릿(HeroSlot) 1개
// - HeroPreview: 슬라임 그림(SlimeBody)과 같은 자리 (그림·이름·설명), 처음엔 꺼 둠
// - 탭 글씨: 0 = 용사(KSM_Tab_Hero), 1 = 수련(KSM_Tab_Training) TranslateText key를 씬에 저장
// 이미 있으면 지우고 새로. 이후 모양·위치는 씬에서 직접 수정
public static class KSHeroTabBuilder
{
    [MenuItem("Tools/Lobby/Add Hero Tab To KSManagement")]
    public static void Build()
    {
        var canvas = Object.FindFirstObjectByType<KSManagementCanvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            Debug.LogWarning("[KSHeroTabBuilder] KSManagementCanvas가 열린 씬에 없음 (Lobby 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(canvas, "Add Hero Tab");
        if (canvas.heroContainer != null)
            Undo.DestroyObjectImmediate(canvas.heroContainer.gameObject);
        if (canvas.heroPreview != null)
            Undo.DestroyObjectImmediate(canvas.heroPreview.gameObject);
        canvas.heroContainer = null;
        canvas.heroPreview = null;
        if (canvas.slimeBody == null && canvas.kSStatStateContainer != null)
            canvas.slimeBody = canvas.kSStatStateContainer.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "SlimeBody")?.gameObject;

        canvas.BuildHeroContainer();
        if (canvas.heroContainer == null)
            return;
        Undo.RegisterCreatedObjectUndo(canvas.heroContainer.gameObject, "Add Hero Tab");
        HeroSlotPanel slot = HeroSlotPanel.Create(canvas.heroContainer.content, canvas.SlotStyleSource);
        slot.name = "HeroSlot_Template";
        if (slot.nameText != null)
            slot.nameText.text = "용사 이름";
        if (slot.lockText != null)
            slot.lockText.text = "Lv.1";
        canvas.heroContainer.slotTemplate = slot;

        canvas.BuildHeroPreview();
        Undo.RegisterCreatedObjectUndo(canvas.heroPreview.gameObject, "Add Hero Tab");
        canvas.heroPreviewName.text = "용사 이름";
        canvas.heroPreviewDesc.text = "용사 설명";
        canvas.heroPreview.gameObject.SetActive(false);

        foreach (var tt in canvas.GetComponentsInChildren<KSMMenuTapButton>(true).Select(b => b.GetComponentInChildren<TranslateText>(true)).Where(t => t != null))
            Undo.RecordObject(tt, "Add Hero Tab");
        canvas.SetTabLabels();

        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(canvas.heroContainer);
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Selection.activeGameObject = canvas.heroContainer.gameObject;
        Debug.Log("[KSHeroTabBuilder] [용사] 탭 생성: HeroContainer + HeroSlot_Template + HeroPreview (씬 저장은 직접)");
    }
}
