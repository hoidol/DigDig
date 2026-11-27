using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 결과 화면 게임 기록 UI를 InGame 씬 ResultCanvas에 실제로 만듦 (한 번 실행 후 씬 저장, 이후 모양은 씬에서 직접 수정)
// - ResultCanvas/Panel/Bg를 세로로 늘리고 기존 칸(최종 웨이브·보상·버튼·클리어 팁)을 위아래로 다시 배치
// - ResultStats(2열 그리드 + StatRow_Template) · 피해 순위 제목 · ResultDamage(세로 줄 + DamageRow_Template = 피해 집계 줄 복사 + 이름·스킬 비율 글씨)
// - ResultCanvas.statsPanel 연결. 이미 있으면 지우고 새로
// 피해 집계 창 슬라임 줄: Tools/InGame/Add Slime Row To DamageMeter (패널 하나 추가)
public static class ResultStatsBuilder
{
    const float BG_HEIGHT = 1760f, BG_Y = 60f, TOP = BG_HEIGHT / 2f, OLD_TOP = 500f;
    static readonly Color ROW_COLOR = new Color(0.22f, 0.22f, 0.26f, 1f);
    static readonly Color LABEL_COLOR = new Color(0.78f, 0.80f, 0.86f, 1f);
    static readonly Color DETAIL_COLOR = new Color(1f, 0.82f, 0.35f, 1f);

    [MenuItem("Tools/InGame/Add Result Stats To ResultCanvas")]
    public static void Build()
    {
        ResultCanvas canvas = Object.FindFirstObjectByType<ResultCanvas>(FindObjectsInactive.Include);
        DamageMeterSystemContainer meter = Object.FindFirstObjectByType<DamageMeterSystemContainer>(FindObjectsInactive.Include);
        if (canvas == null || meter == null)
        {
            Debug.LogWarning("[ResultStatsBuilder] InGame 씬(ResultCanvas·DamageMeterSystemContainer)을 열고 실행");
            return;
        }
        RectTransform bg = canvas.transform.Find("Panel/Bg") as RectTransform;
        if (bg == null)
        {
            Debug.LogWarning("[ResultStatsBuilder] ResultCanvas/Panel/Bg 없음");
            return;
        }
        foreach (string n in new[] { "ResultStats", "ResultDamageTitle", "ResultDamage" })
        {
            Transform old = bg.Find(n);
            if (old != null)
                Undo.DestroyObjectImmediate(old.gameObject);
        }

        bool firstBuild = bg.sizeDelta.y < BG_HEIGHT - 1f;
        Undo.RecordObject(bg, "Result stats");
        if (firstBuild)
        {
            // 기존 칸: 위쪽 기준 거리를 유지한 채 위로 (최종 웨이브·BEST)
            foreach (Transform c in bg)
            {
                if (c.name == "NewRecord" || c.name == "LineDeco (1)" || c.name.StartsWith("BasicFrame_Gradient_02"))
                {
                    var rt = (RectTransform)c;
                    Undo.RecordObject(rt, "Result stats");
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, TOP - (OLD_TOP - rt.anchoredPosition.y));
                }
            }
        }
        bg.sizeDelta = new Vector2(bg.sizeDelta.x, BG_HEIGHT);
        bg.anchoredPosition = new Vector2(bg.anchoredPosition.x, BG_Y);

        TMP_Text styleText = canvas.transform.Find("Panel/Bg/LineDeco/StageTitleText (3)")?.GetComponent<TMP_Text>();
        Image styleRow = meter.GetComponentInChildren<DamageMeterSystemPanel>(true)?.GetComponent<Image>();

        // ---- 통계 칸 (2열)
        RectTransform stats = NewRect("ResultStats", bg, new Vector2(0f, 321f), new Vector2(900f, 248f));
        var grid = stats.gameObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.cellSize = new Vector2(442f, 56f);
        grid.spacing = new Vector2(16f, 8f);
        grid.childAlignment = TextAnchor.UpperCenter;
        RectTransform statRow = NewRect("StatRow_Template", stats, Vector2.zero, new Vector2(442f, 56f));
        var rowImg = statRow.gameObject.AddComponent<Image>();
        CopyImage(styleRow, rowImg);
        TMP_Text label = NewText("LabelText", statRow, styleText, 28f, TextAlignmentOptions.MidlineLeft, LABEL_COLOR);
        Stretch(label.rectTransform, 18f, 200f);
        TMP_Text value = NewText("ValueText", statRow, styleText, 30f, TextAlignmentOptions.MidlineRight, Color.white);
        Stretch(value.rectTransform, 200f, 18f);
        var statComp = statRow.gameObject.AddComponent<ResultStatRow>();
        statComp.labelText = label;
        statComp.valueText = value;
        label.text = "플레이 시간";
        value.text = "0:00";
        statRow.gameObject.SetActive(false);

