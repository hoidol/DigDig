using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-21) wave 카드를 드래그하지 않고 누르면 카드 줄 위에 그 미니언 이름·공격 타입·설명 (MinionOfferCard.OnPointerClick)
// 같은 카드를 다시 누르거나, 카드·유닛 드래그를 시작하거나, wave가 시작되면 닫힘
// 씬: MinionOfferPanel/MinionOfferCardInfo (모양은 씬에서 수정). 없으면 실행 중 기본 모양 생성(Build)
public class MinionOfferCardInfo : MonoBehaviour
{
    public RectTransform panel;  // 말풍선 (아래 가운데 기준)
    public TMP_Text nameText;
    public TMP_Text descText;
    public Image attackTypeIcon; // (선택) 공격 타입 아이콘 (Resources/Icons/AttackType/{타입}), 비우면 글씨만
    public float gap = 16f;      // 카드 위 간격

    int shownHandIdx = -1;
    public bool IsShown => panel != null && panel.gameObject.activeSelf;

    void Awake() => Hide();

    // 같은 카드를 다시 누르면 닫힘
    public void Toggle(int handIdx, MinionData data, RectTransform card)
    {
        if (IsShown && shownHandIdx == handIdx)
        {
            Hide();
            return;
        }
        Show(handIdx, data, card);
    }

    public void Show(int handIdx, MinionData data, RectTransform card)
    {
        if (panel == null || data == null)
            return;
        shownHandIdx = handIdx;
        // (2026-11-22) 공격 타입(물리·마법·진동·혼돈)을 이름 옆에 이름과 같은 크기·타입 색으로 표시 (이전: 작은 노란 글씨) → 이번 wave 방어 타입 상성 판단용
        string attackType = $"<color={GetAttackTypeColor(data.attackType)}>{AttackInfo.Title(data.attackType)}</color>";
        nameText.text = $"{data.Title}  {attackType}  <size=75%><color=#9FD8FF>{GetFormName(data.tags)}</color></size>";
        descText.text = data.GetDescription(0);
        if (attackTypeIcon != null)
        {
            attackTypeIcon.sprite = AttackInfo.GetSprite(data.attackType);
            attackTypeIcon.enabled = attackTypeIcon.sprite != null;
        }
        panel.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

        // 카드 위 가운데. 부모 밖으로 나가지 않게 가로만 맞춤
        RectTransform parent = (RectTransform)panel.parent;
        Vector3[] corners = new Vector3[4];
        card.GetWorldCorners(corners);
        Vector3 topCenter = (corners[1] + corners[2]) * 0.5f;
        Vector2 local = parent.InverseTransformPoint(topCenter);
        float half = panel.rect.width * 0.5f;
        Rect r = parent.rect;
        float x = Mathf.Clamp(local.x, r.xMin + half, r.xMax - half);
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = new Vector2(x - r.center.x, local.y - r.center.y + gap);
    }

    // 공격 타입 글씨 색 (회색 금지: 밝은 타일 위에서 안 보임)
    public static string GetAttackTypeColor(AttackType type)
    {
        switch (type)
        {
            case AttackType.Physic: return "#FF9A5C";    // 주황
            case AttackType.Magic: return "#8FB8FF";     // 파랑
            case AttackType.Vibration: return "#7CF0A0"; // 초록
            case AttackType.Chaos: return "#E08CFF";     // 보라
        }
        return "#FFFFFF";
    }

    // 공격 형태 (MinionTag 형태 = 발사·장판·회전·버프·특수), 번역 MinionForm.{태그}
    public static string GetFormName(MinionTag tags)
    {
        foreach (MinionTag form in new[] { MinionTag.Shooter, MinionTag.Zone, MinionTag.Orbit, MinionTag.Support, MinionTag.Special })
            if ((tags & form) == form)
                return TranslateManager.GetText($"MinionForm.{form}");
        return "";
    }

    public void Hide()
    {
        shownHandIdx = -1;
        if (panel != null)
            panel.gameObject.SetActive(false);
    }

    #region 기본 모양 (씬에 없을 때)
    public static MinionOfferCardInfo Build(Transform parent, TMP_FontAsset font)
    {
        var holder = new GameObject("MinionOfferCardInfo", typeof(RectTransform), typeof(MinionOfferCardInfo));
        var holderRt = (RectTransform)holder.transform;
        holderRt.SetParent(parent, false);
        holderRt.anchorMin = Vector2.zero;
        holderRt.anchorMax = Vector2.one;
        holderRt.offsetMin = holderRt.offsetMax = Vector2.zero;
        var info = holder.GetComponent<MinionOfferCardInfo>();

        var bg = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var rt = (RectTransform)bg.transform;
        rt.SetParent(holderRt, false);
        rt.sizeDelta = new Vector2(640, 0);
        bg.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.14f, 0.94f);
        bg.GetComponent<Image>().raycastTarget = false;
        var layout = bg.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 20, 22);
        layout.spacing = 8;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        var fitter = bg.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        info.panel = rt;
        info.nameText = NewText("NameText", rt, font, 40, FontStyles.Bold);
        info.descText = NewText("DescText", rt, font, 30, FontStyles.Normal);
        info.Hide();
        return info;
    }

    static TMP_Text NewText(string name, Transform parent, TMP_FontAsset font, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (font != null)
            t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.raycastTarget = false;
        return t;
    }
    #endregion
}
