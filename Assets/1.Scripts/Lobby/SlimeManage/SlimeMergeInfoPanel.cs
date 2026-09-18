using System.Collections.Generic;
using UnityEngine;

public class SlimeMergeInfoPanel : MonoBehaviour
{
    public SlimeStatPanel[] statPanels;
    public GameObject mergeUpButton;
    public GameObject mergeDownButton;
    public GameObject growth0StarObject;
    public GameObject growth1StarsObject;
    public GameObject[] growth1Stars;
    public GameObject growth2StarObject;
    public int mergeLevel;
    UserSlime userSlime;
    SlimeData slimeData;
    public void SetSlimeData(UserSlime userSlime, SlimeData slimeData)
    {
        this.userSlime = userSlime;
        this.slimeData = slimeData;

        mergeLevel = 0;
        growth0StarObject.SetActive(false);
        growth1StarsObject.SetActive(false);
        growth2StarObject.SetActive(false);
        
        if(slimeData.growth == 0)
        {
            growth0StarObject.SetActive(true);
        }else if(slimeData.growth == 1)
        {
            growth1StarsObject.SetActive(true);
            UpdateStar();
        }else if(slimeData.growth == 2)
        {
            growth2StarObject.SetActive(true);
        }

        UpdatePanel();
    }   

    void UpdateStar()
    {
        if(slimeData.growth != 1)
        {
            return;            
        }

        for(int i = 0; i < growth1Stars.Length; i++)
        {
            growth1Stars[i].SetActive(false);
        }
        
        for(int i = 0; i < growth1Stars.Length; i++)
        {
            if(mergeLevel >= i)
            {
                growth1Stars[i].SetActive(true);
            }
        }
    }

    public void UpdatePanel()
    {
        mergeUpButton.SetActive(false);
        mergeDownButton.SetActive(false);
        if(slimeData.growth == 1)
        {
            if(mergeLevel < 2)
                mergeUpButton.SetActive(true);
            
            if(mergeLevel >0)
                mergeDownButton.SetActive(true);

            UpdateStar();
        }
        List<SlimeStat> slimeStats = slimeData.GetDisplaySlimeStats(userSlime.enhanceLevel);

        for (int i = 0; i < statPanels.Length; i++)
        {
            if (i < slimeStats.Count)
            {
                statPanels[i].SetSlimeStat(slimeStats[i], mergeLevel);
                statPanels[i].gameObject.SetActive(true);
            }
            else
            {
                statPanels[i].gameObject.SetActive(false);
            }
            
        }
    }

    public void OnClickedMergeUp()
    {
        mergeLevel++;
        UpdatePanel();
    }
    public void OnClickedMergeDown()
    {
        mergeLevel--;
        UpdatePanel();
    }
}
