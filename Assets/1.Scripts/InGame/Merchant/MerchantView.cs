using UnityEngine.UI;
using UnityEngine;

public class MerchantView : MonoBehaviour
{
    public Image timerImage;

    public void SetTimer(float rate)
    {
        timerImage.fillAmount = rate;
    }

}