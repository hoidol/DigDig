using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 멀티 매칭 대기 표시 (화면 오른쪽 위). 프리팹 Resources/UI/MultiMatchStatus (메뉴 Tools/Multi/Create MultiMatchStatus Prefab)
// "매칭 중 2/8" + 방 코드 "코드 1234567"(방장이면 ★) + 작은 버튼 [친구 초대](게임 링크 공유) · [코드 공유](7자리 코드 공유) + 취소
// (2026-11-27) 남은 시간 "4:59" + 안내 "방이 꽉 안 차면 AI 봇으로 대체됩니다." → 방이 차면 "5초 후 시작합니다"
// MultiMatchManager가 만들어 씬을 넘어 유지 (로비·싱글 판 어디서든 보임). 모양은 프리팹에서 수정
public class MultiMatchStatusPanel : MonoBehaviour
{
    public TMP_Text countText;   // "2/8"
    public TMP_Text titleText;   // "매칭 중" (번역 Multi_Matching)
    public TMP_Text codeText;    // "코드 1234567" (선택)
    public GameObject hostMark;  // 내가 방장 (선택)
    public Button cancelButton;
    public Button inviteButton;    // 친구 초대 (게임 링크)
    public Button shareCodeButton; // 코드 공유
    public TMP_Text timerText;     // AI로 채우기까지 남은 시간 "4:59"
    public TMP_Text noticeText;    // 안내
    public TMP_Text recordText;    // (2026-11-27) 내 전적 "우승 N · 클리어 N"

    void Awake()
    {
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnClickedCancel);
        if (inviteButton != null)
            inviteButton.onClick.AddListener(MultiShare.ShareInvite);
        if (shareCodeButton != null)
            shareCodeButton.onClick.AddListener(() =>
            {
                if (!MultiShare.ShareCode(MultiMatchManager.Instance.RoomCode))
                    ToastCanvas.Toast(TranslateManager.GetText("Multi_CodeNotReady"));
            });
    }

    public void Set(int count, int max, string code = null, bool isHost = false)
    {
        if (countText != null)
            countText.text = $"{count}/{max}";
        if (titleText != null)
            titleText.text = TranslateManager.GetText("Multi_Matching");
        if (codeText != null)
            codeText.text = string.IsNullOrEmpty(code) ? "" : string.Format(TranslateManager.GetText("Multi_CodeLabel"), code);
        if (hostMark != null)
            hostMark.SetActive(isHost);
        if (shareCodeButton != null)
            shareCodeButton.interactable = !string.IsNullOrEmpty(code);
        if (recordText != null && UserDataManager.Instance != null)
        {
            UserData user = UserDataManager.Instance.userData;
            recordText.text = string.Format(TranslateManager.GetText("Multi_Record"), user.multiChampion, user.multiClear);
        }
    }

    public void SetWaiting(float remainSec, bool filling)
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = GameResultStats.FormatTime(Mathf.Ceil(remainSec));
        }
        if (noticeText != null)
            noticeText.text = TranslateManager.GetText(filling || remainSec <= 0f ? "Multi_AiFilling" : "Multi_AiNotice");
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(true);
    }

    public void SetStarting(int seconds)
    {
        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (noticeText != null)
            noticeText.text = string.Format(TranslateManager.GetText("Multi_StartIn"), Mathf.Max(1, seconds));
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(false); // 찬 뒤엔 취소 안 됨
    }

    void OnClickedCancel()
    {
        YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("Multi_CancelTitle"), TranslateManager.GetText("Multi_CancelBody"), yes =>
        {
            if (yes)
                MultiMatchManager.Instance.Cancel().Forget();
        });
    }
}
