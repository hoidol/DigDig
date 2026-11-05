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

    protected int PurchaseCount => UserDataManager.Instance.userPurchaseManager.GetPurchaseCount(key);

    // Purchased에서 구매 기록이 먼저 쌓이므로, 뽑을 때는 이번 구매까지 센 값으로 판정
    protected bool IsSpecialDraw => specialDrawInterval > 0 && PurchaseCount % specialDrawInterval == 0;

    // 특별 뽑기까지 진행도 (0 ~ specialDrawInterval-1)
    protected int SpecialDrawProgress => specialDrawInterval > 0 ? PurchaseCount % specialDrawInterval : 0;
}
