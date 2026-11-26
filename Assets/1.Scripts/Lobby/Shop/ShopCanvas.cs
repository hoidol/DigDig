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
        // (2026-11-26) 용사·미니언 판매 줄: 씬에 둔 줄(UnitShopSection)의 칸 템플릿을 데이터 수만큼 복사 (씬에 만들기 = Tools/Lobby/Add Hero·Minion Shop Sections)
        foreach (UnitShopSection section in GetComponentsInChildren<UnitShopSection>(true))
            section.Fill();
        productContainers = GetComponentsInChildren<ProductContainer>();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        Init(); // (2026-11-26) 처음 열 때 용사·미니언 칸 생성 + 상품 줄 목록 (Init을 부르는 곳이 없었음)
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