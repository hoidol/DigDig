using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyInfoPanel : MonoBehaviour
{
    public TMP_Text expText;
    public Image barImage;
    public TMP_Text nickText;
    public TMP_Text lvText;

    // 경험치 바 차오르는 연출: 바 한 칸(0 → 가득) 기준 시간. 실제 시간은 차오르는 비율만큼 (최소 MIN)
    public const float EXP_FILL_TIME_PER_BAR = 1f;
    public const float EXP_FILL_MIN_TIME = 0.3f;

    // 연출 중엔 경험치 변경 이벤트로 바가 튀지 않게 (끝나면 UpdatePanel)
    public bool IsAnimating { get; private set; }
    Tween expTween;

    void Start()
    {
        if (!IsAnimating)
            UpdatePanel();
    }

    void OnEnable()
    {
        GameEventBus.Subscribe<ChangedEnergeEvent>(OnChangedEnergeEvent);
        GameEventBus.Subscribe<ChangedUserExpEvent>(OnChangedUserExpEvent);
    }
    void OnDisable()
    {
        GameEventBus.Unsubscribe<ChangedEnergeEvent>(OnChangedEnergeEvent);
        GameEventBus.Unsubscribe<ChangedUserExpEvent>(OnChangedUserExpEvent);
    }
    void OnChangedUserExpEvent(ChangedUserExpEvent e)
    {
        if (!IsAnimating)
            UpdatePanel();
    }
    void OnChangedEnergeEvent(ChangedEnergeEvent e)
    {
        if (!IsAnimating)
            UpdatePanel();
    }

    public void UpdatePanel()
    {
        var myInfo = UserDataManager.Instance.userMyInfoManager;
        if (myInfo.IsMaxLv)
        {
            barImage.fillAmount = 1f;
            expText.text = "MAX";
        }
        else
        {
            barImage.fillAmount = (float)myInfo.userMyInfoData.exp / (float)myInfo.GetMaxExp();
            expText.text = $"{myInfo.userMyInfoData.exp}/{myInfo.GetMaxExp()}";
        }
        nickText.text = UserDataManager.Instance.userMyInfoManager.userMyInfoData.nickname;
        lvText.text = $"LV.{UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv}";
    }

    // 연출 시작: 이 레벨·경험치로 표시를 고정 (이후 BeginExpAnimation ~ EndExpAnimation 동안 이벤트 무시)
    public void BeginExpAnimation(int lv, int exp)
    {
        IsAnimating = true;
        expTween?.Kill();
        SetExpDisplay(lv, exp);
    }

    public void EndExpAnimation()
    {
        expTween?.Kill();
        IsAnimating = false;
        UpdatePanel();
    }

    // lv 레벨 바를 fromExp → toExp로 채움 (toExp는 그 레벨 최대 경험치 이하)
    public UniTask AnimateExp(int lv, int fromExp, int toExp)
    {
        var myInfo = UserDataManager.Instance.userMyInfoManager;
        int maxExp = myInfo.GetMaxExp(lv);
        expTween?.Kill();
        SetExpDisplay(lv, fromExp);
        if (toExp <= fromExp || maxExp <= 0)
            return UniTask.CompletedTask;

        float duration = Mathf.Max(EXP_FILL_MIN_TIME, EXP_FILL_TIME_PER_BAR * (toExp - fromExp) / maxExp);
        var done = new UniTaskCompletionSource();
        float value = fromExp;
        expTween = DOTween.To(() => value, v => { value = v; SetExpDisplay(lv, v); }, toExp, duration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnKill(() => done.TrySetResult());
        return done.Task;
    }

    void SetExpDisplay(int lv, float exp)
    {
        var myInfo = UserDataManager.Instance.userMyInfoManager;
        lvText.text = $"LV.{lv}";
        if (UserMyInfoManager.IsMaxLevel(lv))
        {
            barImage.fillAmount = 1f;
            expText.text = "MAX";
            return;
        }
        int maxExp = myInfo.GetMaxExp(lv);
        barImage.fillAmount = maxExp > 0 ? Mathf.Clamp01(exp / maxExp) : 1f;
        expText.text = $"{Mathf.RoundToInt(exp)}/{maxExp}";
    }

    public void OnClickedEditNick()
    {

    }
    // public void OnClickedEditMyInfo()
    // {

    // }
}
