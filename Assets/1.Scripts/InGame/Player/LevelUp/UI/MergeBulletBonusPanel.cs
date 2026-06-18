using System.Collections.Generic;
using UnityEngine;

public class MergeBulletBonusPanel : LevelUpBonusPanel
{
    public override void SetBonusPanel()
    {
        base.SetBonusPanel();
        List<MergeBulletData> canMergeBulletDatas = Player.Instance.weapon.bulletInventory.GetCanMergeBulletData();
        descriptionText.text= string.Format(TranslateManager.GetText($"{levelUpBonusType}_description"), canMergeBulletDatas.Count);
    }
}