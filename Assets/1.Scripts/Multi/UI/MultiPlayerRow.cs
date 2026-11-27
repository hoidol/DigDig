using TMPro;
using UnityEngine;

// (2026-11-27) Ready 창 유저 한 줄: 닉네임 + Ready 표시 (MultiReadyPanel이 템플릿을 8개로 복사)
public class MultiPlayerRow : MonoBehaviour
{
    public TMP_Text nicknameText;
    public GameObject readyMark;   // Ready를 누르면 켜짐
    public GameObject meMark;      // 나일 때 켜짐 (선택)
    public GameObject hostMark;    // 방장 (선택, 2026-11-27)

    public void Set(MultiPlayerState player, bool isMe, bool isHost = false)
    {
        if (hostMark != null)
            hostMark.SetActive(isHost);
        if (nicknameText != null)
            nicknameText.text = MultiGame.DisplayName(player);
        if (readyMark != null)
            readyMark.SetActive(player != null && player.ready);
        if (meMark != null)
            meMark.SetActive(isMe);
    }
}
