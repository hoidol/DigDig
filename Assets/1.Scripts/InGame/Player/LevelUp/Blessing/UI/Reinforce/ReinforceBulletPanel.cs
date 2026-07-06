using UnityEngine;

public class ReinforceBulletPanel : ReinforcePanel
{
    public override void SetReinforce(IReinforce reinforce, int preLv, int nextLv)
    {
        base.SetReinforce(reinforce,preLv,nextLv);
        BulletData bulletData = BulletData.GetBulletData(reinforce.Key);
        thumImage.sprite = bulletData.thumbnail;
        titleText.text = bulletData.Title;
    }
}