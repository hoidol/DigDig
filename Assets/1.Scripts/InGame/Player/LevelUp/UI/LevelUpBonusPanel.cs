using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpBonusPanel : MonoBehaviour
{

    // public Image thumImage;
    public Image frameImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public LevelUpBonusType levelUpBonusType;

    public void SetBonusPanel()//
    {
        //this.bonusLv = bonusLv;
    }

    public void OnClickedSelect()
    {
        LevelUpCanvas.Instance.CloseCanvas();
        switch (levelUpBonusType)
        {
            case LevelUpBonusType.AddNormalBullet:
                // int count = bonusLv * 2;
                // for (int i = 0; i < count; i++)
                // {
                //     Player.Instance.bulletInventory.AddBullet("Normal");
                // }
                Player.Instance.weapon.AddBullet("Normal");
                Player.Instance.weapon.AddBullet("Normal");

                break;
            case LevelUpBonusType.AddBounce:
                Player.Instance.AddBounce(1);
                break;
            case LevelUpBonusType.AddSpecialBullet:
                // Time.timeScale = 0;
                BulletData pickedBulletData = BulletManager.Instance.DrawRandomBullet();
                BulletShortInfoPanel.Instance.AddShortInfo(pickedBulletData, true);
                Player.Instance.weapon.ReleaseBullet("Normal");
                Player.Instance.weapon.AddBullet(pickedBulletData.key);

                // pickedBulletPanel.SetBulletData(pickedBulletData);
                // if (!alreadyPicked.Contains(pickedBulletData.key))
                // {
                //     alreadyPicked.Add(pickedBulletData.key);
                // }
                // Player.Instance.weapon.AddBullet(pickedBulletData);
                // PickedBulletCanvas.Instance.OpenCanvas(() =>
                // {
                //     Time.timeScale = 1;
                // });
                break;
            case LevelUpBonusType.MergeBullet:
                Time.timeScale = 0;
                MergeBulletCanvas.Instance.OpenCanvas(() =>
                {
                    Time.timeScale = 1;
                });
                break;
        }


    }
}
public enum LevelUpBonusType
{
    AddNormalBullet,
    AddBounce,
    AddSpecialBullet,
    MergeBullet
}
