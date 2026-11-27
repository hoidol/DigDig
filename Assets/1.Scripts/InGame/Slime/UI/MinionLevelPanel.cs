using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class MinionLevelPanel : MonoBehaviour
{
    public GameObject growth0Obj;
    public GameObject growth1Obj;
    public GameObject growth2Obj;
    public Image[] stars; //3개 참조하기


    public void SetMinion(Minion minion)
    {
        growth0Obj.SetActive(false);
        growth1Obj.SetActive(false);
        growth2Obj.SetActive(false);
        if (minion.minionData.growth == 0)
        {
            growth0Obj.SetActive(true);
        }
        else if (minion.minionData.growth == 1)
        {
            growth1Obj.SetActive(true);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < Mathf.Min(minion.mergeLevel + 1, stars.Length); i++)
            {
                stars[i].gameObject.SetActive(true);
            }

        }
        else if (minion.minionData.growth == 2)
        {
            growth2Obj.SetActive(true);
        }


    }

}