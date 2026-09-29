using System;
using UnityEngine;

public class ShopCanvas : BaseLobbyCanvas
{
    //장비 뽑기!
    //펫 뽑기! 
    public ProductContainer[] productContainers;

    public override void Init()
    {
        if (init)
            return;
        base.Init();
        productContainers = GetComponentsInChildren<ProductContainer>();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for (int i = 0; i < productContainers.Length; i++)
        {
            productContainers[i].OpenContainer();
        }

        UpdateCanvas();
    }

    public override void UpdateCanvas()
    {
        for (int i = 0; i < productContainers.Length; i++)
        {
            productContainers[i].UpdateContainer();
        }
    }
}

//미니미를 뽑아야되는가?
//미니미는 Stage를 클리어하면 보상으로 받게하자.
//