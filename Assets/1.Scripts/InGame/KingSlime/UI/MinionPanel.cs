using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class MinionPanel : MonoBehaviour
{
    public TMP_Text nameText;
    public Image thumImage;
    public TMP_Text descText;
    public MinionLevelPanel levelPanel;

    Minion minion;
    public void SetMinion(Minion me)
    {
        minion = me;

        if (nameText != null)
            nameText.text = minion.MinionData.Title;

        if (thumImage != null)
            thumImage.sprite = minion.MinionData.thum;

        if (descText != null)
            descText.text = minion.minionData.GetDescription(minion.mergeLevel);

        levelPanel?.SetMinion(me);
    }

}