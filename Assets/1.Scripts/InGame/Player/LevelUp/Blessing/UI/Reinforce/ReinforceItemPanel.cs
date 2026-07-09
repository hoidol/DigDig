using UnityEngine;

public class ReinforceItemPanel : ReinforcePanel
{
     public override void SetReinforce(IReinforce reinforce, int preLv, int nextLv)
    {
        base.SetReinforce(reinforce,preLv,nextLv);
        ItemData itemData = ItemData.GetItemData(reinforce.Key);
        thumImage.sprite = itemData.thumbnail;
        titleText.text = itemData.Title;
    }
}