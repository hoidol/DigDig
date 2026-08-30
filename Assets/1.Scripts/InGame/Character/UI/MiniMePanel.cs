using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class MiniMePanel : MonoBehaviour
{
    public TMP_Text nameText;
    public Image thumImage;
    public TMP_Text descText;
    public MiniMeLevelPanel levelPanel;

    MiniMe miniMe;
    public void SetMiniMe(MiniMe me)
    {
        miniMe = me;

        if (nameText != null)
            nameText.text = miniMe.MiniMeData.Title;

        if (thumImage != null)
            thumImage.sprite = miniMe.MiniMeData.thum;

        if (descText != null)
            descText.text = miniMe.GetDescription();

        levelPanel?.SetMiniMe(me);
    }

}