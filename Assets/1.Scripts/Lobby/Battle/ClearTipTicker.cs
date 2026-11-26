using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) 로비 전투 화면 하단 띠지: 지금 고른 스테이지·난이도의 클리어 팁 "닉네임 : 팁" 한 줄씩 (ClearTipManager)
// AUTO_INTERVAL초마다 또는 띠지를 누르면 지금 줄이 위로 올라가며 사라지고 다음 줄이 아래에서 올라옴 (끝까지 가면 처음부터)
// 팁이 없으면 안내 한 줄(ClearTip_Empty), 넘김 없음. 스테이지·난이도가 바뀌면(BattleCanvas.UpdateCanvas → Refresh) 그 팁으로
// 모양은 씬에 만들어 둠 (메뉴 Tools/Lobby/Add ClearTip Band To Lobby Scene → BattleCanvas/ClearTipBand + LineA/LineB 연결). 실행 중 생성 없음 → 씬에서 직접 수정
public class ClearTipTicker : MonoBehaviour
{
    public const float AUTO_INTERVAL = 5f;
    const float MOVE_TIME = 0.35f;

    public RectTransform band;
    public TMP_Text lineA;
    public TMP_Text lineB;
    // (2026-11-26) 로비 하단 메뉴 바(LobbyCanvas LobbyStateButtons_Hor, 아래 0~200, 정렬 150)가 덮지 않게 바로 위 (204~260)
    //   씬의 Play·난이도 버튼은 70 올려 둠. 하단 메뉴 높이가 바뀌면 같이 조정
    public const float LOBBY_BOTTOM_BAR_HEIGHT = 200f;
    [Tooltip("기본 모양 위치 (화면 아래에서 위로, 띠지 가운데 y)")] public float bottomY = LOBBY_BOTTOM_BAR_HEIGHT + 4f + 28f;
    [Tooltip("기본 모양 높이")] public float height = 56f;

    readonly List<ClearTip> tips = new List<ClearTip>();
    // 이번 앱 실행에서 고정 팁을 한 번 보여 줬는지 (씬이 바뀌어도 유지, 앱을 다시 켜면 처음부터)
    public static bool PinnedShownThisRun { get; set; }
    int index;
    float timer;
    bool moving;
    string loadedKey;
    int loadVersion;
    TMP_Text current, next;

    public IReadOnlyList<ClearTip> Tips => tips;
    public string CurrentText => current != null ? current.text : null;

    void Awake()
    {
        if (band == null || lineA == null || lineB == null)
        {
            Debug.LogWarning("[ClearTipTicker] 띠지(band·lineA·lineB) 연결 없음 → 메뉴 Tools/Lobby/Add ClearTip Band To Lobby Scene");
            enabled = false;
            return;
        }
        current = lineA;
        next = lineB;
        next.gameObject.SetActive(false);
        Button btn = band.GetComponent<Button>();
        if (btn == null)
            btn = band.gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(Next);
    }

    void OnEnable()
    {
        if (band != null)
            Refresh();
    }

    void Update()
    {
        if (tips.Count <= 1 || moving)
            return;
        timer += Time.unscaledDeltaTime;
        if (timer >= AUTO_INTERVAL)
            Next();
    }

    // 지금 스테이지·난이도 팁 다시 불러오기 (같은 스테이지면 그대로)
    public void Refresh(bool force = false)
    {
        if (current == null)
            return;
        string key = ClearTipManager.CurrentStageKey;
        if (!force && key == loadedKey && tips.Count > 0)
            return;
        loadedKey = key;
        Load(key);
    }

    async void Load(string key)
    {
        int version = ++loadVersion;
        List<ClearTip> loaded = await ClearTipManager.GetTipsAsync(key);
        if (this == null || version != loadVersion)
            return;
        tips.Clear();
        tips.AddRange(loaded);
        if (PinnedShownThisRun && tips.Exists(t => !t.pinned))
            tips.RemoveAll(t => t.pinned); // 이번 실행에서 고정 팁을 이미 봤으면 유저 팁만
        index = 0;
        timer = 0f;
        ShowImmediate();
    }

