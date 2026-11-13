using UnityEngine;
using TMPro;

// 씬에 박힌 고정 글씨(제목·버튼 이름 등)를 번역 시트 key로 출력. 언어를 바꾸면 TranslateManager.ChangeLanguage가 다시 그린다.
// formatArg가 있으면 번역문의 {0} 자리에 넣는다 (예: "Lv {0} 권능" + "10")
[RequireComponent(typeof(TMP_Text))]
public class TranslateText : MonoBehaviour
{
    public TMP_Text text;
    public string key;
    public string formatArg;

    void Start()
    {
        UpdatePanel();
    }

    public void UpdatePanel()
    {
        if (string.IsNullOrEmpty(key))
            return;
        if (text == null) // 아직 켜진 적 없는 오브젝트도 언어 변경 때 갱신되게
            text = GetComponent<TMP_Text>();
        if (text == null)
            return;
        string value = TranslateManager.GetText(key);
        text.text = string.IsNullOrEmpty(formatArg) ? value : string.Format(value, formatArg);
    }
}
