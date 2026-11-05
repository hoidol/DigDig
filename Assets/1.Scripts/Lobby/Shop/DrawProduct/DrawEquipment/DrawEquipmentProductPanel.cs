using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class DrawEquipmentProductPanel : DrawProductPanel
{
    //어떤것들이 나올지 설정해야지
    public DrawEquipment drawEquipment;
    protected virtual string[] Draw(int count) => drawEquipment.Draw(count);
    public virtual void Purchased(int count)
    {
        base.Purchased();
        drawEquipment.count = count;
        string[] keys = drawEquipment.Draw(count);
        for(int i = 0; i < keys.Length; i++)
        {
            UserDataManager.Instance.userSlimeManager.AddUserSlime(keys[i]);
        }
        Lobby.DrawSlimeResultCanvas.Instance.OpenCanvas(keys);
        UpdatePanel();
    }
    public virtual void OnTryToPurchaseDraw3()
    {
        //광고 시청으로 뽑음 기회 하루에 3회
        BlockCanvas.Instance.OpenCanvas("구매중...");
        ProductManager.Instance.TryToBuy(productData, (success) =>
        {
            if (success)
            {
                Purchased(3); // Example: Draw 10 slimes
            }
            BlockCanvas.Instance.CloseCanvas();
        });
    }

    public virtual void OnTryToPurchaseDraw10()
    {
        //DrawTicket 30개 소비
        BlockCanvas.Instance.OpenCanvas("구매중...");
        ProductManager.Instance.TryToBuy(productData, (success) =>
        {
            if (success)
            {
                Purchased(10); // Example: Draw 30 slimes
            }
            BlockCanvas.Instance.CloseCanvas();
        });
        
    }
}
