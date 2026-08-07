using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class InAppPurchaseManager : MonoSingleton<InAppPurchaseManager>
{
    [Serializable]
    public class IAPProductInfo
    {
        public string productId;
        public ProductType productType;
    }

    [SerializeField] private List<IAPProductInfo> productList = new();

    public bool IsInitialized { get; private set; }

    public event Action OnStoreInitialized;
    public event Action<Product> OnPurchaseSuccess;
    public event Action<Product, string> OnPurchaseFailed;
    public event Action<Product> OnPurchaseDeferred;

    private StoreController storeController;
    private readonly HashSet<string> processedOrderIds = new();
    private Action<bool> purchaseCallback;

    async void Awake()
    {
        storeController = UnityIAPServices.StoreController();

        // 재연결 시 미완료 구매가 즉시 발생할 수 있으므로 Connect() 이전에 전부 구독한다
        storeController.OnPurchasePending += HandlePurchasePending;
        storeController.OnPurchaseFailed += HandlePurchaseFailed;
        storeController.OnPurchaseDeferred += HandlePurchaseDeferred;
        storeController.OnStoreConnected += HandleStoreConnected;
        storeController.OnStoreDisconnected += HandleStoreDisconnected;

        await storeController.Connect();
    }

    private void HandleStoreConnected()
    {
        List<ProductDefinition> definitions = productList
            .Select(info => new ProductDefinition(info.productId, info.productType))
            .ToList();

        storeController.OnProductsFetched += HandleProductsFetched;
        storeController.OnProductsFetchFailed += HandleProductsFetchFailed;
        storeController.FetchProducts(definitions);
    }

    private void HandleStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        Debug.LogError($"[IAP] 스토어 연결 실패 : {failure.Message}");
    }

    private void HandleProductsFetched(List<Product> products)
    {
        IsInitialized = true;
        Debug.Log($"[IAP] 상품 정보 로드 완료 : {products.Count}개");
        OnStoreInitialized?.Invoke();
    }

    private void HandleProductsFetchFailed(ProductFetchFailed failure)
    {
        Debug.LogError($"[IAP] 상품 정보 로드 실패 : {failure.FailureReason}");
    }

    public Product GetProduct(string productId)
    {
        return storeController.GetProductById(productId);
    }

    // "KRW 1100" 형식의 문자열을 반환한다. 상품 정보를 아직 못 가져왔으면 빈 문자열을 반환한다
    public string GetFormattedPrice(string productId)
    {
        Product product = GetProduct(productId);
        if (product?.metadata == null)
        {
            return string.Empty;
        }

        ProductMetadata metadata = product.metadata;
        return $"{metadata.isoCurrencyCode} {metadata.localizedPrice:0.##}";
    }

    public void Buy(string productId, Action<bool> callback = null)
    {
        Product product = storeController.GetProductById(productId);
        if (product == null || !product.availableToPurchase)
        {
            Debug.LogError($"[IAP] 구매할 수 없는 상품 : {productId}");
            callback?.Invoke(false);
            return;
        }

        purchaseCallback = callback;
        storeController.PurchaseProduct(product);
    }

    private void HandlePurchasePending(PendingOrder order)
    {
        Product product = order.CartOrdered.Items().FirstOrDefault()?.Product;

        // 앱이 재시작되면 확정되지 않은 구매가 다시 전달될 수 있어 중복 지급을 막는다
        if (processedOrderIds.Add(order.Info.TransactionID))
        {
            OnPurchaseSuccess?.Invoke(product);
        }

        storeController.ConfirmPurchase(order);

        InvokePurchaseCallback(true);
    }

    private void HandlePurchaseFailed(FailedOrder failedOrder)
    {
        Product product = failedOrder.CartOrdered.Items().FirstOrDefault()?.Product;
        Debug.LogError($"[IAP] 구매 실패 : {failedOrder.FailureReason} - {failedOrder.Details}");
        OnPurchaseFailed?.Invoke(product, failedOrder.Details);

        InvokePurchaseCallback(false);
    }

    private void InvokePurchaseCallback(bool success)
    {
        Action<bool> callback = purchaseCallback;
        purchaseCallback = null;
        callback?.Invoke(success);
    }

    private void HandlePurchaseDeferred(DeferredOrder order)
    {
        Product product = order.CartOrdered.Items().FirstOrDefault()?.Product;
        Debug.Log("[IAP] 구매 승인 대기 중 (부모 승인 등)");
        OnPurchaseDeferred?.Invoke(product);
    }

    public void RestorePurchases()
    {
        storeController.RestoreTransactions((success, error) =>
        {
            if (success)
            {
                Debug.Log("[IAP] 구매 복원 완료");
            }
            else
            {
                Debug.LogError($"[IAP] 구매 복원 실패 : {error}");
            }
        });
    }
}
