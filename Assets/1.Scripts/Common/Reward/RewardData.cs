

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
            case RewardType.Slime:
                return SlimeManager.Instance.GetSlimeData(value).Title;
        }
        return value;
    }
    public Sprite GetThum()
    {
        switch (rewardType)
        {
            case RewardType.Slime:
                return SlimeManager.Instance.GetSlimeData(value).thum;
            case RewardType.Gold:
                return Resources.Load<Sprite>("Icons/Gold");
            case RewardType.Dia:
                return Resources.Load<Sprite>("Icons/Dia");
            case RewardType.Energe:
                return Resources.Load<Sprite>("Icons/Energe");
            case RewardType.Exp:
                return Resources.Load<Sprite>("Icons/Exp");
            case RewardType.DrawTicket:
                return Resources.Load<Sprite>("Icons/DrawTicket");
        }
        return null;
    }
    public void Receive()
    {
        switch (rewardType)
        {
            case RewardType.Slime:
                RewardCanvas.Instance.OpenCanvas(this);
                UserDataManager.Instance.userSlimeManager.AddUserSlime(value);
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
    Slime,
    Gold,
    Dia,
    Energe,
    Exp,
    DrawTicket
}