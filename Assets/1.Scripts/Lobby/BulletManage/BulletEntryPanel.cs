using UnityEngine;
using UnityEngine.UI;

public class BulletEntryPanel : MonoBehaviour 
{
    [SerializeField] Image thumImage;
    BulletData bulletData ;
    UserBullet userBullet;
    [SerializeField] Button equiptButton; //장착하기
    [SerializeField] GameObject equipingPanel; //장착중 보여줌
    [SerializeField] GameObject unownPanel; //장착중 보여줌
    public void SetBullet(string key)
    {
        bulletData = BulletData.GetBulletData(key);
        userBullet = UserManager.Instance.userBulletManager.GetUserBullet(key);

        thumImage.sprite= bulletData.thumbnail;
        UpdatePanel();
    }
    public void UpdatePanel()
    {
        unownPanel.SetActive(false);
        equipingPanel.SetActive(false);
        equiptButton.gameObject.SetActive(false);

        if (!userBullet.own) //비보유
        {
            thumImage.color= new Color(0.1f,0.1f,0.1f);
            unownPanel.SetActive(true);
        }
        else
        {  
            thumImage.color = Color.white; 
            if(userBullet.equipedIdx == -1)
            {
                equiptButton.gameObject.SetActive(true);
            }
            else
            {
                equipingPanel.SetActive(true);
            }
        }
    }

    public void OnClickedEntry()
    {
        BulletChangeCanvas.Instance.OpenCanvas(() =>
        { 
            GetComponentInParent<BulletManageCanvas>().UpdateCanvas();
        });
    }
    
}