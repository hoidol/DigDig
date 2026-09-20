

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
        }
        return null;
    }
    public void Receive()
    {
        switch (rewardType)
        {
            case RewardType.Slime:
                RewardCanvas.Instance.OpenCanvas(this);
                UserManager.Instance.userSlimeManager.AddUserSlime(value);
                break;

            case RewardType.Gold:
                UserManager.Instance.AddGold(int.Parse(value));
                break;
            case RewardType.Dia:
                UserManager.Instance.AddDia(int.Parse(value));
                break;
            case RewardType.Energe:
                UserManager.Instance.AddEnerge(int.Parse(value));
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
    Energe
}