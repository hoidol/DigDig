using System.Collections.Generic;
using UnityEngine;

public class SlimeMergeInfoPanel : MonoBehaviour
{
    public Lobby.SlimeStatPanel[] statPanels;
    public GameObject mergeUpButton;
    public GameObject mergeDownButton;
    
    public GameObject starsObject;
    public GameObject[] stars;
    public GameObject maxStarObject;
    public int mergeLevel;
    UserSlime userSlime;
    SlimeData slimeData;
    public void SetSlimeData(UserSlime userSlime, SlimeData slimeData)
    {
        this.userSlime = userSlime;
        this.slimeData = slimeData;

        mergeLevel = 0;
        
        

        UpdatePanel();
    }

    void UpdateStar()
    {
        starsObject.SetActive(false);
        maxStarObject.SetActive(false);
        if (slimeData.growth != 1)
        {
            return;
        }
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive(false);
        }

        if(mergeLevel <= 3)
        {
            starsObject.SetActive(true);
            for (int i = 0; i < stars.Length; i++)
            {
                if(i <= mergeLevel)
                    stars[i].SetActive(true);
            }
        }
        else
        {
            maxStarObject.SetActive(true);
        }
    }

    public void UpdatePanel()
    {
        mergeUpButton.SetActive(false);
        mergeDownButton.SetActive(false);
        if (slimeData.growth == 1)
        {
            if (mergeLevel < SlimeData.MAX_MERGE_LEVEL)
                mergeUpButton.SetActive(true);

            if (mergeLevel > 0)
                mergeDownButton.SetActive(true);

            UpdateStar();
        }
        List<SlimeStat> slimeStats = slimeData.GetDisplaySlimeStats(userSlime.enhanceLevel);

        for (int i = 0; i < statPanels.Length; i++)
        {
            if (i < slimeStats.Count)
            {
                statPanels[i].SetSlimeStat(slimeStats[i], mergeLevel, slimeData.GetRandomRatio(slimeStats[i].statType));
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
