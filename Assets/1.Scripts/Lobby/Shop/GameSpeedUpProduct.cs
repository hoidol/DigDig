using TMPro;
using UnityEngine;

// 구매하면 인게임 배속을 5배까지 쓸 수 있다 (SpeedUpButton)
public class GameSpeedUpProduct : ProductPanel
{
    public GameObject purchasedObject; // 구매 완료 표시
    public TMP_Text descText;

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if (descText != null)
            descText.text = ProductData.GetDesc(InAppProductInfo.GAME_SPEED_UP_KEY);
        if (purchasedObject != null)
            purchasedObject.SetActive(UserDataManager.Instance.IsGameSpeedUp);
    }

}
