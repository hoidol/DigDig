using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 다른 유저 한 줄 (MultiPlayersPanel이 템플릿을 복사): 닉네임(순위 표시 없음) · WAVE N · 킹 레벨·용사 · [보기]
// 연결이 빈 글씨는 건너뜀 (지금 씬 템플릿엔 미니언 글씨가 없음 → 미니언은 [보기] 화면에서)
public class MultiStatusRow : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text nicknameText;
    public TMP_Text waveText;
    public TMP_Text minionsText;
    public TMP_Text kingText;
    public Button viewButton;
    public GameObject reachedMark; // 도착 (선택)
    public GameObject outMark;     // (2026-11-27) 탈락은 보여주지 않음 → 항상 꺼 둠

    public string Uid { get; private set; }
    System.Action<string> onView;

    void Awake()
    {
        if (viewButton != null)
            viewButton.onClick.AddListener(() => onView?.Invoke(Uid));
    }

    public void Set(MultiPlayerState p, System.Action<string> view)
    {
        Uid = p.uid;
        onView = view;
        if (rankText != null) rankText.gameObject.SetActive(false); // (2026-11-27) 순위 표시 없음
        if (nicknameText != null) nicknameText.text = MultiGame.DisplayName(p);
        if (waveText != null) waveText.text = MultiUiText.Wave(p);
        if (minionsText != null) minionsText.text = MultiUiText.Minions(p.minions, 6);
        if (kingText != null) kingText.text = MultiUiText.King(p);
        if (reachedMark != null) reachedMark.SetActive(p.HasReached);
        if (outMark != null) outMark.SetActive(false);
    }
}
