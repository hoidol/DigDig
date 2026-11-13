using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 조커 변신 선택지 하나: 슬라임 아이콘 + 이름. 버튼 onClick에 OnClickedButton 연결
public class JokerSelectPanel : MonoBehaviour
{
    public Image thumImage;
    public TMP_Text nameText;
    public TMP_Text levelText; // 선택: 변신 후 별(머지 레벨) 표시

    string key;

    public void Set(MinionData data, int mergeLevel)
    {
        key = data.key;
        if (thumImage != null)
            thumImage.sprite = data.thum;
        if (nameText != null)
            nameText.text = data.Title;
        if (levelText != null)
            levelText.text = $"★{mergeLevel + 1}";
    }

    public void OnClickedButton()
    {
        JokerSelectCanvas.Instance.Selected(key);
    }
}
