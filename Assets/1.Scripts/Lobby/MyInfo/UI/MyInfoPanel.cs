using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyInfoPanel : MonoBehaviour
{
    public TMP_Text expText;
    public Image barImage;
    public TMP_Text nickText;
    public TMP_Text lvText;

    public void UpdatePanel()
    {
        barImage.fillAmount = (float)UserDataManager.Instance.userMyInfoManager.userMyInfoData.exp/(float)UserDataManager.Instance.userMyInfoManager.GetMaxExp();
        expText.text =$"{UserDataManager.Instance.userMyInfoManager.userMyInfoData.exp}/{UserDataManager.Instance.userMyInfoManager.GetMaxExp()}";
        nickText.text = UserDataManager.Instance.userMyInfoManager.userMyInfoData.nickname;
        lvText.text = $"LV.{UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv}";
    }

    public void OnClickedEditNick()
    {
        
    }
    // public void OnClickedEditMyInfo()
    // {
        
    // }
}