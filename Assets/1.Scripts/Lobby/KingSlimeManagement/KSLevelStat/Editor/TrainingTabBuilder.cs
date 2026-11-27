using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// (2026-11-27) 로비 킹 관리 화면 [수련] 탭 정리 (에디터 메뉴, 실행 후 씬 저장)
// - 묶음 제목(LvBg ~ LvBg (5)) 오른쪽에 "남은 포인트 N" 글씨(TierRemain) → LevelStatContainer.tierRemainTexts에 연결 (있으면 지우고 새로)
// - 이름 Authority → Training: AuthorityContainer → TrainingContainer, AuthorityCategoryButton → TrainingCategoryButton,
//   AuthoritySlotPanel_T*_* → TrainingSlotPanel_T*_*, AuthorityInfoCanvas → TrainingInfoCanvas (안 쓰는 이전 권능 정보 창)
// - 번역 key: Lobby_Authority → Lobby_Training, Authority_TierTitle → Training_TierTitle
// 이후 글씨 위치·크기·색은 씬에서 직접 수정
public static class TrainingTabBuilder
{
    const string REMAIN_NAME = "TierRemain";

    [MenuItem("Tools/Lobby/Add Training Tier Remain Texts (Rename Authority)")]
    public static void Build()
    {
        var container = Object.FindFirstObjectByType<LevelStatContainer>(FindObjectsInactive.Include);
        if (container == null)
        {
            Debug.LogWarning("[TrainingTabBuilder] LevelStatContainer가 열린 씬에 없음 (Lobby 씬을 열고 실행)");
            return;
        }
        Undo.RecordObject(container, "Training Tab");

        // 묶음 제목 LvBg (순서 = 형제 순서)
        var lvBgs = container.GetComponentsInChildren<RectTransform>(true)
            .Where(t => t.name == "LvBg" || t.name.StartsWith("LvBg ("))
            .OrderBy(t => t.GetSiblingIndex()).ToList();
        var texts = new List<TMP_Text>();
        foreach (RectTransform bg in lvBgs)
        {
            Transform old = bg.Find(REMAIN_NAME);
            if (old != null)
                Undo.DestroyObjectImmediate(old.gameObject);
            TMP_Text titleStyle = bg.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault();

            var go = new GameObject(REMAIN_NAME, typeof(RectTransform), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(go, "Training Tab");
            var rt = (RectTransform)go.transform;
            rt.SetParent(bg, false);
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(300f, 70f);
            rt.anchoredPosition = new Vector2(-30f, 13f);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (titleStyle != null)
            {
                tmp.font = titleStyle.font;
                tmp.fontSharedMaterial = titleStyle.fontSharedMaterial;
            }
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.MidlineRight;
            tmp.color = container.remainColor;
            tmp.raycastTarget = false;
            tmp.text = "남은 포인트 0";
            texts.Add(tmp);
        }
        container.tierRemainTexts = texts.ToArray();

        // 이름 Authority → Training
        int renamed = 0;
        foreach (Transform t in container.transform.root.GetComponentsInChildren<Transform>(true).ToList())
            renamed += Rename(t);
        foreach (GameObject root in container.gameObject.scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name.Contains("Authority"))
                    renamed += Rename(t);

        // 번역 key
        int keys = 0;
        foreach (var tt in Object.FindObjectsByType<TranslateText>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string newKey = tt.key == "Lobby_Authority" ? "Lobby_Training" : tt.key == "Authority_TierTitle" ? "Training_TierTitle" : null;
            if (newKey == null)
                continue;
            Undo.RecordObject(tt, "Training Tab");
            tt.key = newKey;
            EditorUtility.SetDirty(tt);
            keys++;
        }

        EditorUtility.SetDirty(container);
        EditorSceneManager.MarkSceneDirty(container.gameObject.scene);
        Selection.activeGameObject = container.gameObject;
        Debug.Log($"[TrainingTabBuilder] 남은 포인트 글씨 {texts.Count}개, 이름 변경 {renamed}개, 번역 key {keys}개 (씬 저장은 직접)");
    }

    static int Rename(Transform t)
    {
        if (!t.name.Contains("Authority"))
            return 0;
        Undo.RecordObject(t.gameObject, "Training Tab");
        t.name = t.name.Replace("Authority", "Training");
        return 1;
    }
}
