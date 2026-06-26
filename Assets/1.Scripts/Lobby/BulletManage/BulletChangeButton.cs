using UnityEngine;
using UnityEngine.UI;

public class BulletChangeButton : ButtonUI 
{
    public int index;
    public Image thumImage;
    public void Init(int idx)
    {
        index = idx;
    }
    BulletData bulletData;
    public void SetBullet(string key)
    {
        bulletData = BulletData.GetBulletData(key);
        thumImage.sprite = bulletData.thumbnail;
    }

    public override void OnClickedBtn()
    {
        UserManager.Instance.userBulletManager.EquiptUserBullet(bulletData.key,index);
        BulletChangeCanvas.Instance.CloseCanvas();
    }
}