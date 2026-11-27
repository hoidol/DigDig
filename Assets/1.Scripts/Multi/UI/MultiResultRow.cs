using TMPro;
using UnityEngine;

// (2026-11-27) 멀티 결과 한 줄: 순위 · 닉네임 · 상태(도착 시간 / 탈락 wave / 진행 중 wave) · 나 표시 · 우승 표시
public class MultiResultRow : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text nicknameText;
    public TMP_Text statusText;
    public GameObject meMark;
    public GameObject championMark;

    public void Set(int rank, MultiPlayerState p, bool isMe, bool isChampion, long startAtMs)
    {
        if (rankText != null) rankText.text = rank.ToString();
        if (nicknameText != null) nicknameText.text = MultiGame.DisplayName(p);
        if (statusText != null) statusText.text = MultiUiText.Status(p, startAtMs);
        if (meMark != null) meMark.SetActive(isMe);
        if (championMark != null) championMark.SetActive(isChampion);
    }
}
