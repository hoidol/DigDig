using System.Collections.Generic;
using UnityEngine;

public class MergeBulletButton : BlessingButton
{
    public override void UpdateButton()
    {
        List<MergeBulletData> canMergeBulletDatas = Player.Instance.weapon.bulletInventory.GetCanMergeBulletData();
        titleText.text= string.Format(TranslateManager.GetText($"MergeBullet_Title"), canMergeBulletDatas.Count);

        gameObject.SetActive(canMergeBulletDatas.Count <= 0);
    }


    public override void OnClickedBtn()
    { 
        Time.timeScale = 0;
        MergeBulletCanvas.Instance.OpenCanvas(() =>
        {
            Time.timeScale = 1;
        });
    }
}