using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 상점 탭: ShopCanvas/Panel/Bg/Top/CategoryButtons 하위에 탭 버튼 3개(Package·Hero·Minion, TapButton idx 0·1·2)
// + Bg 하위 Package_Scroll·Hero_Scroll·Minion_Scroll에 TapPanel(idx 0·1·2) + Bg에 TapContainer(버튼·패널 연결)
// 버튼 모양 = 킹 관리 화면 탭 버튼(TrainingCategoryButton) 복사. 선택된 탭 = ColorSetting.activeColor, 나머지 = inactiveColor (TapButton)
// 다시 실행하면 버튼은 지우고 새로 만듦. 이후 모양은 씬에서 직접 수정
public static class ShopCategoryTabBuilder
{
    static readonly (string button, string scroll, string key)[] TABS =
    {
        ("PackageCategoryButton", "Package_Scroll", "Shop_Tab_Package"),
        ("HeroCategoryButton", "Hero_Scroll", "Shop_Tab_Hero"),
        ("MinionCategoryButton", "Minion_Scroll", "Shop_Tab_Minion"),
    };

    [MenuItem("Tools/Lobby/Add Shop Category Tabs")]
    public static void Build()
    {
        ShopCanvas shop = Object.FindFirstObjectByType<ShopCanvas>(FindObjectsInactive.Include);
        if (shop == null)
        {
            Debug.LogWarning("[ShopCategoryTabBuilder] ShopCanvas가 열린 씬에 없음 (Lobby 씬을 열고 실행)");
            return;
        }
        Transform bg = FindChild(shop.transform, "Bg");
        Transform category = FindChild(shop.transform, "CategoryButtons");
        if (bg == null || category == null)
        {
            Debug.LogWarning("[ShopCategoryTabBuilder] ShopCanvas에 Bg 또는 CategoryButtons가 없음");
            return;
        }
        KSMMenuTapButton srcBtn = Object.FindObjectsByType<KSMMenuTapButton>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        if (srcBtn == null)
        {
            Debug.LogWarning("[ShopCategoryTabBuilder] 복사할 탭 버튼(KSMMenuTapButton)이 씬에 없음");
            return;
        }

        // 같은 이름(앞뒤 공백 무시)의 버튼은 지우고 새로
        foreach (Transform old in category.Cast<Transform>().ToArray())
            if (TABS.Any(t => t.button == old.name.Trim()))
                Undo.DestroyObjectImmediate(old.gameObject);

        float width = (((RectTransform)category).sizeDelta.x - 20f * (TABS.Length - 1)) / TABS.Length;
        HorizontalLayoutGroup hlg = category.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            Undo.RecordObject(hlg, "Shop tabs");
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
        }

        var buttons = new TapButton[TABS.Length];
        var panels = new TapPanel[TABS.Length];
        for (int i = 0; i < TABS.Length; i++)
        {
            var tab = TABS[i];
            GameObject go = Object.Instantiate(srcBtn.gameObject, category);
            Undo.RegisterCreatedObjectUndo(go, "Shop tabs");
            go.name = tab.button;
            go.SetActive(true);
            Object.DestroyImmediate(go.GetComponent<KSMMenuTapButton>());
            TapButton tb = go.AddComponent<TapButton>();
            tb.idx = i;
            tb.button = go.GetComponent<Button>();
            tb.image = go.GetComponent<Image>();
            tb.image.color = i == 0 ? ColorSetting.activeColor : ColorSetting.inactiveColor;
            ((RectTransform)go.transform).sizeDelta = new Vector2(width, ((RectTransform)srcBtn.transform).sizeDelta.y);

            TranslateText tt = go.GetComponentInChildren<TranslateText>(true);
            if (tt != null)
            {
                tt.key = tab.key;
                tt.formatArg = null;
                TMP_Text label = tt.text != null ? tt.text : tt.GetComponent<TMP_Text>();
                if (label != null)
                    label.text = TranslateManager.GetText(tab.key);
            }
            buttons[i] = tb;

            Transform scroll = bg.Find(tab.scroll);
            if (scroll == null)
            {
                Debug.LogWarning($"[ShopCategoryTabBuilder] Bg 하위에 {tab.scroll}가 없음");
                continue;
            }
            TapPanel tp = scroll.GetComponent<TapPanel>();
            if (tp == null)
                tp = Undo.AddComponent<TapPanel>(scroll.gameObject);
            Undo.RecordObject(tp, "Shop tabs");
            tp.idx = i;
            Undo.RecordObject(scroll.gameObject, "Shop tabs");
            scroll.gameObject.SetActive(i == 0);
            panels[i] = tp;
        }

        TapContainer container = bg.GetComponent<TapContainer>();
        if (container == null)
            container = Undo.AddComponent<TapContainer>(bg.gameObject);
        var so = new SerializedObject(container);
        SetArray(so.FindProperty("tapButtons"), buttons);
        SetArray(so.FindProperty("tapPanels"), panels.Where(p => p != null).ToArray());
        so.FindProperty("selectedIdx").intValue = 0;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);
        Debug.Log("[ShopCategoryTabBuilder] 상점 탭 버튼 3개·탭 패널 연결 완료 (씬 저장은 직접)");
    }

    static void SetArray(SerializedProperty prop, Object[] items)
    {
        prop.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
    }

    static Transform FindChild(Transform root, string name) =>
        root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
}