        // ---- 피해 순위 제목 (보상 제목 LineDeco 복사)
        Transform rewardTitle = bg.Find("LineDeco");
        if (rewardTitle != null)
        {
            GameObject dmgTitle = Object.Instantiate(rewardTitle.gameObject, bg);
            Undo.RegisterCreatedObjectUndo(dmgTitle, "Result stats");
            dmgTitle.name = "ResultDamageTitle";
            ((RectTransform)dmgTitle.transform).anchoredPosition = new Vector2(0f, 90f);
            TMP_Text t = dmgTitle.GetComponentInChildren<TMP_Text>(true);
            if (t != null)
            {
                t.text = TranslateManager.GetText("Result_DamageTitle");
                var tt = t.gameObject.AddComponent<TranslateText>();
                tt.key = "Result_DamageTitle";
            }
            if (firstBuild)
            {
                Undo.RecordObject(rewardTitle, "Result stats");
                ((RectTransform)rewardTitle).anchoredPosition = new Vector2(0f, -430f);
            }
        }

        // ---- 피해 순위 줄 (피해 집계 줄 복사)
        RectTransform damage = NewRect("ResultDamage", bg, new Vector2(0f, -131f), new Vector2(900f, 402f));
        var vlg = damage.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        DamageMeterSystemPanel srcRow = meter.GetComponentInChildren<DamageMeterSystemPanel>(true);
        GameObject dmgRowGo = Object.Instantiate(srcRow.gameObject, damage);
        dmgRowGo.name = "DamageRow_Template";
        dmgRowGo.SetActive(true);
        var le = dmgRowGo.GetComponent<LayoutElement>();
        if (le == null)
            le = dmgRowGo.AddComponent<LayoutElement>();
        le.preferredHeight = 62f;
        le.minHeight = 62f;
        DamageMeterSystemPanel dmgRow = dmgRowGo.GetComponent<DamageMeterSystemPanel>();
        if (dmgRow.thumImage != null)
            dmgRow.thumImage.rectTransform.sizeDelta = new Vector2(54f, 54f);
        TMP_Text name = NewText("NameText", dmgRowGo.transform, dmgRow.damageText != null ? dmgRow.damageText : styleText, 28f, TextAlignmentOptions.MidlineLeft, Color.white);
        LeftBox(name.rectTransform, 84f, 260f);
        name.text = "이름";
        TMP_Text detail = NewText("DetailText", dmgRowGo.transform, name, 22f, TextAlignmentOptions.MidlineLeft, DETAIL_COLOR);
        LeftBox(detail.rectTransform, 350f, 180f);
        detail.text = "스킬 0%";
        dmgRow.nameText = name;
        dmgRow.detailText = detail;
        dmgRowGo.SetActive(false);

        // ---- 보상 (경험치만) · 클리어 팁 · 버튼을 아래로
        if (firstBuild)
        {
            RectTransform rewardBg = bg.Cast<Transform>().FirstOrDefault(t => t.name == "Bg" && t.GetComponentInChildren<RewardPanel>(true) != null) as RectTransform;
            if (rewardBg != null)
            {
                Undo.RecordObject(rewardBg, "Result stats");
                rewardBg.anchoredPosition = new Vector2(0f, -580f);
                rewardBg.sizeDelta = new Vector2(rewardBg.sizeDelta.x, 170f);
                RectTransform row = rewardBg.GetComponentInChildren<HorizontalLayoutGroup>(true)?.transform as RectTransform;
                if (row != null)
                {
                    Undo.RecordObject(row, "Result stats");
                    row.anchorMin = row.anchorMax = new Vector2(0.5f, 0.5f);
                    row.anchoredPosition = Vector2.zero;
                    row.sizeDelta = new Vector2(row.sizeDelta.x, 160f);
                }
            }
            RectTransform hor = bg.Find("Hor") as RectTransform;
            if (hor != null)
            {
                Undo.RecordObject(hor, "Result stats");
                hor.anchoredPosition = new Vector2(hor.anchoredPosition.x, -1000f);
            }
            RectTransform tip = (canvas.transform.Find("ClearTipInput") ?? canvas.transform.Find("Panel/ClearTipInput")) as RectTransform;
            if (tip != null)
            {
                Undo.RecordObject(tip, "Result stats");
                tip.anchoredPosition = new Vector2(tip.anchoredPosition.x, BG_Y - 745f);
            }
        }

