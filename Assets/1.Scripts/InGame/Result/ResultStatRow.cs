using TMPro;
using UnityEngine;

// (2026-11-27) 결과 화면 통계 한 칸: 이름 + 값 (ResultStatsPanel이 템플릿을 복사해 씀)
public class ResultStatRow : MonoBehaviour
{
    public TMP_Text labelText;
    public TMP_Text valueText;

    public void Set(string label, string value)
    {
        if (labelText != null)
            labelText.text = label;
        if (valueText != null)
            valueText.text = value;
    }
}
