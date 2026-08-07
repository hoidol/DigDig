using System;
using UnityEngine;

public class ShopCanvas : CanvasUI<ShopCanvas>
{
    //장비 뽑기!
    //펫 뽑기! 
    public ProductContainer[] productContainers;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < productContainers.Length; i++)
        {
            productContainers[i].OpenContainer();
        }

        UpdateCanvas();
    }

    void UpdateCanvas()
    {
        for(int i = 0; i < productContainers.Length; i++)
        {
            productContainers[i].UpdateContainer();
        }
        
    }

}