        ResultStatsPanel panel = canvas.GetComponent<ResultStatsPanel>();
        if (panel == null)
            panel = Undo.AddComponent<ResultStatsPanel>(canvas.gameObject);
        Undo.RecordObject(panel, "Result stats");
        panel.statRowTemplate = statComp;
        panel.statContainer = stats;
        panel.damageRowTemplate = dmgRow;
        panel.damageContainer = damage;
        Undo.RecordObject(canvas, "Result stats");
        canvas.statsPanel = panel;
        EditorUtility.SetDirty(canvas);
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("[ResultStatsBuilder] 결과 화면 게임 기록 UI 생성 (씬 저장은 직접)");
    }

    // 피해 집계 창(장착 5종)에 슬라임 줄 패널 하나 추가 (줄 수 = 장착 수 + 1)
    [MenuItem("Tools/InGame/Add Slime Row To DamageMeter")]
    public static void AddSlimeRow()
    {
        DamageMeterSystemContainer meter = Object.FindFirstObjectByType<DamageMeterSystemContainer>(FindObjectsInactive.Include);
        if (meter == null)
        {
            Debug.LogWarning("[ResultStatsBuilder] DamageMeterSystemContainer 없음 (InGame 씬을 열고 실행)");
            return;
        }
        var panels = meter.GetComponentsInChildren<DamageMeterSystemPanel>(true);
        int need = GameSetting.MINION_SLOT_COUNT + 1;
        if (panels.Length >= need)
        {
            Debug.Log("[ResultStatsBuilder] 피해 집계 패널이 이미 충분함");
            return;
        }
        var rt = (RectTransform)meter.transform;
        var vlg = meter.GetComponent<VerticalLayoutGroup>();
        float rowH = ((RectTransform)panels[0].transform).sizeDelta.y + (vlg != null ? vlg.spacing : 0f);
        Undo.RecordObject(rt, "Slime row");
        for (int i = panels.Length; i < need; i++)
        {
            GameObject go = Object.Instantiate(panels[panels.Length - 1].gameObject, meter.transform);
            Undo.RegisterCreatedObjectUndo(go, "Slime row");
            go.name = $"DamageMeterSystemPanel{i + 1}";
            float oldH = rt.sizeDelta.y;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, oldH + rowH);
            rt.anchoredPosition -= new Vector2(0f, rowH * (1f - rt.pivot.y)); // 위쪽 끝은 그대로
        }
        Undo.RecordObject(meter, "Slime row");
        meter.damageMeterSystemPanels = meter.GetComponentsInChildren<DamageMeterSystemPanel>(true);
        EditorUtility.SetDirty(meter);
        EditorSceneManager.MarkSceneDirty(meter.gameObject.scene);
        Debug.Log("[ResultStatsBuilder] 피해 집계 슬라임 줄 패널 추가 (씬 저장은 직접)");
    }

    static RectTransform NewRect(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Result stats");
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static TMP_Text NewText(string name, Transform parent, TMP_Text style, float size, TextAlignmentOptions align, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (style != null)
        {
            t.font = style.font;
            t.fontSharedMaterial = style.fontSharedMaterial;
        }
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Ellipsis;
        return t;
    }

    // 좌우 여백만 두고 위아래로 꽉
    static void Stretch(RectTransform rt, float left, float right)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, 0f);
        rt.offsetMax = new Vector2(-right, 0f);
    }

    static void LeftBox(RectTransform rt, float x, float width)
    {
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(x, 0f);
        rt.sizeDelta = new Vector2(width, 0f);
    }

    static void CopyImage(Image src, Image dst)
    {
        if (src != null)
        {
            dst.sprite = src.sprite;
            dst.type = src.type;
            dst.pixelsPerUnitMultiplier = src.pixelsPerUnitMultiplier;
        }
        dst.color = ROW_COLOR;
        dst.raycastTarget = false;
    }
}
