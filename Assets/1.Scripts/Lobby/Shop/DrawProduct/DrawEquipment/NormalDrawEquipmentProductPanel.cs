using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NormalDrawEquipmentProductPanel : DrawEquipmentProductPanel 
{
public TMP_Text adDrawCountText;
    [Header("광고 시청 뽑기")]
    public int adDrawCountPerDay = 3;
    public int adDrawCount = 3;

    // 광고 뽑기는 재화 구매와 따로 기록해서 하루 횟수를 센다
    string AdDrawKey => $"{key}_Ad";
    int RemainAdDrawCount => Mathf.Max(0, adDrawCountPerDay - UserDataManager.Instance.userPurchaseManager.GetTodayPurchaseCount(AdDrawKey));

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if (adDrawCountText != null)
            adDrawCountText.text = $"{RemainAdDrawCount}/{adDrawCountPerDay}";
        if (adPurchaseButton != null)
            adPurchaseButton.GetComponent<Button>().interactable = RemainAdDrawCount > 0;
    }

    // 광고 시청으로 뽑기, 하루 adDrawCountPerDay회
    public override void OnTryToPurchaseDraw3()
    {
        if (RemainAdDrawCount <= 0)
        {
            ToastCanvas.Toast("오늘 광고 뽑기를 모두 사용했습니다");
            return;
        }

        BlockCanvas.Instance.OpenCanvas("광고 시청중...");
        AdManager.Instance.Play($"product_{key}", AdType.RA, async (success) =>
        {
            await UniTask.SwitchToMainThread(); // 광고 SDK 콜백은 메인 스레드가 아닐 수 있음
            BlockCanvas.Instance.CloseCanvas();
            if (!success)
                return;

            UserDataManager.Instance.userPurchaseManager.AddUserPurchase(AdDrawKey);
            Purchased(adDrawCount);
        });
    }    public GameObject adPurchaseButton;

}