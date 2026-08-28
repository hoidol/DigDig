using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class MiniMeInfoPanel : MonoSingleton<MiniMeInfoPanel>
{
    public MiniMePanel miniMePanel;
    
    MiniMe miniMe;
    public void SetMiniMe(MiniMe me)
    {
        miniMe = me;
        gameObject.SetActive(true);
        //nameText.text = me.
        miniMePanel.SetMiniMe(me);
    }
    
}