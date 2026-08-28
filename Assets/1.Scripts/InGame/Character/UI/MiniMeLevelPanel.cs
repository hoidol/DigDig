using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class MiniMeLevelPanel : MonoBehaviour
{
    public Image[] stars;

    public void SetMiniMe(MiniMe miniMe)
    {
        for(int i = 0; i < stars.Length; i++)
        {
            stars[i].gameObject.SetActive(false);
        }
        if(miniMe.MiniMeData.growth == 1)
        {
            for(int i = 0; i < miniMe.level; i++)
            {
                stars[i].gameObject.SetActive(true);
            }
        }
    }
    
}