    void ShowImmediate()
    {
        current.DOKill();
        next.DOKill();
        moving = false;
        current.rectTransform.anchoredPosition = Vector2.zero;
        current.alpha = 1f;
        current.text = tips.Count > 0 ? tips[index].DisplayText : TranslateManager.GetText("ClearTip_Empty");
        current.gameObject.SetActive(true);
        next.gameObject.SetActive(false);
    }

    // 다음 팁: 지금 줄은 위로 올라가며 사라지고 다음 줄이 아래에서 올라옴
    public void Next()
    {
        timer = 0f;
        if (tips.Count <= 1 || moving)
        {
            if (tips.Count == 1 && tips[0].pinned)
                PinnedShownThisRun = true;
            return;
        }
        // (2026-11-26) 고정 팁(pinned)은 앱을 켤 때마다 처음 한 번만: 한 번 보여 준 뒤 넘기면 목록에서 빼고 유저 팁만 반복 (유저 팁이 없으면 고정 팁 유지)
        if (tips[index].pinned)
            PinnedShownThisRun = true;
        if (PinnedShownThisRun && tips.Exists(t => t.pinned) && tips.Exists(t => !t.pinned))
        {
            ClearTip shown = tips[index];
            tips.RemoveAll(t => t.pinned);
            index = shown.pinned ? -1 : tips.IndexOf(shown); // -1 → 다음 줄 = 유저 팁 첫 줄
        }
        index = (index + 1) % tips.Count;
        float h = band.rect.height > 0f ? band.rect.height : height;
        moving = true;
        next.text = tips[index].DisplayText;
        next.gameObject.SetActive(true);
        next.alpha = 0f;
        next.rectTransform.anchoredPosition = new Vector2(0f, -h);
        current.rectTransform.DOAnchorPosY(h, MOVE_TIME).SetUpdate(true);
        current.DOFade(0f, MOVE_TIME).SetUpdate(true);
        next.rectTransform.DOAnchorPosY(0f, MOVE_TIME).SetUpdate(true);
        next.DOFade(1f, MOVE_TIME).SetUpdate(true).OnComplete(() =>
        {
            current.gameObject.SetActive(false);
            (current, next) = (next, current);
            moving = false;
        });
    }

    // 기본 모양: 화면 아래쪽 반투명 띠 + 가운데 한 줄 (넘치는 글씨는 띠 밖으로 안 보이게 RectMask2D)
    // (2026-11-26) 에디터 메뉴 Tools/Lobby/Add ClearTip Band To Lobby Scene 도 이걸로 씬에 만듦
    public void BuildUI()
    {
        RectTransform parent = transform as RectTransform;
        var go = new GameObject("ClearTipBand", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        band = (RectTransform)go.transform;
        band.SetParent(parent, false);
        band.anchorMin = new Vector2(0.06f, 0f);
        band.anchorMax = new Vector2(0.94f, 0f);
        band.pivot = new Vector2(0.5f, 0.5f);
        band.sizeDelta = new Vector2(0f, height);
        band.anchoredPosition = new Vector2(0f, bottomY);
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

        TMP_Text fontSrc = GetComponentInParent<Canvas>() != null ? GetComponentInParent<Canvas>().GetComponentInChildren<TMP_Text>(true) : null;
        lineA = NewLine("LineA", fontSrc);
        lineB = NewLine("LineB", fontSrc);
    }

    TMP_Text NewLine(string name, TMP_Text fontSrc)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rt = (RectTransform)go.transform;
        rt.SetParent(band, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(20f, 0f);
        rt.offsetMax = new Vector2(-20f, 0f);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (fontSrc != null)
        {
            t.font = fontSrc.font;
            t.fontSharedMaterial = fontSrc.fontSharedMaterial;
        }
        t.fontSize = 30f;
        t.enableAutoSizing = true;
        t.fontSizeMin = 18f;
        t.fontSizeMax = 30f;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Ellipsis;
        t.raycastTarget = false;
        return t;
    }
}
