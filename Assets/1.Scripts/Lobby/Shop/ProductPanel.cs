using TMPro;
using UnityEngine;

public class ProductPanel : MonoBehaviour 
{
    public string key;
    public ProductData productData;
    public TMP_Text titleText;
    public TMP_Text priceText;
    public virtual void OpenPanel()
    {
        productData = ProductManager.Instance.GetProductData(key);
        if(productData == null)
            gameObject.SetActive(false);

        gameObject.SetActive(true);
        titleText.text = productData.key;
        priceText.text = productData.GetPriceToString();
        
    }
    public virtual void UpdatePanel()
    {
        
    }

    public virtual void OnClickedPurchase()
    {
        BlockCanvas.Instance.OpenCanvas("구매중...");
        ProductManager.Instance.TryToBuy(productData, (success) =>
        {
            if (success)
            {
                Purchased();
            }
            BlockCanvas.Instance.CloseCanvas();
        });
    }
    public virtual void Purchased()
    {
        
    }
    
}