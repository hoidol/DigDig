using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class SlimeLevelPanel : MonoBehaviour
{
    public Image[] stars; //3개 참조하기
    public Image redStar;

    public void SetSlime(Slime slime)
    {
        for(int i = 0; i < stars.Length; i++)
        {
            stars[i].gameObject.SetActive(false);
        }

        redStar.gameObject.SetActive(false);
        
        if(slime.SlimeData.growth == 1)
        {
            for(int i = 0; i < slime.level+1; i++)
            {
                stars[i].gameObject.SetActive(true);
            }
        }else if(slime.SlimeData.growth == 2)
        {
            redStar.gameObject.SetActive(true);
        }
    }
    
}