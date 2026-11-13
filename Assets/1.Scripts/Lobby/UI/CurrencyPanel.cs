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

    public static void UpdatePanels(CurrencyType currencyType)
    {
        if (!currencyPanelDict.TryGetValue(currencyType, out var panels))
            return;
        foreach (var panel in panels)
            panel.UpdatePanel();
    }

    public static void UpdateAllPanels()
    {
        foreach (var panels in currencyPanelDict.Values)
            foreach (var panel in panels)
                panel.UpdatePanel();
    }

    public virtual void UpdatePanel()
    {
        // 에너지는 BuildSetting.ignoreEnerge면 최대로 표시
        int value = currencyType == CurrencyType.Energe ? EnergeManager.Current : UserDataManager.Instance.userData.GetCurrency(currencyType);
        currencyText.text = value.ToString();
    }
}