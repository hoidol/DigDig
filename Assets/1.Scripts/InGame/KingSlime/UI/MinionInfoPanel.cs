using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class MinionInfoPanel : MonoSingleton<MinionInfoPanel>
{
    public MinionPanel minionPanel;
    
    Minion minion;
    public void SetMinion(Minion minion)
    {
        this.minion = minion;
        gameObject.SetActive(true);
        //nameText.text = me.
        minionPanel.SetMinion(this.minion);
    }
    
}