using UnityEngine;
using System.Collections.Generic;
using TMPro;

using UnityEngine.UI;

public class MergeBulletPanel : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descText;
    public Image bullet1Thum;
    public Image bullet2Thum;
    public Image mergeBulletThum;
    MergeBulletData mergeBulletData;
    
    public void SetMergeBulletData(MergeBulletData mergeBulletData)
    {
        this.mergeBulletData = mergeBulletData;
        BulletData resultBulletData = BulletData.GetBulletData(mergeBulletData.resultBulletKey);
        mergeBulletThum.sprite = resultBulletData.thumbnail;
        titleText.text = resultBulletData.Title;
        descText.text = resultBulletData.GetDescription();
        BulletData bulletData1 = BulletData.GetBulletData(mergeBulletData.resourceBulletKeys[0]);
        BulletData bulletData2 = BulletData.GetBulletData(mergeBulletData.resourceBulletKeys[1]);
    
        bullet1Thum.sprite = bulletData1.thumbnail;
        bullet2Thum.sprite = bulletData2.thumbnail;
    }

    public void OnClicked()
    {
        GetComponentInParent<MergeBulletCanvas>().Selected(mergeBulletData);
    }
}
