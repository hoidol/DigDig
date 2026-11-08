using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


// 뽑기 상품 공통: specialDrawInterval회 구매마다 결과 1개를 special 설정(높은 등급 확률표)에서 뽑는다
public class DrawProductPanel : ProductPanel
{
    [Header("N회 구매마다 특별 뽑기 (0이면 없음)")]
    public int specialDrawInterval;
    public TMP_Text specialDrawInfoText;

    public string[] LastDrawKeys { get; protected set; } // 마지막 뽑기 결과 (높은 등급 순)

    // 이 패널 하위 구매 버튼들(key)의 구매 횟수 합. 특별 뽑기는 Unique 패널만 쓰므로(specialDrawInterval > 0) 그 패널 구매만 카운트
    protected int PurchaseCount
    {
        get
        {
            if (purchaseButtons == null)
                purchaseButtons = GetComponentsInChildren<ProductPurchaseButton>(true);
            return purchaseButtons.Sum(b => UserDataManager.Instance.userPurchaseManager.GetPurchaseCount(b.key));
        }
    }

    // 버튼이 구매 기록을 먼저 남기고 Purchased(count)를 부르므로, 뽑을 때는 이번 구매까지 센 값으로 판정
    protected bool IsSpecialDraw => specialDrawInterval > 0 && PurchaseCount % specialDrawInterval == 0;

    // 특별 뽑기까지 진행도 (0 ~ specialDrawInterval-1)
    protected int SpecialDrawProgress => specialDrawInterval > 0 ? PurchaseCount % specialDrawInterval : 0;
}
