using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-22) 방어·공격 상성표. WaveEnemyInfoPanel의 인포 버튼(i) → 열기
// 행 = 방어 타입(경갑·중갑·중장갑·영체), 열 = 공격 타입(물리·마법·진동·카오스), 칸 = 피해 배율(%)
// 값은 열 때마다 Resources/DefenceInfo 표에서 읽음 → 배율을 고치면 그대로 반영. 약점(>100%) 초록, 내성(<100%) 빨강
// 지금 보고 있던 wave의 방어 타입 행은 강조. 바깥(어두운 배경)·X를 누르면 닫힘
public class DefenceTableCanvas : CanvasUI<DefenceTableCanvas>
{
    public RectTransform gridRoot;   // 칸이 놓일 곳 (GridLayoutGroup, 열 수는 코드에서)
    public TMP_Text textStyle;       // 글씨 모양(폰트·머티리얼) 기준
    public Button closeButton;
    public Button dimButton;         // 바깥 누르면 닫힘
    public Vector2 cellSize = new Vector2(170, 96);
    public Color headerColor = new Color(0.2f, 0.2f, 0.26f, 1f);
    public Color cellColor = new Color(0.13f, 0.13f, 0.17f, 1f);
    public Color highlightColor = new Color(1f, 0.82f, 0.3f, 0.35f);
    public Color weakColor = new Color(0.45f, 1f, 0.45f);
    public Color resistColor = new Color(1f, 0.45f, 0.4f);

    // 표시 순서 (방어: 가벼운 갑옷 → 무거운 갑옷 → 영체, 공격: 물리·마법·진동·카오스)
    static readonly DefenceType[] DEFENCE_ORDER = { DefenceType.Light, DefenceType.Normal, DefenceType.Armored, DefenceType.Ethereal };
    static readonly AttackType[] ATTACK_ORDER = { AttackType.Physic, AttackType.Magic, AttackType.Vibration, AttackType.Chaos };

    DefenceType? highlight;
    readonly List<GameObject> cells = new List<GameObject>();

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCanvas);
        if (dimButton != null)
            dimButton.onClick.AddListener(CloseCanvas);
    }

    public void Open(DefenceType? current)
    {
        highlight = current;
        OpenCanvas();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        Build();
    }

    void Build()
    {
        foreach (GameObject c in cells)
            if (c != null) Destroy(c);
        cells.Clear();
        if (gridRoot == null)
            return;
        var grid = gridRoot.GetComponent<GridLayoutGroup>() ?? gridRoot.gameObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = ATTACK_ORDER.Length + 1;
        grid.cellSize = cellSize;
        grid.spacing = new Vector2(6, 6);
        grid.childAlignment = TextAnchor.MiddleCenter;

        // 머리 줄: 빈 칸(방어＼공격) + 공격 타입
        AddCell(TranslateManager.GetText("DefenceTable_Corner"), headerColor, null, 26);
        foreach (AttackType a in ATTACK_ORDER)
            AddCell(AttackInfo.Title(a), headerColor, AttackInfo.GetSprite(a), 30);

        foreach (DefenceType d in DEFENCE_ORDER)
        {
            bool hl = highlight.HasValue && highlight.Value == d;
            AddCell(DefenceInfo.Title(d), hl ? Color.Lerp(headerColor, highlightColor, 0.6f) : headerColor, DefenceInfo.GetSprite(d), 30);
            foreach (AttackType a in ATTACK_ORDER)
            {
                float mul = DefenceInfo.Get(a, d);
                string text = $"{Mathf.RoundToInt(mul * 100f)}%";
                if (mul > 1.001f) text = $"<color=#{ColorUtility.ToHtmlStringRGB(weakColor)}>{text}</color>";
                else if (mul < 0.999f) text = $"<color=#{ColorUtility.ToHtmlStringRGB(resistColor)}>{text}</color>";
                AddCell(text, hl ? Color.Lerp(cellColor, highlightColor, 0.5f) : cellColor, null, 40);
            }
        }
    }

    void AddCell(string text, Color bg, Sprite icon, float fontSize)
    {
        var go = new GameObject("Cell", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(gridRoot, false);
        var img = go.GetComponent<Image>();
        img.color = bg;
        img.raycastTarget = false;
        cells.Add(go);

        if (icon != null)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)iconGo.transform;
            rt.SetParent(go.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(34, 0);
            rt.sizeDelta = new Vector2(48, 48);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
        }

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = (RectTransform)textGo.transform;
        trt.SetParent(go.transform, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(icon != null ? 60 : 6, 4);
        trt.offsetMax = new Vector2(-6, -4);
        var t = textGo.GetComponent<TextMeshProUGUI>();
        if (textStyle != null)
        {
            t.font = textStyle.font;
            t.fontSharedMaterial = textStyle.fontSharedMaterial;
        }
        t.text = text;
        t.fontSize = fontSize;
        t.enableAutoSizing = true;
        t.fontSizeMin = 16;
        t.fontSizeMax = fontSize;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
    }
}
