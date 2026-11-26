using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 미니언 정보 창의 머지 레벨별 스탯 보기 (▲▼ 버튼으로 머지 레벨 바꿈)
// (2026-11-26) 머지 레벨 표시 = 별 이미지 대신 MergeLvText 글씨 (미니언 머리 위와 같은 숫자 = 머지 레벨 + 1, MinionGameStatePanel.GetMergeLevelText)
public class MinionMergeInfoPanel : MonoBehaviour
{
    public Lobby.MinionStatPanel[] statPanels;
    public GameObject mergeUpButton;
    public GameObject mergeDownButton;
    
    public TMP_Text mergeLvText; // 씬의 MergeLvText
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

    void UpdateMergeLevelText()
    {
        if (mergeLvText != null)
            mergeLvText.text = MinionGameStatePanel.GetMergeLevelText(mergeLevel);
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

            UpdateMergeLevelText();
        }
        List<MinionStat> minionStats = minionData.GetDisplayMinionStats();

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
