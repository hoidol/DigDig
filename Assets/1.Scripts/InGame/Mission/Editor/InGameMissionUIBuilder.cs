using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// InGame 씬에 미션 버튼·창을 만든다 (피해 집계 버튼·창을 복사해 모양을 맞춤). 이미 있으면 다시 만듦
// 씬은 저장하지 않고 dirty만 표시 → 확인 후 직접 저장
public static class InGameMissionUIBuilder
{
    const string ICON_PATH = "Assets/2.Sprites/Icons/UI_Common_Mission_01_Blue.png";
    const float BUTTON_SPACING = 10f;
    const float PANEL_WIDTH = 440f;
    const float LINE_FONT_SIZE = 26f;
    const float TITLE_FONT_SIZE = 30f;

    [MenuItem("Tools/Mission/Add Mission UI To InGame Scene")]
    public static string Build()
    {
        var damageButton = Object.FindFirstObjectByType<DamageMeterSystemButton>(FindObjectsInactive.Include);
        if (damageButton == null)
            return Log("DamageMeterSystemButton이 열린 씬에 없음 (InGame 씬을 열고 실행)");
        DamageMeterSystemContainer container = damageButton.container;
        if (container == null)
            return Log("DamageMeterSystemButton.container 연결 없음");

        // 다시 만들기: 기존 것 삭제
        var old = Object.FindFirstObjectByType<InGameMissionButton>(FindObjectsInactive.Include);
        if (old != null)
        {
            if (old.panel != null)
                Undo.DestroyObjectImmediate(old.panel);
            Undo.DestroyObjectImmediate(old.gameObject);
        }

        RectTransform damageRt = (RectTransform)damageButton.transform;
        RectTransform containerRt = (RectTransform)container.transform;
        Vector2 panelOffset = containerRt.anchoredPosition - damageRt.anchoredPosition;

        // 버튼: 피해 집계 버튼 복사 → 피해 집계 기능 제거 → 아래로
        GameObject buttonGo = Object.Instantiate(damageButton.gameObject, damageRt.parent);
        buttonGo.name = "MissionButton";
        Undo.RegisterCreatedObjectUndo(buttonGo, "Add Mission UI");
        Object.DestroyImmediate(buttonGo.GetComponent<DamageMeterSystemButton>());
        RectTransform buttonRt = (RectTransform)buttonGo.transform;
        buttonRt.SetSiblingIndex(damageRt.GetSiblingIndex() + 1);
        buttonRt.anchoredPosition = damageRt.anchoredPosition + new Vector2(0, -(damageRt.sizeDelta.y + BUTTON_SPACING));
        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_PATH);
        if (icon != null && buttonGo.TryGetComponent(out Image image))
        {
            image.sprite = icon;
            image.preserveAspect = true;
        }

        // 창: 피해 집계 창 복사(바탕·세로 정렬) → 내용 비움 → 제목 + 줄 템플릿
        TMP_Text fontSource = container.GetComponentInChildren<TMP_Text>(true);
        GameObject panelGo = Object.Instantiate(container.gameObject, containerRt.parent);
        panelGo.name = "MissionPanel";
        Undo.RegisterCreatedObjectUndo(panelGo, "Add Mission UI");
        Object.DestroyImmediate(panelGo.GetComponent<DamageMeterSystemContainer>());
        for (int i = panelGo.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(panelGo.transform.GetChild(i).gameObject);
        panelGo.SetActive(false);

        RectTransform panelRt = (RectTransform)panelGo.transform;
        panelRt.SetSiblingIndex(buttonRt.GetSiblingIndex() + 1);
        panelRt.anchoredPosition = buttonRt.anchoredPosition + panelOffset;
        panelRt.sizeDelta = new Vector2(PANEL_WIDTH, panelRt.sizeDelta.y);
        if (panelGo.TryGetComponent(out VerticalLayoutGroup layout))
        {
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
        }
        var fitter = panelGo.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = panelGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateText(panelGo.transform, "Title", fontSource, TITLE_FONT_SIZE, "미션", true, "Mission_Title");
        TMP_Text template = CreateText(panelGo.transform, "MissionLine_Template", fontSource, LINE_FONT_SIZE, "미션 내용 0/5", false, null);
        template.gameObject.SetActive(false);

        var missionPanel = panelGo.AddComponent<InGameMissionPanel>();
        missionPanel.lineTemplate = template;

        var missionButton = buttonGo.AddComponent<InGameMissionButton>();
        missionButton.panel = panelGo;
        missionButton.damageButton = damageButton;

        EditorSceneManager.MarkSceneDirty(buttonGo.scene);
        Selection.activeGameObject = buttonGo;
        return Log($"미션 UI 추가: {GetPath(buttonRt)}, {GetPath(panelRt)} (씬 저장은 직접)");
    }

    static TMP_Text CreateText(Transform parent, string name, TMP_Text fontSource, float size, string text, bool bold, string translateKey)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (fontSource != null)
        {
            tmp.font = fontSource.font;
            tmp.fontSharedMaterial = fontSource.fontSharedMaterial;
        }
        tmp.fontSize = size;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        tmp.color = Color.white;
        tmp.text = text;
        if (!string.IsNullOrEmpty(translateKey))
        {
            var translate = go.AddComponent<TranslateText>();
            translate.text = tmp;
            translate.key = translateKey;
        }
        return tmp;
    }

    static string GetPath(Transform t) => t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;

    static string Log(string msg)
    {
        Debug.Log("[InGameMissionUIBuilder] " + msg);
        return msg;
    }
}
