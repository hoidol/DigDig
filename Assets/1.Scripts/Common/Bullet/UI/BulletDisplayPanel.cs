using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BulletDisplayPanel : MonoBehaviour 
{
    public Image thumImage;
    public TMP_Text titleText;
    public void SetBulletData(string key)
    {
        BulletData bulletData = BulletData.GetBulletData(key);
        if(thumImage != null)
        {
            thumImage.sprite = bulletData.thumbnail;
        }
            

        if(titleText != null)
        {
            titleText.text = bulletData.Title;
        }
    }
}