using TMPro;
using UnityEngine;

public class ProductPurchaseButton : MonoBehaviour
{
    public TMP_Text inAppPurchasePriceText;
    public CurrencyDisplayPanel currencyDisplayPanel;
    

    public void SetProductData(ProductData productData)
    {
        inAppPurchasePriceText?.gameObject.SetActive(false);
        currencyDisplayPanel?.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(productData.productId))
        {
            if(inAppPurchasePriceText != null)
            {
                inAppPurchasePriceText.gameObject.SetActive(true);
                inAppPurchasePriceText.text = productData.GetPriceToString();
            }
        }
        else
        {
            if(currencyDisplayPanel != null)
            {
                currencyDisplayPanel.gameObject.SetActive(true);
                currencyDisplayPanel.currencyImage.sprite = CurrencyInfo.GetSprite(productData.currencyType);
                currencyDisplayPanel.valueText.text = productData.GetPriceToString();    
            }
            
        }        
    }
    
}
