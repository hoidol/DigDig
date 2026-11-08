using System;
using UnityEngine;

public class ProductContainer : MonoBehaviour 
{
    [SerializeField] ProductPanel[] productPanels; // 비워 두면 하위 패널 전부
    public void OpenContainer()
    {
        if (productPanels == null || productPanels.Length == 0)
            productPanels = GetComponentsInChildren<ProductPanel>(true);
        for(int i = 0; i < productPanels.Length; i++)
        {
            productPanels[i].OpenPanel();
        }
    }
    public void UpdateContainer()
    {
        for(int i = 0; i < productPanels.Length; i++)
        {
            productPanels[i].UpdatePanel();
        }
        
    }
    
}