using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) 진화 발동 스킬 버튼: 미니언 머리 위 상태(MinionGameStatePanel) 오른쪽 살짝 위
// (2026-11-26) 쿨타임이 다 찼을 때만 보임(충전 중·발동 중엔 숨김) → 금색 테두리 + 통통 → 누르면 MinionEvolution.UseActiveSkill
// 누르기 전까지 쿨타임은 찬 채로 멈춰 있음(다음 충전은 발동이 끝난 뒤부터, ActiveSkillEvolution). 대상이 없어 못 쓰면 토스트
// 위치·크기: 씬 템플릿(MinionGameStatePanel)과 이 버튼은 편집 모드에서 켜 두어 Scene 뷰에서 바로 옮길 수 있음 (실행 중엔 GameWorldCanvas가 템플릿을 숨김)
// (2026-11-26) 씬에 있음: InGame 씬 GameWorldCanvas의 MinionGameStatePanel 템플릿 하위 (메뉴 Tools/InGame/Add Evolution Skill Button To State Panel). 실행 중 생성 없음 → 씬에서 직접 수정
public class EvolutionSkillButton : MonoBehaviour
{
    static readonly Color DIM = new Color(0.25f, 0.25f, 0.3f, 0.9f);
    static readonly Color READY_OUTLINE = new Color(1f, 0.82f, 0.2f, 1f);

    Minion target;
    public Image bg;       // 버튼 바탕 (씬 모양 그대로, 2026-11-26 미니언 그림 안 넣음)
    public Image fill;     // 버튼 안쪽 (씬 모양 그대로)
    public Button button;
    public Outline outline; // 준비되면 금색 테두리
    RectTransform rect;
    bool hooked;

    void Awake()
    {
        rect = (RectTransform)transform;
        if (!hooked && button != null)
        {
            hooked = true;
            button.onClick.AddListener(OnClick);
        }
    }

    // 에디터 빌더가 씬(상태 패널 템플릿)에 만들 때 씀
    public static EvolutionSkillButton Create(RectTransform parent, float size)
    {
        var go = new GameObject("EvolutionSkillButton", typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(size, size);
        var btn = go.AddComponent<EvolutionSkillButton>();
        btn.rect = rt;
        btn.bg = go.GetComponent<Image>();
        btn.bg.color = DIM;
        btn.bg.preserveAspect = true;
        btn.outline = go.AddComponent<Outline>();
        btn.outline.effectDistance = new Vector2(size * 0.06f, -size * 0.06f);
        btn.outline.effectColor = READY_OUTLINE;

        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        var frt = (RectTransform)fillGo.transform;
        frt.SetParent(rt, false);
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.offsetMin = frt.offsetMax = Vector2.zero;
        btn.fill = fillGo.GetComponent<Image>();
        btn.fill.type = Image.Type.Filled;
        btn.fill.fillMethod = Image.FillMethod.Radial360;
        btn.fill.fillOrigin = (int)Image.Origin360.Top;
        btn.fill.fillClockwise = true;
        btn.fill.preserveAspect = true;
        btn.fill.raycastTarget = false;

        btn.button = go.GetComponent<Button>();
        btn.button.targetGraphic = btn.bg;
        return btn;
    }

    public void Bind(Minion minion)
    {
        if (rect == null)
            rect = (RectTransform)transform;
        target = minion;
        // (2026-11-26) 미니언 썸네일은 안 보여줌 → 그림·색·글씨는 씬 템플릿 모양 그대로
        Refresh();
    }

    public void Refresh()
    {
        MinionEvolution evo = target != null ? target.EvolutionAbility : null;
        // 쿨타임이 다 찼을 때만 보임
        bool show = evo != null && evo.HasActiveSkill && evo.IsActiveSkillReady && !evo.IsActiveSkillRunning;
        if (gameObject.activeSelf != show)
            gameObject.SetActive(show);
        if (!show)
            return;
        if (fill != null)
            fill.fillAmount = 1f;
        if (outline != null)
            outline.enabled = true;
        button.interactable = true;
        rect.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(Time.unscaledTime * 6f));
    }

    void OnClick()
    {
        MinionEvolution evo = target != null ? target.EvolutionAbility : null;
        if (evo == null || !evo.IsActiveSkillReady)
            return;
        if (!evo.UseActiveSkill())
            ToastCanvas.Toast(TranslateManager.GetText("Evolution_NoTarget"));
    }
}
