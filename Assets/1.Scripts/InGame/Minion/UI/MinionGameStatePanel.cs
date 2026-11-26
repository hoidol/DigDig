using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 미니언 머리 위 상태 표시 하나. GameWorldCanvas 하위에 템플릿으로 하나 두면 미니언마다 복사해서 씀 (풀링, 미니언마다 Canvas를 두지 않음)
// 스킬(쿨타임)이 있는 미니언(MinionData.hasSkill) = mergeLvForCooltime(머지 레벨 + 쿨타임 바), 없으면 mergeLv(머지 레벨만)
public class MinionGameStatePanel : MonoBehaviour
{
    [Header("쿨타임 없는 미니언")]
    public GameObject mergeLv;
    public TMP_Text mergeLvText;

    [Header("쿨타임 있는 미니언")]
    public GameObject mergeLvForCooltime;
    public TMP_Text mergeLvForCooltimeText;
    public Slider cooltimeSlider; // 다음 스킬까지 차오르는 바 (0~1, 조작 불가로 씀)

    [Header("추가 상태 글씨 (버서커 중첩 등, Minion.StateText)")]
    public TMP_Text stateText; // 미니언이 StateText를 주면 보이고, null이면 숨김

    public Vector3 offset; // Minion.StatePointPosition 기준 추가 위치 (월드 단위)

    // (2026-11-26) 진화 발동 스킬 버튼 (진화한 미니언에 발동 스킬이 있을 때만 보임). 템플릿에 만들어 둠: Tools/InGame/Add Evolution Skill Button To State Panel
    public EvolutionSkillButton skillButton;

    public Minion Target { get; private set; }
    RectTransform rect;
    Canvas canvas;
    int shownMergeLevel = -1;
    string shownStateText;

    void Awake()
    {
        rect = transform as RectTransform;
        canvas = GetComponentInParent<Canvas>();
        if (cooltimeSlider != null)
        {
            cooltimeSlider.minValue = 0f;
            cooltimeSlider.maxValue = 1f;
            cooltimeSlider.interactable = false;
        }
    }

    public void Bind(Minion minion)
    {
        Target = minion;
        shownMergeLevel = -1;
        shownStateText = null;
        if (stateText != null)
            stateText.gameObject.SetActive(false);
        gameObject.SetActive(true);
        bool hasSkill = minion.minionData != null && minion.minionData.hasSkill;
        if (mergeLv != null)
            mergeLv.SetActive(!hasSkill);
        if (mergeLvForCooltime != null)
            mergeLvForCooltime.SetActive(hasSkill);
        if (skillButton != null)
            skillButton.Bind(minion);
        Refresh();
        FollowTarget();
    }

#if UNITY_EDITOR
    // 에디터 빌더용: 머리 위 상태 오른쪽 살짝 위. 크기 = 머지 레벨 표시 높이 기준
    public EvolutionSkillButton CreateSkillButtonForEditor()
    {
        RectTransform self = (RectTransform)transform;
        RectTransform refRect = (mergeLvForCooltime != null ? mergeLvForCooltime.transform : mergeLv != null ? mergeLv.transform : transform) as RectTransform;
        float size = refRect != null && refRect.rect.height > 0f ? refRect.rect.height * 1.3f : 40f;
        float width = self.rect.width;
        EvolutionSkillButton btn = EvolutionSkillButton.Create(self, size);
        ((RectTransform)btn.transform).anchoredPosition = new Vector2(width * 0.5f + size * 0.6f, size * 0.35f);
        btn.gameObject.SetActive(false);
        return btn;
    }
#endif

    public void Unbind()
    {
        Target = null;
        gameObject.SetActive(false);
    }

    // 머지 레벨은 바뀔 때만 글씨 갱신, 쿨타임은 매 프레임
    void Refresh()
    {
        if (Target.mergeLevel != shownMergeLevel)
        {
            shownMergeLevel = Target.mergeLevel;
            string text = GetMergeLevelText(shownMergeLevel);
            if (mergeLvText != null)
                mergeLvText.text = text;
            if (mergeLvForCooltimeText != null)
                mergeLvForCooltimeText.text = text;
        }
        if (cooltimeSlider != null && mergeLvForCooltime != null && mergeLvForCooltime.activeSelf)
            cooltimeSlider.SetValueWithoutNotify(Target.SkillProgress);

        // 진화 발동 스킬 버튼 (씬 템플릿에 있는 것만 씀)
        if (skillButton != null)
            skillButton.Refresh();

        // 추가 상태 글씨: 바뀔 때만 갱신
        if (stateText != null)
        {
            string state = Target.StateText;
            if (state != shownStateText)
            {
                shownStateText = state;
                stateText.gameObject.SetActive(state != null);
                if (state != null)
                    stateText.text = state;
            }
        }
    }

    // 머지 레벨 0 = 1단계 (별 1과 같은 기준)
    public static string GetMergeLevelText(int mergeLevel) => (mergeLevel + 1).ToString();

    // 미니언이 움직인(드래그 포함) 뒤 따라가도록 LateUpdate
    void LateUpdate()
    {
        if (Target == null) // Release 없이 미니언이 파괴됨 (Unity null)
        {
            if (!ReferenceEquals(Target, null))
                GameWorldCanvas.Instance.ReleaseMinionState(Target);
            return;
        }
        if (!Target.gameObject.activeInHierarchy)
        {
            GameWorldCanvas.Instance.ReleaseMinionState(Target);
            return;
        }
        Refresh();
        FollowTarget();
    }

    void FollowTarget()
    {
        Vector3 worldPos = Target.StatePointPosition + offset;
        if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
        {
            transform.position = worldPos;
            return;
        }
        // Screen Space 캔버스면 화면 좌표로 변환
        Camera cam = Camera.main;
        if (cam == null)
            return;
        Vector2 screen = cam.WorldToScreenPoint(worldPos);
        RectTransform parent = rect.parent as RectTransform;
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (parent != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCam, out Vector2 local))
            rect.anchoredPosition = local;
    }
}
