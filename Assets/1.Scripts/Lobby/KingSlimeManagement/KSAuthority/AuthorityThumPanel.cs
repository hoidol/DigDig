using UnityEngine;
using UnityEngine.UI;

// 권능 썸네일 (권능은 등급 없음). 목록 칸·정보 창 공용
public class AuthorityThumPanel : MonoBehaviour
{
    public Image thumImage;
    public bool isNullToInactive;

    public void SetAuthorityData(AuthorityData authorityData)
    {
        if (thumImage != null)
        {
            thumImage.enabled = authorityData != null;
            if (authorityData != null)
                thumImage.sprite = authorityData.thum;
        }
        if (authorityData != null)
            gameObject.SetActive(true);
        else if (isNullToInactive)
            gameObject.SetActive(false);
    }
}
