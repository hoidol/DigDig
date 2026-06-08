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

    public void SetMergeBulletData(MergeBulletData mergeBulletData)
    {
        BulletData bulletData1 = BulletData.GetBulletData(mergeBulletData.resourceBulletKeys[0]);
        BulletData bulletData2 = BulletData.GetBulletData(mergeBulletData.resourceBulletKeys[1]);
    }

    public void OnClicked()
    {

    }
}
