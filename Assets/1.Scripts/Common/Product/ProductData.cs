public class ProductData
{
    public string key;
    public string productId; //인앱 결제용
    public int price;
    public string GetPriceToString()
    {
        if (!string.IsNullOrEmpty(productId))
        {
            string formattedPrice = InAppPurchaseManager.Instance.GetFormattedPrice(productId);
            if (!string.IsNullOrEmpty(formattedPrice))
            {
                return formattedPrice;
            }
        }
        return price.ToString();
    }
}
