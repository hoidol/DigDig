using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyPanel : MonoBehaviour 
{
    public CurrencyType currencyType;
    public TMP_Text currencyText;
    public static Dictionary<CurrencyType, HashSet<CurrencyPanel>> currencyPanelDict = new Dictionary<CurrencyType, HashSet<CurrencyPanel>>();
    void OnEnable()
    {
        UpdatePanel();
        if (!currencyPanelDict.ContainsKey(currencyType))
        {
            currencyPanelDict.Add(currencyType, new HashSet<CurrencyPanel>());
        }
        currencyPanelDict[currencyType].Add(this);
    }

    private void OnDisable()
    {
        currencyPanelDict[currencyType].Remove(this);
    }

    public virtual void UpdatePanel()
    {
        currencyText.text= UserDataManager.Instance.userData.GetCurrency(currencyType).ToString();
    }
}