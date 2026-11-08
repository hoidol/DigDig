using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class DrawSlimeProductPanel : DrawProductPanel
{
    //어떤것들이 나올지 설정해야지
    public DrawSlime drawSlime;
    // 구매 버튼 onPurchased에 등록 (인스펙터에서 뽑기 횟수 입력)
    public virtual void Purchased(int count)
    {
        string[] keys = Draw(count);
        LastDrawKeys = keys;
        for(int i = 0; i < keys.Length; i++)
        {
            UserDataManager.Instance.userSlimeManager.AddUserSlime(keys[i]);
        }
        // 결과 창이 씬에 없으면 지급만 하고 토스트
        if (Lobby.DrawSlimeResultCanvas.Instance != null)
            Lobby.DrawSlimeResultCanvas.Instance.OpenCanvas(keys);
        else
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Draw_Slime_Get"), keys.Length));
    }

    protected virtual string[] Draw(int count) => drawSlime.Draw(count);
}
