using TMPro;
using UnityEngine;

// 선택한 유닛 설정 버튼 공용 (타겟 방식 / 버프 범위 / 회전 방향)
// 유닛마다 쓸 수 있는 설정만 노출: IsAvailable이 false면 버튼을 숨긴다
// 누를 때마다 다음 값으로 순환 → 텍스트 갱신
public abstract class AllyOptionButton : MonoBehaviour
{
    public TMP_Text targetInfoText;
    protected IAllyUnit targetAlly;

    public void SetSlime(IAllyUnit unit)
    {
        targetAlly = unit;
        bool show = unit != null && IsAvailable(unit);
        gameObject.SetActive(show);
        if (show)
            UpdateText();
    }

    // 이 유닛에 이 설정 버튼을 보여줄지
    public abstract bool IsAvailable(IAllyUnit unit);

    // 다음 값으로 순환
    protected abstract void SetNext();

    protected abstract string GetText();

    // 버튼 onClick에 연결
    public void OnClickedButton()
    {
        if (targetAlly == null || !IsAvailable(targetAlly))
            return;
        SetNext();
        UpdateText();
    }

    protected void UpdateText()
    {
        if (targetInfoText == null || targetAlly == null)
            return;
        targetInfoText.text = GetText();
    }
}
