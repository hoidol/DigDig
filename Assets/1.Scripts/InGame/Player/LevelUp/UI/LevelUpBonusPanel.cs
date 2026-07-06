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

    public virtual void SetBonusPanel()
    {
        titleText.text= TranslateManager.GetText($"{levelUpBonusType}_title");
        descriptionText.text= TranslateManager.GetText($"{levelUpBonusType}_description");
    }

    public void OnClickedSelect()
    {
        LevelUpCanvas.Instance.CloseCanvas();
        switch (levelUpBonusType)
        {
            case LevelUpBonusType.AddNormalBullet:
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
                break;
        }


    }
}
public enum LevelUpBonusType
{
    AddNormalBullet,
    AddBounce,
    AddSpecialBullet,
    // MergeBullet
}
