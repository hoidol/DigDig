using System.Collections.Generic;
using UnityEngine;

public class MinionMergeInfoPanel : MonoBehaviour
{
    public Lobby.MinionStatPanel[] statPanels;
    public GameObject mergeUpButton;
    public GameObject mergeDownButton;
    
    public GameObject starsObject;
    public GameObject[] stars;
    public GameObject maxStarObject;
    public int mergeLevel;
    UserMinion userMinion;
    MinionData minionData;
    public void SetMinionData(UserMinion userMinion, MinionData minionData)
    {
        this.userMinion = userMinion;
        this.minionData = minionData;

        mergeLevel = 0;
        
        

        UpdatePanel();
    }

    void UpdateStar()
    {
        starsObject.SetActive(false);
        maxStarObject.SetActive(false);
        if (minionData.growth != 1)
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
        if (minionData.growth == 1)
        {
            if (mergeLevel < MinionData.MAX_MERGE_LEVEL)
                mergeUpButton.SetActive(true);

            if (mergeLevel > 0)
                mergeDownButton.SetActive(true);

            UpdateStar();
        }
        List<MinionStat> minionStats = minionData.GetDisplayMinionStats(userMinion.enhanceLevel);

        for (int i = 0; i < statPanels.Length; i++)
        {
            if (i < minionStats.Count)
            {
                statPanels[i].SetMinionStat(minionStats[i], mergeLevel, minionData.GetRandomRatio(minionStats[i].statType));
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
