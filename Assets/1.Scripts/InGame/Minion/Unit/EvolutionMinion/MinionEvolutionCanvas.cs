using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-24) 5성 진화 갈래 선택 창. 미니언이 5성(MAX_MERGE_LEVEL)이 되면(Minion.Spawn → Request) 갈래 2장 중 하나를 고름 → Minion.Evolve
// 반드시 골라야 함(닫기 없음), 게임은 멈추지 않음. 여러 마리가 한꺼번에 5성이 되면 차례로 열림. 고르기 전에 미니언이 사라지면 건너뜀
// 씬에 없으면 실행 중 기본 모양을 만듦 (모양을 바꾸려면 씬에 이 컴포넌트 + 연결 칸을 채운 UI를 두면 그걸 씀)
public class MinionEvolutionCanvas : MonoBehaviour
{
    public GameObject panel;              // 켜고 끄는 부분
    public TMP_Text titleText;            // "진화!"
    public TMP_Text subText;              // "{미니언 이름}의 진화 방향을 선택하세요"
    public Image thumImage;               // 미니언 그림
    public MinionEvolutionOption[] options; // 갈래 칸 (2개)

    static MinionEvolutionCanvas instance;
    readonly Queue<Minion> queue = new Queue<Minion>();
    Minion current;

    public static MinionEvolutionCanvas Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<MinionEvolutionCanvas>(FindObjectsInactive.Include);
            if (instance == null)
                instance = new GameObject(nameof(MinionEvolutionCanvas)).AddComponent<MinionEvolutionCanvas>();
            return instance;
        }
    }

    // 5성이 된 미니언 진화 요청. 한 프레임 뒤에 처리 (Spawn 직후엔 아직 타일로 옮기기 전이라 위치·선택이 안 맞음)
    public static async UniTaskVoid Request(Minion minion)
    {
        if (await UniTask.Yield(PlayerLoopTiming.Update, minion.GetCancellationTokenOnDestroy()).SuppressCancellationThrow())
            return;
        if (!IsWaiting(minion))
            return;
        List<MinionEvolutionData> branches = minion.minionData.GetEvolutions();
        if (branches.Count <= 1) // 갈래가 하나면 고르지 않고 바로
        {
            minion.Evolve(branches.Count == 1 ? branches[0] : null);
            return;
        }
        Instance.Enqueue(minion);
    }

    // 아직 진화를 기다리는 미니언인지 (사라졌거나 이미 진화했으면 false)
    static bool IsWaiting(Minion minion) => minion != null && minion.gameObject.activeInHierarchy && minion.CanEvolve;

    void Awake()
    {
        if (instance == null)
            instance = this;
        if (panel == null || options == null || options.Length == 0)
            BuildUI();
        panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void Enqueue(Minion minion)
    {
        if (current == minion || queue.Contains(minion))
            return;
        queue.Enqueue(minion);
        if (current == null)
            ShowNext();
    }

    void Update()
    {
        // 고르는 중에 미니언이 사라졌으면(판매 등) 다음으로
        if (current != null && !IsWaiting(current))
            ShowNext();
    }

    void ShowNext()
    {
        current = null;
        while (queue.Count > 0)
        {
            Minion next = queue.Dequeue();
            if (IsWaiting(next))
            {
                current = next;
                break;
            }
        }
        if (current == null)
        {
            panel.SetActive(false);
            return;
        }

        MinionData data = current.minionData;
        List<MinionEvolutionData> branches = data.GetEvolutions();
        if (titleText != null)
            titleText.text = TranslateManager.GetText("Evolution_Title");
        if (subText != null)
            subText.text = string.Format(TranslateManager.GetText("Evolution_Select"), data.Title);
        if (thumImage != null)
        {
            Sprite sprite = data.evolvedSprite != null ? data.evolvedSprite : data.thum;
            thumImage.sprite = sprite;
            thumImage.enabled = sprite != null;
            thumImage.preserveAspect = true;
        }
        for (int i = 0; i < options.Length; i++)
        {
            bool active = i < branches.Count;
            options[i].gameObject.SetActive(active);
            if (active)
                options[i].Set(this, branches[i]);
        }
        panel.SetActive(true);
        SoundManager.Play(SFXType.PopupOpen);
    }

    public void Select(MinionEvolutionData branch)
    {
        Minion target = current;
        if (IsWaiting(target))
            target.Evolve(branch);
        ShowNext();
    }

    // ---- 실행 중 기본 모양 ----
    static readonly Color DIM_COLOR = new Color(0f, 0f, 0f, 0.65f);
    static readonly Color PANEL_COLOR = new Color(0.16f, 0.13f, 0.24f, 0.97f);
    static readonly Color OPTION_COLOR = new Color(0.32f, 0.25f, 0.5f, 1f);
    static readonly Color TITLE_COLOR = new Color(1f, 0.85f, 0.3f);

    void BuildUI()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 140;
        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0f;
        if (gameObject.GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        // 어두운 배경 (뒤 화면 터치 막음)
        RectTransform dim = NewRect("Dim", (RectTransform)transform);
        Stretch(dim);
        dim.gameObject.AddComponent<Image>().color = DIM_COLOR;
        panel = dim.gameObject;

        RectTransform box = NewRect("Box", dim);
        box.sizeDelta = new Vector2(940, 900);
        box.gameObject.AddComponent<Image>().color = PANEL_COLOR;

        titleText = NewText("Title", box, 72, TITLE_COLOR, new Vector2(0, 360), new Vector2(880, 100));
        subText = NewText("Sub", box, 40, Color.white, new Vector2(0, 275), new Vector2(880, 70));

        RectTransform thum = NewRect("Thum", box);
        thum.anchoredPosition = new Vector2(0, 140);
        thum.sizeDelta = new Vector2(180, 180);
        thumImage = thum.gameObject.AddComponent<Image>();
        thumImage.raycastTarget = false;

        options = new MinionEvolutionOption[2];
        for (int i = 0; i < options.Length; i++)
        {
            RectTransform opt = NewRect($"Option{i}", box);
            opt.anchoredPosition = new Vector2(i == 0 ? -225 : 225, -175);
            opt.sizeDelta = new Vector2(420, 400);
            Image bg = opt.gameObject.AddComponent<Image>();
            bg.color = OPTION_COLOR;
            MinionEvolutionOption option = opt.gameObject.AddComponent<MinionEvolutionOption>();
            option.button = opt.gameObject.AddComponent<Button>();
            option.button.targetGraphic = bg;
            option.nameText = NewText("Name", opt, 46, TITLE_COLOR, new Vector2(0, 130), new Vector2(380, 80));
            option.descText = NewText("Desc", opt, 34, Color.white, new Vector2(0, -40), new Vector2(380, 250));
            options[i] = option;
        }
    }

    static RectTransform NewRect(string name, RectTransform parent)
    {
        RectTransform rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static TMP_Text NewText(string name, RectTransform parent, float size, Color color, Vector2 pos, Vector2 boxSize)
    {
        RectTransform rt = NewRect(name, parent);
        rt.anchoredPosition = pos;
        rt.sizeDelta = boxSize;
        TextMeshProUGUI text = rt.gameObject.AddComponent<TextMeshProUGUI>(); // 폰트 = TMP 기본(Main_Regular SDF)
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }
}
