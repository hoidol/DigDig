using TMPro;
using UnityEngine;

public class RankingPanel : MonoBehaviour
{
    public TMP_Text numText;
    public TMP_Text nicknameText;
    public TMP_Text waveText;
    public GameObject myHighlight; // 내 줄 강조 (선택)

    // numOverride : 순위 대신 표시할 문자열 (목록 밖 "100+" 등)
    public void SetRanking(RankData data, bool isMine, string numOverride = null)
    {
        numText.text = numOverride ?? data.num.ToString();
        nicknameText.text = data.nickname;
        waveText.text = data.wave > 0 ? $"Wave {data.wave}" : "-";
        if (myHighlight != null)
            myHighlight.SetActive(isMine);
    }
}
