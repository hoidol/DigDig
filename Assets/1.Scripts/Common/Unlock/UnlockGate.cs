using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 조건(유저 레벨·wave 달성)을 채워야 버튼이 동작하게 하는 컴포넌트. 잠글 버튼 오브젝트에 붙인다.
// 잠겨 있을 때 보이는 모습은 인스펙터 조합으로 정한다:
//   - 그대로 보이고 눌러도 동작 안 함(+ 조건 안내 토스트) : 기본
//   - 아예 안 보임                                       : hideWhenLocked
//   - 다른 오브젝트로 대체(자물쇠, "Lv.5" 글씨 등)        : lockedObjs / unlockedObjs
// 그 밖의 연출은 onChanged(bool unlocked)에 연결하거나 상속해서 Apply를 재정의
public class UnlockGate : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("모두 만족해야 해금")]
    public UnlockCondition[] conditions;

    [Header("잠겨 있을 때")]
    [Tooltip("비우면 이 오브젝트의 Button")]
    public Button button;
    [Tooltip("이 오브젝트를 숨김")]
    public bool hideWhenLocked;
    [Tooltip("잠겨 있을 때만 보이는 오브젝트 (대체 UI)")]
    public GameObject[] lockedObjs;
    [Tooltip("해금됐을 때만 보이는 오브젝트")]
    public GameObject[] unlockedObjs;
    [Tooltip("잠긴 버튼을 누르면 조건 안내 토스트")]
    public bool toastWhenLocked = true;
    [Tooltip("조건 안내 문구를 넣을 글씨 (lockedObjs 안의 글씨 등). 없어도 됨")]
    public TMPro.TMP_Text lockedMessageText;

    public UnityEvent<bool> onChanged; // 해금 여부가 바뀔 때 (처음 1번 포함)

    public bool IsUnlocked { get; private set; }

    bool isApplied;      // 처음 한 번은 값이 같아도 적용
    bool isButtonLocked; // 이 컴포넌트가 버튼을 꺼 둔 상태 (다른 코드가 끈 interactable은 건드리지 않음)

    // hideWhenLocked로 이 오브젝트가 꺼져 있어도 조건이 바뀌면 다시 켜져야 해서 OnEnable이 아니라 Awake/OnDestroy에서 구독
    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        GameEventBus.Subscribe<ChangedUserExpEvent>(OnChangedUserExp);
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStart);
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<ChangedUserExpEvent>(OnChangedUserExp);
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);
    }

    void Start()
    {
        Refresh();
    }

    void OnEnable()
    {
        // 꺼져 있던 창이 다시 열릴 때 최신 상태로 (첫 적용은 다른 매니저 준비가 끝난 Start에서)
        if (isApplied)
            Refresh();
    }

    void OnChangedUserExp(ChangedUserExpEvent e) => Refresh();
    void OnWaveStart(WaveStartEvent e) => Refresh();

    // 조건을 다시 확인해 화면에 반영. 이벤트가 없는 조건을 추가했다면 값이 바뀌는 곳에서 직접 호출
    public void Refresh()
    {
        bool unlocked = CheckUnlocked();
        if (isApplied && unlocked == IsUnlocked)
            return;

        isApplied = true;
        IsUnlocked = unlocked;
        Apply(unlocked);
        onChanged?.Invoke(unlocked);
    }

    public bool CheckUnlocked()
    {
        return GetLockedCondition() == null;
    }

    // 아직 못 채운 첫 조건 (모두 채웠으면 null)
    public UnlockCondition GetLockedCondition()
    {
        if (conditions == null)
            return null;

        foreach (UnlockCondition condition in conditions)
        {
            if (!condition.IsMet())
                return condition;
        }
        return null;
    }

    // 잠김/해금 모습 적용. 다른 표현이 필요하면 상속해서 재정의
    protected virtual void Apply(bool unlocked)
    {
        SetButtonLocked(!unlocked);

        SetActive(lockedObjs, !unlocked);
        SetActive(unlockedObjs, unlocked);

        if (!unlocked && lockedMessageText != null)
            lockedMessageText.text = GetLockedCondition().GetLockedMessage();

        if (hideWhenLocked)
            gameObject.SetActive(unlocked);
    }

    void SetButtonLocked(bool locked)
    {
        if (button == null)
            return;

        if (locked)
        {
            if (button.interactable)
            {
                button.interactable = false;
                isButtonLocked = true;
            }
        }
        else if (isButtonLocked)
        {
            button.interactable = true;
            isButtonLocked = false;
        }
    }

    static void SetActive(GameObject[] objs, bool active)
    {
        if (objs == null)
            return;

        foreach (GameObject obj in objs)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    // 잠긴 버튼을 눌렀을 때: 버튼은 꺼져 있어 동작하지 않고 안내만 띄운다
    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsUnlocked || !toastWhenLocked)
            return;

        UnlockCondition condition = GetLockedCondition();
        if (condition != null && ToastCanvas.Instance != null)
            ToastCanvas.Toast(condition.GetLockedMessage());
    }
}
