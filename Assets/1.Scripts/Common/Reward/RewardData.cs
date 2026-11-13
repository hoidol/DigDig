

using UnityEngine;

[System.Serializable]
public class RewardData
{
    public RewardType rewardType;
    public string value;

    public string GetValueToString()
    {
        switch (rewardType)
        {
            case RewardType.Minion:
                return MinionManager.Instance.GetMinionData(value).Title;
        }
        return value;
    }
    public Sprite GetThum()
    {
        switch (rewardType)
        {
            case RewardType.Minion:
                return MinionManager.Instance.GetMinionData(value).thum;
            case RewardType.Gold:
                return Resources.Load<Sprite>("Icons/Gold");
            // 아래 재화는 Resources/Icons/Currency 아이콘 (이전 경로 Icons/Dia 등은 파일이 없었음)
            case RewardType.Dia:
                return CurrencyInfo.GetSprite(CurrencyType.Dia);
            case RewardType.Energe:
                return Resources.Load<Sprite>("Icons/Currency/Energy");
            case RewardType.Exp:
                return Resources.Load<Sprite>("Icons/Exp");
            case RewardType.DrawTicket:
                return CurrencyInfo.GetSprite(CurrencyType.DrawTicket);
        }
        return null;
    }
    public void Receive()
    {
        switch (rewardType)
        {
            case RewardType.Minion:
                RewardCanvas.Instance.OpenCanvas(this);
                UserDataManager.Instance.userMinionManager.AddUserMinion(value);
                break;
            case RewardType.Gold:
                UserDataManager.Instance.AddGold(int.Parse(value));
                break;
            case RewardType.Dia:
                UserDataManager.Instance.AddDia(int.Parse(value));
                break;
            case RewardType.Energe:
                UserDataManager.Instance.AddEnerge(int.Parse(value));
                break;
            case RewardType.Exp:
                UserDataManager.Instance.userMyInfoManager.AddExp(int.Parse(value));
                break;
            case RewardType.DrawTicket:
                UserDataManager.Instance.AddDrawTicket(int.Parse(value));
                break;
        }

    }
}
public class UserReward
{

    public string id;
    public bool gotten;
}
public enum RewardType
{
    Minion,
    Gold,
    Dia,
    Energe,
    Exp,
    DrawTicket
}