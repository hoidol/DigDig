using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class DrawMinionProductPanel : DrawProductPanel
{
    //어떤것들이 나올지 설정해야지
    public DrawMinion drawMinion;
    // 구매 버튼 onPurchased에 등록 (인스펙터에서 뽑기 횟수 입력)
    public virtual void Purchased(int count)
    {
        string[] keys = Draw(count);
        LastDrawKeys = keys;
        AchievementManager.Report(AchievementType.DrawMinion, keys.Length); // 뽑은 장수
        for(int i = 0; i < keys.Length; i++)
        {
            UserDataManager.Instance.userMinionManager.AddUserMinion(keys[i]);
        }
        // 결과 창이 씬에 없으면 지급만 하고 토스트
        if (Lobby.DrawMinionResultCanvas.Instance != null)
            Lobby.DrawMinionResultCanvas.Instance.OpenCanvas(keys, IsUniqueChest);
        else
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Draw_Minion_Get"), keys.Length));
    }

    // 결과 창에 보여줄 상자 종류
    protected virtual bool IsUniqueChest => false;

    protected virtual string[] Draw(int count) => drawMinion.Draw(count);
}
