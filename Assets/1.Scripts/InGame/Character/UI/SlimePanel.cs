using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class SlimePanel : MonoBehaviour
{
    public TMP_Text nameText;
    public Image thumImage;
    public TMP_Text descText;
    public SlimeLevelPanel levelPanel;

    Slime slime;
    public void SetSlime(Slime me)
    {
        slime = me;

        if (nameText != null)
            nameText.text = slime.SlimeData.Title;

        if (thumImage != null)
            thumImage.sprite = slime.SlimeData.thum;

        if (descText != null)
            descText.text = slime.GetDescription(slime.level);

        levelPanel?.SetSlime(me);
    }

}