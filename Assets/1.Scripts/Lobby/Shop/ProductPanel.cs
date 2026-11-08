using UnityEngine;

// 상품 패널: 상품 구분(key)·구매·구매 기록은 하위 ProductPurchaseButton마다.
// 패널은 버튼 onPurchased에 등록할 지급 함수(Purchased)와 표시 갱신(UpdatePanel)만 담당
public class ProductPanel : MonoBehaviour
{
    protected ProductPurchaseButton[] purchaseButtons;

    public virtual void OpenPanel()
    {
        gameObject.SetActive(true);
        purchaseButtons = GetComponentsInChildren<ProductPurchaseButton>(true);
        foreach (ProductPurchaseButton button in purchaseButtons)
            button.Refresh();
        UpdatePanel();
    }
    public virtual void UpdatePanel()
    {

    }

    // 버튼 onPurchased에 등록 (구매 기록은 버튼이 이미 남김)
    public virtual void Purchased()
    {
    }

}
