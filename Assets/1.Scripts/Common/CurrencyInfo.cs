using UnityEngine;

public static class CurrencyInfo
{
    public static Sprite GetSprite(CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.Gold:
                return Resources.Load<Sprite>("Icons/Currency/Gold");
            case CurrencyType.Ad:
                return Resources.Load<Sprite>("Icons/Currency/Ad");
            case CurrencyType.InApp:
                return Resources.Load<Sprite>("Icons/Currency/InApp");
            case CurrencyType.Dia:
                return Resources.Load<Sprite>("Icons/Currency/Dia");
            case CurrencyType.DrawTicket:
                return Resources.Load<Sprite>("Icons/Currency/DrawTicket");
            default:
                return null;
        }
    }
}