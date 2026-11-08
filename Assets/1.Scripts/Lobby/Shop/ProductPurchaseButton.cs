using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// key(ProductData.key)를 넣으면 켜질 때 그 상품 가격으로 UI 설정
// 인앱 = 스토어 가격 텍스트, 광고 = 광고 아이콘, 재화 = 재화 아이콘 + 가격
// 누르면 구매 처리(ProductManager.TryToBuy: 재화 확인·차감 / 인앱 결제 / 광고 시청) → 구매 기록(key) → onPurchased → 상위 ProductPanel.UpdatePanel
// 상품 구분(key)은 패널이 아니라 버튼마다 (한 패널에 10회/30회/광고 버튼 등)
public class ProductPurchaseButton : MonoBehaviour
{
    public string key; // ProductData.key = 구매 기록 key
    public TMP_Text inAppPurchasePriceText;
    public CurrencyDisplayPanel currencyDisplayPanel;

    [Header("하루 구매 제한 (0이면 없음)")]
    public int dailyLimit;
    public TMP_Text dailyRemainText; // "남은/제한"

    // 구매 성공 시 호출 (구매 기록 후). 인스펙터에서 패널 함수 등록 (뽑기는 Purchased(int)에 횟수 입력)
    public UnityEvent onPurchased = new UnityEvent();

    public ProductData ProductData => productData;
    ProductData productData;
    bool isPurchasing; // 결제·광고 진행 중 중복 클릭 방지

    void Awake()
    {
        if (TryGetComponent(out Button button))
            button.onClick.AddListener(Purchase);
    }

    public int TodayPurchaseCount => UserDataManager.Instance.userPurchaseManager.GetTodayPurchaseCount(key);
    public int DailyRemainCount => dailyLimit > 0 ? Mathf.Max(0, dailyLimit - TodayPurchaseCount) : int.MaxValue;

    void OnEnable()
    {
        Refresh();
    }

    // key로 가격 UI·하루 남은 횟수 갱신
    public void Refresh()
    {
        if (!string.IsNullOrEmpty(key))
            SetProductKey(key);
        UpdateDailyLimit();
    }

    void UpdateDailyLimit()
    {
        if (dailyLimit <= 0 || UserDataManager.Instance == null)
            return;
        int remain = DailyRemainCount;
        if (dailyRemainText != null)
            dailyRemainText.text = $"{remain}/{dailyLimit}";
        if (TryGetComponent(out Button button))
            button.interactable = remain > 0;
    }

    public void SetProductKey(string key)
    {
        this.key = key;
        ProductData data = ProductManager.Instance != null ? ProductManager.Instance.GetProductData(key) : null;
        if (data == null)
        {
            Debug.LogWarning($"[ProductPurchaseButton] ProductData 없음 : '{key}' ({name})");
            return;
        }
        SetProductData(data);
    }

    public void SetProductData(ProductData productData)
    {
        this.productData = productData;
        // ?.는 인스펙터에서 비워 둔 Unity 오브젝트를 null로 못 걸러서 명시적으로 비교
        if (inAppPurchasePriceText != null)
            inAppPurchasePriceText.gameObject.SetActive(false);
        if (currencyDisplayPanel != null)
            currencyDisplayPanel.gameObject.SetActive(false);

        // 인앱인데 가격 텍스트가 없으면 재화 패널(인앱 아이콘)에 가격 표시
        if (!string.IsNullOrEmpty(productData.productId) && inAppPurchasePriceText != null)
        {
            inAppPurchasePriceText.gameObject.SetActive(true);
            inAppPurchasePriceText.text = productData.GetPriceToString();
        }
        else
        {
            if(currencyDisplayPanel != null)
            {
                currencyDisplayPanel.gameObject.SetActive(true);
                currencyDisplayPanel.currencyImage.sprite = CurrencyInfo.GetSprite(productData.currencyType);
                // 광고 상품은 가격 대신 "광고"
                currencyDisplayPanel.valueText.text = productData.currencyType == CurrencyType.Ad ? TranslateManager.GetText("Ad") : productData.GetPriceToString();
            }

        }
    }

    // 재화 상품 = 잔액 확인 후 차감, 인앱 = 결제 완료 후, 광고 = 시청 완료 후 onPurchased
    public void Purchase()
    {
        if (productData == null || isPurchasing)
            return;
        if (DailyRemainCount <= 0)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Purchase_DailyLimit"));
            return;
        }

        isPurchasing = true;
        BlockCanvas.Instance.OpenCanvas(productData.currencyType == CurrencyType.Ad ? TranslateManager.GetText("Ad_Watching") : TranslateManager.GetText("Purchasing"));
        ProductManager.Instance.TryToBuy(productData, async (success) =>
        {
            await UniTask.SwitchToMainThread(); // 결제·광고 SDK 콜백은 메인 스레드가 아닐 수 있음
            isPurchasing = false;
            BlockCanvas.Instance.CloseCanvas();
            if (!success)
                return;

            // 기록이 먼저 (뽑기 패널의 특별 뽑기 판정은 이번 구매까지 센 값)
            UserDataManager.Instance.userPurchaseManager.AddUserPurchase(productData);
            onPurchased.Invoke();
            UpdateDailyLimit();
            ProductPanel panel = GetComponentInParent<ProductPanel>(true);
            if (panel != null)
                panel.UpdatePanel();
        });
    }
}
