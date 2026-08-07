using System;
using UnityEngine;

public class ProductContainer : MonoBehaviour 
{
    [SerializeField] ProductPanel[] productPanels;
    public void OpenContainer()
    {
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