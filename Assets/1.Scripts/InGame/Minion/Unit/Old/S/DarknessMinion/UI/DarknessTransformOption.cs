using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 다크니스 변신 목록의 선택지 하나: 미니언 썸네일 + 이름. 버튼은 코드에서 연결
public class DarknessTransformOption : MonoBehaviour
{
    public Button button;
    public Image thumImage;
    // public TMP_Text nameText;

    DarknessTransformPopup popup;
    string key;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => popup?.Select(key));
    }

    public void Set(DarknessTransformPopup popup, MinionData data)
    {
        this.popup = popup;
        key = data.key;
        if (thumImage != null)
            thumImage.SetThum(data.thum);
    }
}
