using System.Collections.Generic;
using UnityEngine;

public class MergeRecommendContainer : MonoBehaviour
{
    public MergeRecommendButton prefab;
    public List<MergeRecommendButton> mergeRecommendButtons = new();

    void Awake()
    {
        GameEventBus.Subscribe<UpdaterMergeRecommendItemEvent>(UpdaterMergeRecommendItem);
    }

    void UpdaterMergeRecommendItem(UpdaterMergeRecommendItemEvent e)
    {
        mergeRecommendButtons.ForEach(e => e.gameObject.SetActive(false));
        for (int i = 0; i < e.recommendMergeItems.Count; i++)
        {
            MergeRecommendButton button = GetMergeRecommendButton();
            button.SetMergeItemData(e.recommendMergeItems[i]);
        }
    }
    MergeRecommendButton GetMergeRecommendButton()
    {
        for (int i = 0; i < mergeRecommendButtons.Count; i++)
        {
            if (mergeRecommendButtons[i].gameObject.activeSelf)
                continue;
            mergeRecommendButtons[i].gameObject.SetActive(true);
            return mergeRecommendButtons[i];
        }
        MergeRecommendButton button = Instantiate(prefab, transform);
        mergeRecommendButtons.Add(button);
        return button;



    }

}
