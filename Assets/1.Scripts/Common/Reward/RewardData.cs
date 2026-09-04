

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
        return null;
    }
    public Sprite GetThum()
    {
        switch (rewardType)
        {
            case RewardType.Slime:
                return SlimeManager.Instance.GetSlimeData(value).thum;
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
    Slime
}