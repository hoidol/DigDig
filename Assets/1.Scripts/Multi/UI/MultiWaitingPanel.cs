using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 온라인 게임(8인 모드) 대기 창 — Lobby 씬 (메뉴 Tools/Lobby/Add Online Waiting Canvas), 화면 전체를 막음
// 가운데 크게 "1/8" · 남은 시간 300 → 0초 · 상태 안내("다른 유저를 기다리는 중…" / "AI 봇으로 채우는 중…" / "5초 후 시작합니다!")
// · 작게 "부족한 인원은 AI봇으로 대체됩니다." · 방 코드 · [친구 초대](방 코드 + 게임 링크를 SNS로, 받은 친구는 [코드입력]으로 같은 방) · 내 전적 · 취소 (방이 차면 취소 숨김)
// MultiMatchManager가 대기 중에 켜고 글씨를 갱신. 대기 중엔 싱글 게임·다른 로비 화면을 못 씀 (대기 중 로컬 게임·이어하기 없음)
public class MultiWaitingPanel : MonoBehaviour
{
    public GameObject root;        // 창 전체 (비우면 이 오브젝트)
    public TMP_Text countText;     // "1/8" (가운데 크게)
    public TMP_Text timerText;     // 남은 초 "300"
    public TMP_Text noticeText;    // 상태 안내
    public TMP_Text aiNoticeText;  // 작게 "부족한 인원은 AI봇으로 대체됩니다."
    public TMP_Text codeText;      // "코드 1234567"
    public TMP_Text recordText;    // "우승 N · 클리어 N"
    public GameObject hostMark;    // 내가 방장 (선택)
    public Button cancelButton;
    public Button inviteButton;    // 친구 초대 = 방 코드 + 게임 링크 공유 (2026-11-27 코드 공유 버튼과 합침)

    GameObject Root => root != null ? root : gameObject;

    void Awake()
    {
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnClickedCancel);
        if (inviteButton != null)
            inviteButton.onClick.AddListener(() =>
            {
                // 코드가 있으면 코드 + 링크, 아직 없으면 링크만
                if (!MultiShare.ShareCode(MultiMatchManager.Instance.RoomCode))
                    MultiShare.ShareInvite();
            });
    }

    public void SetVisible(bool visible)
    {
        if (Root.activeSelf != visible)
            Root.SetActive(visible);
        if (visible && !gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void Set(int count, int max, string code = null, bool isHost = false)
    {
        if (countText != null)
            countText.text = $"{count}/{max}";
        if (codeText != null)
            codeText.text = string.IsNullOrEmpty(code) ? "" : string.Format(TranslateManager.GetText("Multi_CodeLabel"), code);
        if (hostMark != null)
            hostMark.SetActive(isHost);
        if (aiNoticeText != null)
            aiNoticeText.text = TranslateManager.GetText("Multi_AiNotice");
        if (recordText != null && UserDataManager.Instance != null)
        {
            UserData user = UserDataManager.Instance.userData;
            recordText.text = string.Format(TranslateManager.GetText("Multi_Record"), user.multiChampion, user.multiClear);
        }
    }

    // 대기 중: 남은 초 (300 → 0)
    public void SetWaiting(float remainSec, bool filling)
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = string.Format(TranslateManager.GetText("Multi_RemainSec"), Mathf.CeilToInt(Mathf.Max(0f, remainSec)));
        }
        if (noticeText != null)
            noticeText.text = TranslateManager.GetText(filling || remainSec <= 0f ? "Multi_AiFilling" : "Multi_WaitingOthers");
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(true);
    }

    // 방이 참: "N초 후 시작합니다!" (취소 안 됨)
    public void SetStarting(int seconds)
    {
        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (noticeText != null)
            noticeText.text = string.Format(TranslateManager.GetText("Multi_StartIn"), Mathf.Max(1, seconds));
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(false);
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
