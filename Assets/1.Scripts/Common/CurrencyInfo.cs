using UnityEngine;

public static class CurrencyInfo
{
    public static Sprite GetSprite(CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.Energe:
                return Resources.Load<Sprite>("Icons/Currency/Energy");
            case CurrencyType.Ad:
                return Resources.Load<Sprite>("Icons/Currency/Ad");
            case CurrencyType.InApp:
                return Resources.Load<Sprite>("Icons/Currency/InApp");
            default:
                return null;
        }
    }
}