using UnityEngine;
using UnityEngine.UI;

public class BulletEquiptPanel : MonoBehaviour 
{
    [SerializeField] Image thumImage;
     BulletData bulletData ;
    UserBullet userBullet;
    public int index;
    public void SetBullet(string key, int idx)
    {
        index= idx;
        bulletData = BulletData.GetBulletData(key);
        userBullet = UserManager.Instance.userBulletManager.GetUserBullet(key);

        thumImage.sprite= bulletData.thumbnail;
        UpdatePanel();
    }
    public void UpdatePanel()
    {
        
    }

}