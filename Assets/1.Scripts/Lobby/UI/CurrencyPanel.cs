using TMPro;
using UnityEngine;

public class CurrencyPanel : MonoBehaviour 
{
    public CurrencyType currencyType;
    public TMP_Text currencyText;

    void OnEnable()
    {
        UpdatePanel();
    }

    public virtual void UpdatePanel()
    {
        currencyText.text= UserManager.Instance.userData.GetCurrency(currencyType).ToString();
    }
}