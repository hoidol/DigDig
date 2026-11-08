using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class DrawEquipmentProductPanel : DrawProductPanel
{
    //어떤것들이 나올지 설정해야지
    public DrawEquipment drawEquipment;
    protected virtual string[] Draw(int count) => drawEquipment.Draw(count);

    // 구매 버튼 onPurchased에 등록 (인스펙터에서 뽑기 횟수 입력)
    public virtual void Purchased(int count)
    {
        string[] keys = Draw(count);
        LastDrawKeys = keys;
        for(int i = 0; i < keys.Length; i++)
        {
            UserDataManager.Instance.userEquipmentManager.AddUserEquipment(keys[i]);
        }
        // 결과 창이 씬에 없으면 지급만 하고 토스트
        if (Lobby.DrawEquipmentResultCanvas.Instance != null)
            Lobby.DrawEquipmentResultCanvas.Instance.OpenCanvas(keys);
        else
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Draw_Equipment_Get"), keys.Length));
    }
}
