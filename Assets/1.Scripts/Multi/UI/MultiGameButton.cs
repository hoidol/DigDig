using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 로비 멀티 버튼 (BattleCanvas, 메뉴 Tools/Lobby/Add Multi Game Buttons)
// mode Quick = [멀티 게임](빠른 매칭) · Create = 작은 글씨 [방만들기](새 방, 코드로만 입장) · Code = 작은 글씨 [코드입력](코드 창)
// 누르면 안내 "온라인 게임 중에는 배속 상품·광고 제거 상품을 사용할 수 없습니다" → 참여 → 대기 (오른쪽 위 N/8, 기다리는 동안 로컬 게임 가능)
// 대기 중이면 [멀티 게임] 글씨 = "매칭 취소" (누르면 취소), 방만들기·코드입력은 잠김
[RequireComponent(typeof(Button))]
public class MultiGameButton : MonoBehaviour
{
    public MultiMatchManager.JoinMode mode = MultiMatchManager.JoinMode.Quick;
    public TMP_Text labelText;
    public MultiCodeInputPanel codeInputPanel; // mode Code

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    void OnEnable() => Refresh();

    void Update() => Refresh();

    void Refresh()
    {
        bool waiting = MultiMatchManager.Exists && MultiMatchManager.Instance.IsWaiting;
        if (mode != MultiMatchManager.JoinMode.Quick)
        {
            if (button != null)
                button.interactable = !waiting;
            return;
        }
        if (labelText == null)
            return;
        string text = TranslateManager.GetText(waiting ? "Multi_CancelButton" : "Multi_StartButton");
        if (labelText.text != text)
            labelText.text = text;
    }

    // 안내 팝업 → Yes면 실행
    public static void ConfirmThen(System.Action action)
    {
        YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("Multi_NoticeTitle"), TranslateManager.GetText("Multi_NoticeBody"), yes =>
        {
            if (yes)
                action?.Invoke();
        });
    }

    void OnClicked()
    {
        bool waiting = MultiMatchManager.Exists && MultiMatchManager.Instance.IsWaiting;
        switch (mode)
        {
            case MultiMatchManager.JoinMode.Quick:
                if (waiting)
                    MultiMatchManager.Instance.Cancel().Forget();
                else
                    ConfirmThen(() => MultiMatchManager.Instance.Join().Forget());
                break;
            case MultiMatchManager.JoinMode.Create:
                if (!waiting)
                    ConfirmThen(() => MultiMatchManager.Instance.CreateRoom().Forget());
                break;
            case MultiMatchManager.JoinMode.Code:
                if (waiting)
                    break;
                if (codeInputPanel != null)
                    codeInputPanel.Open();
                else
                    Debug.LogWarning("[MultiGameButton] codeInputPanel 없음 — 메뉴 Tools/Lobby/Add Multi Game Buttons");
                break;
        }
    }
}
